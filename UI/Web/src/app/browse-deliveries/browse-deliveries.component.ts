import {ChangeDetectionStrategy, Component, inject, input, signal} from '@angular/core';
import {ReactiveFormsModule} from '@angular/forms';
import {Filter,} from '../_models/filter';
import {Delivery} from '../_models/delivery';
import {DeliveryService} from '../_services/delivery.service';
import {TableComponent} from '../shared/components/table/table.component';
import {translate, TranslocoDirective} from '@jsverse/transloco';
import {DeliveryStatePipe} from '../_pipes/delivery-state-pipe';
import {BadgeComponent} from '../shared/components/badge/badge.component';
import {ToastrService} from 'ngx-toastr';
import {FilterComponent} from '../filter/filter.component';
import {NavBarComponent} from '../nav-bar/nav-bar.component';
import {UtcToLocalTimePipe} from '../_pipes/utc-to-local-time.pipe';

@Component({
  selector: 'app-browse-deliveries',
  imports: [
    TableComponent,
    TranslocoDirective,
    ReactiveFormsModule,
    DeliveryStatePipe,
    BadgeComponent,
    FilterComponent,
    NavBarComponent,
    UtcToLocalTimePipe,
  ],
  templateUrl: './browse-deliveries.component.html',
  styleUrl: './browse-deliveries.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BrowseDeliveriesComponent {

  private readonly deliveryService = inject(DeliveryService);
  private readonly toastr = inject(ToastrService);

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


}
