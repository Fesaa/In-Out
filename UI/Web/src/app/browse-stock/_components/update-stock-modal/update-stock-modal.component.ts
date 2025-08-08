import {ChangeDetectionStrategy, Component, inject, model, OnInit} from '@angular/core';
import {NgbActiveModal} from '@ng-bootstrap/ng-bootstrap';
import {StockService} from '../../../_services/stock.service';
import {AllStocksOperations, Stock, StockOperation, UpdateStock} from '../../../_models/stock';
import {FormControl, FormGroup, NonNullableFormBuilder, ReactiveFormsModule} from '@angular/forms';
import {TranslocoDirective} from '@jsverse/transloco';
import {SettingsItemComponent} from '../../../shared/components/settings-item/settings-item.component';
import {StockOperationPipe} from '../../../_pipes/stock-operation-pipe';
import {DefaultValuePipe} from '../../../_pipes/default-value.pipe';

/**
 * In contrast to the EditStockModalComponent, this is for manually adding to history
 */
@Component({
  selector: 'app-update-stock-modal',
  imports: [
    TranslocoDirective,
    ReactiveFormsModule,
    SettingsItemComponent,
    StockOperationPipe,
    DefaultValuePipe
  ],
  templateUrl: './update-stock-modal.component.html',
  styleUrl: './update-stock-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UpdateStockModalComponent implements OnInit {

  private readonly modal = inject(NgbActiveModal);
  private readonly stockService = inject(StockService);
  private readonly fb = inject(NonNullableFormBuilder);

  stock = model.required<Stock>();

  updateForm!: FormGroup<{
    productId: FormControl<number>,
    operation: FormControl<StockOperation>,
    value: FormControl<number>,
    notes: FormControl<string | null>,
    reference: FormControl<string | null>,
  }>;

  close() {
    this.modal.close();
  }

  save() {
    const dto = this.updateForm.value as UpdateStock;

    this.stockService.doOperation(dto).subscribe({
      next: () => {
        this.close();
      },
      error: err => {
        console.error(err);
      }
    })
  }

  ngOnInit(){
    const stock = this.stock();

    this.updateForm = new FormGroup({
      productId: this.fb.control(stock.productId),
      operation: this.fb.control<StockOperation>(StockOperation.Add),
      value: this.fb.control(0),
      notes: new FormControl<string | null>(null),
      reference: new FormControl<string | null>(null),
    });
  }

  protected readonly AllStocksOperations = AllStocksOperations;
}
