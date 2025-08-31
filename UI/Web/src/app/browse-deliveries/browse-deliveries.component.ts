import {ChangeDetectionStrategy, Component, inject, input, OnInit, signal} from '@angular/core';
import {ReactiveFormsModule} from '@angular/forms';
import {Filter,} from '../_models/filter';
import {Delivery, DeliveryState} from '../_models/delivery';
import {DeliveryService} from '../_services/delivery.service';
import {TableComponent} from '../shared/components/table/table.component';
import {translate, TranslocoDirective} from '@jsverse/transloco';
import {DeliveryStatePipe} from '../_pipes/delivery-state-pipe';
import {BadgeColour, BadgeComponent} from '../shared/components/badge/badge.component';
import {ToastrService} from 'ngx-toastr';
import {FilterComponent} from '../filter/filter.component';
import {UtcToLocalTimePipe} from '../_pipes/utc-to-local-time.pipe';
import {NgbTooltip} from '@ng-bootstrap/ng-bootstrap';
import {ModalService} from '../_services/modal.service';
import {
  TransitionDeliveryModalComponent
} from './_components/transition-delivery-modal/transition-delivery-modal.component';
import {DefaultModalOptions} from '../_models/default-modal-options';
import {tap} from 'rxjs';
import {ViewDeliveryModalComponent} from './_components/view-delivery-modal/view-delivery-modal.component';
import {Product, ProductCategory} from '../_models/product';
import {ProductService} from '../_services/product.service';
import {Tracker} from '../shared/tracker';
import {ExportService} from '../_services/export.service';
import {ExportKind} from '../_models/export';

@Component({
  selector: 'app-browse-deliveries',
  imports: [
    TableComponent,
    TranslocoDirective,
    ReactiveFormsModule,
    DeliveryStatePipe,
    BadgeComponent,
    FilterComponent,
    UtcToLocalTimePipe,
    NgbTooltip,
  ],
  templateUrl: './browse-deliveries.component.html',
  styleUrl: './browse-deliveries.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BrowseDeliveriesComponent implements OnInit {

  private readonly deliveryService = inject(DeliveryService);
  private readonly productService = inject(ProductService);
  private readonly toastr = inject(ToastrService);
  private readonly modalService = inject(ModalService);
  private readonly exportService = inject(ExportService);

  showNavbar = input(true);

  deliveries = signal<Delivery[]>([]);
  products = signal<Product[]>([]);
  categories = signal<ProductCategory[]>([]);
  tracker = new Tracker<Delivery, number>((d) => d.id)

  ngOnInit() {
    this.productService.allProducts().subscribe(products => {
      this.products.set(products);
    });
    this.productService.getCategories().subscribe(categories => {
      this.categories.set(categories);
    });
  }

  loadFilter(filter: Filter) {
    this.deliveryService.filter(filter).subscribe({
      next: (data: Delivery[]) => {
        this.deliveries.set(data);
        this.tracker.reset();
      },
      error: (err) => {
        console.log(err);
        this.toastr.error(err, translate('errors.filter-fail'));
      }
    });
  }

  trackDelivery(idx: number, d: Delivery) {
    return `${d.id}`
  }

  transitionDelivery(delivery: Delivery) {
    const [modal, component] = this.modalService.open(TransitionDeliveryModalComponent, DefaultModalOptions);
    component.delivery.set(delivery);

    modal.closed.pipe(
      tap((nextState: DeliveryState | undefined) => {
        if (nextState === undefined) return;

        this.deliveries.update(deliveries => deliveries.map(d => {
          if (d.id === delivery.id) {
            d.state = nextState;
          }

          return d;
        }));
      })
    ).subscribe();
  }

  stateBadgeColour(state: DeliveryState): BadgeColour {
    switch (state) {
      case DeliveryState.InProgress:
      case DeliveryState.Completed:
        return 'secondary'
      case DeliveryState.Cancelled:
        return "error"
      case DeliveryState.Handled:
        return 'primary'
    }
  }

  showInfo(delivery: Delivery) {
    const [_, component] = this.modalService.open(ViewDeliveryModalComponent, DefaultModalOptions);
    component.delivery.set(delivery);
    component.products.set(this.products());
    component.categories.set(this.categories());
  }

  export() {
    this.exportService.export({
      kind: ExportKind.Csv,
      deliveryIds: this.tracker.ids(),
    }).subscribe((id) => {
      window.open(`/api/export/${id}`, '_blank');
      this.tracker.reset();
    });
  }
}
