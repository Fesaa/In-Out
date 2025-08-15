import {ChangeDetectionStrategy, Component, inject, input, signal} from '@angular/core';
import {ReactiveFormsModule} from '@angular/forms';
import {Filter,} from '../_models/filter';
import {Delivery, DeliveryState} from '../_models/delivery';
import {DeliveryService} from '../_services/delivery.service';
import {TableComponent} from '../shared/components/table/table.component';
import {translate, TranslocoDirective} from '@jsverse/transloco';
import {DeliveryStatePipe} from '../_pipes/delivery-state-pipe';
import {BadgeComponent} from '../shared/components/badge/badge.component';
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
export class BrowseDeliveriesComponent {

  private readonly deliveryService = inject(DeliveryService);
  private readonly toastr = inject(ToastrService);
  private readonly modalService = inject(ModalService);

  showNavbar = input(true);

  deliveries = signal<Delivery[]>([]);

  loadFilter(filter: Filter) {
    this.deliveryService.filter(filter).subscribe({
      next: (data: Delivery[]) => {
        this.deliveries.set(data);
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


}
