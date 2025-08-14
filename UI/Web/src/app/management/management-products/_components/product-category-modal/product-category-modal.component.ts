import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  DestroyRef,
  inject,
  input,
  model,
  OnInit, signal
} from '@angular/core';
import {ProductCategory} from '../../../../_models/product';
import {NgbActiveModal} from '@ng-bootstrap/ng-bootstrap';
import {ProductService} from '../../../../_services/product.service';
import {TranslocoDirective} from '@jsverse/transloco';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {SettingsItemComponent} from '../../../../shared/components/settings-item/settings-item.component';
import {LoadingSpinnerComponent} from '../../../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-product-category-modal',
  imports: [
    TranslocoDirective,
    ReactiveFormsModule,
    SettingsItemComponent,
    LoadingSpinnerComponent
  ],
  templateUrl: './product-category-modal.component.html',
  styleUrl: './product-category-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductCategoryModalComponent implements OnInit {

  private readonly productService = inject(ProductService);
  private readonly cdRef = inject(ChangeDetectorRef);
  protected readonly modal = inject(NgbActiveModal);

  existingCategories = model.required<ProductCategory[]>();
  firstRun = model.required<boolean>();
  category = model<ProductCategory>({
    id: -1,
    name: 'Your new category',
    sortValue: 0,
    autoCollapse: false,
    enabled: true,
  });

  isSaving = signal(false);

  categoryForm: FormGroup = new FormGroup({});

  close() {
    this.modal.close();
  }

  save() {
    const formValue = this.categoryForm.value as ProductCategory;
    const { id, sortValue } = this.category();
    const category: ProductCategory = { ...formValue, id, sortValue };

    const action$ = id === -1
      ? this.productService.createCategory(category)
      : this.productService.updateCategory(category);

    action$.subscribe({
      next: () => this.close(),
      error: err => console.error(err)
    });
  }

  ngOnInit(): void {
    const category = this.category();
    this.categoryForm.addControl('name', new FormControl(category.name, [Validators.required]));
    this.categoryForm.addControl('autoCollapse', new FormControl(category.autoCollapse, []));
    this.categoryForm.addControl('enabled', new FormControl(category.enabled, []));

    this.cdRef.markForCheck();
  }

}
