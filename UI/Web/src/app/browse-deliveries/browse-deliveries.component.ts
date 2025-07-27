import {ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal} from '@angular/core';
import {FormArray, FormControl, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {
  AllFilterComparisons,
  AllFilterFields, deserializeFilterFromQuery,
  Filter,
  FilterCombination,
  FilterComparison,
  FilterField, FilterInputType,
  serializeFilterToQuery,
  SortField
} from '../_models/filter';
import {ActivatedRoute, Router} from '@angular/router';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {catchError, debounceTime, distinctUntilChanged, of, tap} from 'rxjs';
import {Delivery} from '../_models/delivery';
import {DeliveryService} from '../_services/delivery.service';
import {TableComponent} from '../shared/components/table/table.component';
import {translate, TranslocoDirective} from '@jsverse/transloco';
import {DeliveryStatePipe} from '../_pipes/delivery-state-pipe';
import {BadgeComponent} from '../shared/components/badge/badge.component';
import {ToastrService} from 'ngx-toastr';
import {FilterComponent} from '../filter/filter.component';
import {NavBarComponent} from '../nav-bar/nav-bar.component';

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
  ],
  templateUrl: './browse-deliveries.component.html',
  styleUrl: './browse-deliveries.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BrowseDeliveriesComponent {

  private readonly deliveryService = inject(DeliveryService);
  private readonly toastr = inject(ToastrService);

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
