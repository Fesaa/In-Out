import {ChangeDetectionStrategy, Component, computed, inject, OnInit, signal} from '@angular/core';
import {StockService} from '../_services/stock.service';
import {Stock} from '../_models/stock';
import {BadgeComponent} from '../shared/components/badge/badge.component';
import {TableComponent} from '../shared/components/table/table.component';
import {TranslocoDirective} from '@jsverse/transloco';
import {ModalService} from '../_services/modal.service';
import {StockHistoryModalComponent} from './_components/stock-history-modal/stock-history-modal.component';
import {DefaultModalOptions} from '../_models/default-modal-options';
import {UpdateStockModalComponent} from './_components/update-stock-modal/update-stock-modal.component';
import {AuthService, Role} from '../_services/auth.service';
import {ProductCategory} from '../_models/product';
import {ProductService} from '../_services/product.service';
import {LoadingSpinnerComponent} from '../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-browse-stock',
  imports: [
    BadgeComponent,
    TableComponent,
    TranslocoDirective,
    LoadingSpinnerComponent,
  ],
  templateUrl: './browse-stock.component.html',
  styleUrl: './browse-stock.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BrowseStockComponent implements OnInit {

  protected readonly authService = inject(AuthService);
  private readonly productService = inject(ProductService);
  private readonly stockService = inject(StockService);
  private readonly modalService = inject(ModalService);

  loading = signal(true);
  stock = signal<Stock[]>([]);
  categories = signal<ProductCategory[]>([])

  sortedStock = computed(() => this.stock()
    .filter(s => s.product.isTracked)
    .sort((a, b) => {
      if (a.product.isTracked && !b.product.isTracked) return -1;
      if (a.product.isTracked && !b.product.isTracked) return 1;

      return a.name.localeCompare(b.name);
    }));

  ngOnInit(): void {
    this.load();

    this.productService.getCategories(false).subscribe(categories => {
      this.categories.set(categories);
    });
  }

  categoryName(id: number) {
    return this.categories().find(c => c.id === id)?.name ?? '';
  }

  private load() {
    this.stockService.getAll().subscribe(stocks => {
      this.stock.set(stocks);
      this.loading.set(false);
    });
  }

  trackStock(idx: number, stock: Stock) {
    return `${stock.id}`
  }

  viewHistory(stock: Stock) {
    const [modal, component] = this.modalService.open(StockHistoryModalComponent, DefaultModalOptions);
    component.stock.set(stock);
  }

  updateStock(stock: Stock) {
    const [modal, component] = this.modalService.open(UpdateStockModalComponent, DefaultModalOptions);
    component.stock.set(stock);

    modal.result.then(() => this.load());
  }


  protected readonly Role = Role;
}
