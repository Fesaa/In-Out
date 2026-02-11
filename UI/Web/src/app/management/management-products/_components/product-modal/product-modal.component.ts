import {ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, model, OnInit, signal} from '@angular/core';
import {AllProductTypes, PriceCategory, Product, ProductCategory, ProductType} from '../../../../_models/product';
import {ProductService} from '../../../../_services/product.service';
import {NgbActiveModal} from '@ng-bootstrap/ng-bootstrap';
import {LoadingSpinnerComponent} from '../../../../shared/components/loading-spinner/loading-spinner.component';
import {TranslocoDirective} from '@jsverse/transloco';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {SettingsItemComponent} from '../../../../shared/components/settings-item/settings-item.component';
import {ProductTypePipe} from '../../../../_pipes/product-type-pipe';
import {DecimalPipe} from '@angular/common';

@Component({
  selector: 'app-product-modal',
  imports: [
    LoadingSpinnerComponent,
    TranslocoDirective,
    ReactiveFormsModule,
    SettingsItemComponent,
    ProductTypePipe,
    DecimalPipe
  ],
  templateUrl: './product-modal.component.html',
  styleUrl: './product-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductModalComponent implements OnInit {

  private readonly productService = inject(ProductService);
  private readonly cdRef = inject(ChangeDetectorRef);
  protected readonly modal = inject(NgbActiveModal);

  categories = model<ProductCategory[]>([]);
  priceCategories = model<PriceCategory[]>([]);

  product = model<Product>({
    id: -1,
    name: 'Your new Product',
    description: '',
    categoryId: -1,
    type: ProductType.Consumable,
    enabled: true,
    isTracked: true,
    sortValue: 0,
    prices: {}
  });

  isSaving = signal(false);

  productForm = new FormGroup({});

  ngOnInit(): void {
    const categories = this.categories();
    const product = this.product();
    if (categories.length === 0) {
      this.close()
      return;
    }

    const selectedCategory =
      categories.find(c => c.id === product.categoryId)?? categories[0];

    this.productForm.addControl('name', new FormControl(product.name, Validators.required));
    this.productForm.addControl('description', new FormControl(product.description));
    this.productForm.addControl('category', new FormControl(selectedCategory.id, Validators.required));
    this.productForm.addControl('type', new FormControl(product.type, Validators.required));
    this.productForm.addControl('enabled', new FormControl(product.enabled, Validators.required));
    this.productForm.addControl('isTracked', new FormControl(product.isTracked, Validators.required));

    const priceGroup = new FormGroup({});
    this.priceCategories().forEach(pc => {
      const initialPrice = product.prices[pc.id.toString()] ?? 0;
      priceGroup.addControl(pc.id.toString(), new FormControl(initialPrice, [Validators.required]));
    });
    this.productForm.addControl('prices', priceGroup);

    this.cdRef.markForCheck();
  }

  save() {
    const formValue: any = this.productForm.value;
    const id = this.product().id;

    const product: Product = {
      id: id,
      name: formValue.name,
      description: formValue.description,
      type: parseInt(formValue.type, 10) as ProductType,
      categoryId: formValue.category,
      enabled: formValue.enabled,
      isTracked: formValue.isTracked,
      sortValue: this.product().sortValue,
      prices: formValue.prices,
    }

    const action$ = id === -1
      ? this.productService.createProduct(product)
      : this.productService.updateProduct(product);

    action$.subscribe({
      next: () => this.close(),
      error: err => console.error(err)
    });
  }

  categoryName(s: string): string {
    const id = parseInt(s, 10);
    return this.categories().find(c => c.id === id)?.name ?? 'unknown';
  }

  close() {
    this.modal.close();
  }

  protected readonly AllProductTypes = AllProductTypes;
}
