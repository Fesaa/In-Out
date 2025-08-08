import {ChangeDetectionStrategy, Component, inject, model, OnInit, signal} from '@angular/core';
import {NgbActiveModal} from '@ng-bootstrap/ng-bootstrap';
import {StockService} from '../../../_services/stock.service';
import {Stock} from '../../../_models/stock';
import {TranslocoDirective} from '@jsverse/transloco';
import {SettingsItemComponent} from '../../../shared/components/settings-item/settings-item.component';
import {FormControl, FormGroup, NonNullableFormBuilder, ReactiveFormsModule, Validators} from '@angular/forms';
import {LoadingSpinnerComponent} from '../../../shared/components/loading-spinner/loading-spinner.component';
import {Product} from '../../../_models/product';
import {ProductService} from '../../../_services/product.service';
import {TypeaheadComponent, TypeaheadSettings} from '../../../type-ahead/typeahead.component';
import {of} from 'rxjs';
import {DefaultValuePipe} from '../../../_pipes/default-value.pipe';
import {ToastrService} from 'ngx-toastr';

@Component({
  selector: 'app-edit-stock-modal',
  imports: [
    TranslocoDirective,
    SettingsItemComponent,
    ReactiveFormsModule,
    TypeaheadComponent,
    DefaultValuePipe,
    LoadingSpinnerComponent
  ],
  templateUrl: './edit-stock-modal.component.html',
  styleUrl: './edit-stock-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditStockModalComponent implements OnInit {

  private readonly modal = inject(NgbActiveModal);
  private readonly stockService = inject(StockService);
  private readonly productService = inject(ProductService);
  private readonly fb = inject(NonNullableFormBuilder);

  stock = model.required<Stock>();
  products = signal<Product[]>([]);
  saving = signal(false);

  stockForm!: FormGroup<{
    name: FormControl<string>,
    description: FormControl<string>,
    product: FormControl<Product>,
  }>;
  productTypeaheadSettings!: TypeaheadSettings<Product>;

  close() {
    this.modal.close();
  }

  save() {
    const stock = this.stock();

    const data: Stock = this.stockForm.value as Stock;
    stock.name = data.name;
    stock.description = data.description;

    this.saving.set(true);
    this.stockService.update(stock).subscribe({
      next: data => {
        this.close();
      },
      error: error => {
        console.log(error);
      }
    }).add(() => this.saving.set(false));
  }

  ngOnInit(): void {
    this.productService.allProducts(true).subscribe((products) => {
      this.products.set(products);
    });

    const stock = this.stock();

    this.productTypeaheadSettings = new TypeaheadSettings<Product>();
    this.productTypeaheadSettings.savedData = stock.product;

    this.stockForm = this.fb.group({
      name: this.fb.control(stock.name, [Validators.required]),
      description: this.fb.control(stock.description),
      product: this.fb.control(stock.product, [Validators.required]),
    });
  }

}
