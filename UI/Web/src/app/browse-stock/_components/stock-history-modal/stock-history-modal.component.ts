import {ChangeDetectionStrategy, Component, inject, model, OnInit, signal} from '@angular/core';
import {Stock, StockHistory} from '../../../_models/stock';
import {StockService} from '../../../_services/stock.service';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {LoadingSpinnerComponent} from '../../../shared/components/loading-spinner/loading-spinner.component';
import {SettingsItemComponent} from '../../../shared/components/settings-item/settings-item.component';
import {TranslocoDirective} from '@jsverse/transloco';
import {NgbActiveModal} from '@ng-bootstrap/ng-bootstrap';
import {BadgeComponent} from '../../../shared/components/badge/badge.component';
import {TableComponent} from '../../../shared/components/table/table.component';
import {UtcToLocalTimePipe} from '../../../_pipes/utc-to-local-time.pipe';
import {DefaultValuePipe} from '../../../_pipes/default-value.pipe';

@Component({
  selector: 'app-stock-history-modal',
  imports: [
    FormsModule,
    ReactiveFormsModule,
    TranslocoDirective,
    LoadingSpinnerComponent,
    BadgeComponent,
    TableComponent,
    UtcToLocalTimePipe,
    DefaultValuePipe
  ],
  templateUrl: './stock-history-modal.component.html',
  styleUrl: './stock-history-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StockHistoryModalComponent implements OnInit{

  private readonly modal = inject(NgbActiveModal);
  private readonly stockService = inject(StockService);

  stock = model.required<Stock>();
  history = signal<StockHistory[]>([]);
  loading = signal(true);

  ngOnInit(): void {

    this.stockService.getHistory(this.stock().id).subscribe({
      next: (history) => {
        this.history.set(history);
        this.loading.set(false);
      },
      error: (error) => {
        console.error(error);
      }
    })

  }

  trackHistory(idx: number, history: StockHistory){
    return `${history.id}`
  }

  close() {
    this.modal.close();
  }

}
