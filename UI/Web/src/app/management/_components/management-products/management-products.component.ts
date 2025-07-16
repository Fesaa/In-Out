import {ChangeDetectionStrategy, Component, computed, inject, OnInit, signal} from '@angular/core';
import {ProductService} from '../../../_services/product.service';
import {Product, ProductCategory} from '../../../_models/product';
import {forkJoin} from 'rxjs';
import {TableComponent} from '../../../shared/components/table/table.component';
import {TranslocoDirective} from '@jsverse/transloco';
import {NgbModal} from '@ng-bootstrap/ng-bootstrap';
import {ProductCategoryModalComponent} from './_components/product-category-modal/product-category-modal.component';
import {DefaultModalOptions} from '../../../_models/default-modal-options';
import {LoadingSpinnerComponent} from '../../../shared/components/loading-spinner/loading-spinner.component';
import {ProductModalComponent} from './_components/product-modal/product-modal.component';
import {ModalService} from '../../../_services/modal.service';

@Component({
  selector: 'app-management-products',
  imports: [
    TableComponent,
    TranslocoDirective,
    LoadingSpinnerComponent
  ],
  templateUrl: './management-products.component.html',
  styleUrl: './management-products.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManagementProductsComponent implements OnInit {

  private readonly modalService = inject(ModalService);
  private readonly productService = inject(ProductService);

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

  category(product: Product): ProductCategory | undefined {
    return this.categories().find(c => c.id === product.categoryId);
  }

  productTracker(idx: number, product: Product): string {
    return `${product.id}`;
  }

  createOrUpdateCategory(category?: ProductCategory) {
    const [modal, component] = this.modalService.open(ProductCategoryModalComponent, DefaultModalOptions);
    component.existingCategories.set(this.categories());
    component.firstRun.set(this.categories().length === 0);
    if (category) {
      component.category.set(category);
    }

    modal.closed.subscribe(() => {
      this.productService.getCategories().subscribe(categories => {
        this.categories.set(categories);
      });
    });
  }

  createOrUpdateProduct(product?: Product) {
    const [modal, component] = this.modalService.open(ProductModalComponent, DefaultModalOptions);
    component.categories.set(this.categories());
    if (product) {
      component.product.set(product);
    }

    modal.closed.subscribe(() => {
      this.productService.allProducts().subscribe(products => {
        this.products.set(products);
      })
    })

  }
}
