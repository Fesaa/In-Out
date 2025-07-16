import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
  TemplateRef,
  ViewChild
} from '@angular/core';
import {ProductService} from '../../../_services/product.service';
import {Product, ProductCategory} from '../../../_models/product';
import {forkJoin} from 'rxjs';
import {TableComponent} from '../../../shared/components/table/table.component';
import {translate, TranslocoDirective} from '@jsverse/transloco';
import {ProductCategoryModalComponent} from './_components/product-category-modal/product-category-modal.component';
import {DefaultModalOptions} from '../../../_models/default-modal-options';
import {LoadingSpinnerComponent} from '../../../shared/components/loading-spinner/loading-spinner.component';
import {ProductModalComponent} from './_components/product-modal/product-modal.component';
import {ModalService} from '../../../_services/modal.service';
import {CdkDragDrop, CdkDragHandle, moveItemInArray} from '@angular/cdk/drag-drop';

@Component({
  selector: 'app-management-products',
  imports: [
    TableComponent,
    TranslocoDirective,
    LoadingSpinnerComponent,
    CdkDragHandle
  ],
  templateUrl: './management-products.component.html',
  styleUrl: './management-products.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManagementProductsComponent implements OnInit {

  private readonly modalService = inject(ModalService);
  private readonly productService = inject(ProductService);

  @ViewChild("deleteProductConfirm") deleteProductConfirm!: TemplateRef<any>;
  @ViewChild("deleteCategoryConfirm") deleteCategoryConfirm!: TemplateRef<any>;

  loading = signal(true);
  products = signal<Product[]>([]);
  categories = signal<ProductCategory[]>([]);

  productWarning = computed(() => this.products().length === 0 && this.categories().length > 0)
  categoriesWarning = computed(() => this.categories().length === 0);

  ngOnInit(): void {
    forkJoin([
      this.productService.allProducts(),
      this.productService.getCategories()
    ]).subscribe({
      next: ([products, categories]) => {
        this.products.set(products);
        this.categories.set(categories);
        this.loading.set(false);
      },
      error: error => {
        console.log(error);
      }
    });
  }

  loadCategories(): void {
    this.productService.getCategories().subscribe(categories => {
      this.categories.set(categories);
    });
  }

  loadProducts(): void {
    this.productService.allProducts().subscribe(products => {
      this.products.set(products);
    });
  }

  category(product: Product): ProductCategory | undefined {
    return this.categories().find(c => c.id === product.categoryId);
  }

  productTracker(idx: number, product: Product): string {
    return `${product.id}`;
  }

  categoryTracker(idx: number, category: ProductCategory): string {
    return `${category.id}`;
  }

  createOrUpdateCategory(category?: ProductCategory) {
    const [modal, component] = this.modalService.open(ProductCategoryModalComponent, DefaultModalOptions);
    component.existingCategories.set(this.categories());
    component.firstRun.set(this.categories().length === 0);
    if (category) {
      component.category.set(category);
    }

    modal.closed.subscribe(() => this.loadCategories());
  }

  createOrUpdateProduct(product?: Product) {
    const [modal, component] = this.modalService.open(ProductModalComponent, DefaultModalOptions);
    component.categories.set(this.categories());
    if (product) {
      component.product.set(product);
    }

    modal.closed.subscribe(() => this.loadProducts());
  }

  async deleteProduct(product: Product) {
    if (!await this.modalService.confirm({bodyTemplate: this.deleteProductConfirm, templateData: product})) {
      return;
    }

    this.productService.deleteProduct(product.id).subscribe({next: () => this.loadProducts()});
  }

  async deleteCategory(category: ProductCategory) {
    if (!await this.modalService.confirm({bodyTemplate: this.deleteCategoryConfirm, templateData: category})) {
      return;
    }

    this.productService.deleteCategory(category.id).subscribe({next: () => this.loadProducts()});
  }

  newDefaultCategory(category: ProductCategory) {
    const others = this.categories().filter(c => c.id !== category.id);
    return others[0];
  }

  categorySize(category: ProductCategory): number {
    const products = this.products().filter(c => c.categoryId === category.id);
    return products.length;
  }

  onCategoryDrop(event: CdkDragDrop<ProductCategory[]>) {
    const oldCopy = [...this.categories()];

    const data = [...event.container.data];
    moveItemInArray(data, event.previousIndex, event.currentIndex);
    this.categories.set(data);

    const ids = data.map(c => c.id);
    this.productService.orderCategories(ids).subscribe({
      next: () => {
        this.loadCategories();
      },
      error: error => {
        console.error(error);
        this.categories.set(oldCopy);
      }
    })
  }
}
