import {ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, model, signal} from '@angular/core';
import {LoadingSpinnerComponent} from '../../../../shared/components/loading-spinner/loading-spinner.component';
import {ProductService} from '../../../../_services/product.service';
import {NgbActiveModal} from '@ng-bootstrap/ng-bootstrap';
import {Product, ProductCategory} from '../../../../_models/product';
import {TranslocoDirective} from '@jsverse/transloco';
import {TableComponent} from '../../../../shared/components/table/table.component';
import {CdkDragDrop, CdkDragHandle, moveItemInArray} from '@angular/cdk/drag-drop';
import {ToastrService} from 'ngx-toastr';

@Component({
  selector: 'app-sort-products-modal',
  imports: [
    LoadingSpinnerComponent,
    TranslocoDirective,
    TableComponent,
    CdkDragHandle
  ],
  templateUrl: './sort-products-modal.component.html',
  styleUrl: './sort-products-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SortProductsModalComponent {

  private readonly productService = inject(ProductService);
  private readonly toastR = inject(ToastrService);
  protected readonly modal = inject(NgbActiveModal);

  isSaving = signal(false);
  category = model.required<ProductCategory>();
  products = model.required<Product[]>()

  trackById(index: number, p: Product) {
    return `${p.id}`
  }

  close() {
    this.modal.close();
  }

  save() {
    const ids = this.products().map((product) => product.id);
    this.productService.orderProducts(ids).subscribe({
      error: err => {
        this.toastR.error(err.message);
      }
    }).add(() => this.close());
  }

  onProductDrop($event: CdkDragDrop<Product[]>) {
    const current = [...this.products()]; // We require a copy as moveItemInArray moves in place
    moveItemInArray(current, $event.previousIndex, $event.currentIndex);
    this.products.set(current);
  }
}
