import {ChangeDetectionStrategy, Component, computed, inject, OnInit, signal} from '@angular/core';
import {ProductService} from '../../../_services/product.service';
import {Product, ProductCategory, ProductType} from '../../../_models/product';
import {forkJoin} from 'rxjs';
import {TableComponent} from '../../../shared/components/table/table.component';
import {TranslocoDirective} from '@jsverse/transloco';
import {NgbModal} from '@ng-bootstrap/ng-bootstrap';
import {ProductCategoryModalComponent} from './_components/product-category-modal/product-category-modal.component';
import {DefaultModalOptions} from '../../../_models/default-modal-options';

@Component({
  selector: 'app-management-products',
  imports: [
    TableComponent,
    TranslocoDirective
  ],
  templateUrl: './management-products.component.html',
  styleUrl: './management-products.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManagementProductsComponent implements OnInit {

  private readonly modalService = inject(NgbModal);
  private readonly productService = inject(ProductService);

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

  createCategory() {
    const modal = this.modalService.open(ProductCategoryModalComponent, DefaultModalOptions);
    (modal.componentInstance as ProductCategoryModalComponent).existingCategories.set(this.categories());
    (modal.componentInstance as ProductCategoryModalComponent).firstRun.set(this.categories().length === 0);
    modal.closed.subscribe(() => {
      this.productService.getCategories().subscribe(categories => {
        this.categories.set(categories);
      });
    });
  }

  createProduct() {

  }
}
