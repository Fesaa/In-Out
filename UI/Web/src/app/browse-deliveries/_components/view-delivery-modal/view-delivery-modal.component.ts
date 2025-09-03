import {ChangeDetectionStrategy, Component, computed, inject, model} from '@angular/core';
import {TranslocoDirective} from '@jsverse/transloco';
import {Delivery} from '../../../_models/delivery';
import {NgbActiveModal} from '@ng-bootstrap/ng-bootstrap';
import {DatePipe} from '@angular/common';
import {DeliveryStatePipe} from '../../../_pipes/delivery-state-pipe';
import {Product, ProductCategory, ProductType} from '../../../_models/product';
import {DefaultValuePipe} from '../../../_pipes/default-value.pipe';

@Component({
  selector: 'app-view-delivery-modal',
  imports: [
    TranslocoDirective,
    DatePipe,
    DefaultValuePipe
  ],
  templateUrl: './view-delivery-modal.component.html',
  styleUrl: './view-delivery-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ViewDeliveryModalComponent {

  private readonly modal = inject(NgbActiveModal);

  delivery = model.required<Delivery>();
  products = model.required<Product[]>();
  categories = model.required<ProductCategory[]>();

  protected productLookup = computed(() => {
    const map = new Map<number, Product>();
    this.products().forEach(p => map.set(p.id, p));
    return map;
  });

  protected categoryLookup = computed(() => {
    const map = new Map<number, ProductCategory>();
    this.categories().forEach(c => map.set(c.id, c));
    return map;
  });

  close() {
    this.modal.close();
  }

  protected readonly ProductType = ProductType;
}
