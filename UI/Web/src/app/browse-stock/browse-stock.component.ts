import {ChangeDetectionStrategy, Component, computed, inject, OnInit, signal} from '@angular/core';
import {StockService} from '../_services/stock.service';
import {Stock} from '../_models/stock';
import {BadgeComponent} from '../shared/components/badge/badge.component';
import {TableComponent} from '../shared/components/table/table.component';
import {TranslocoDirective} from '@jsverse/transloco';
import {NavBarComponent} from '../nav-bar/nav-bar.component';
import {ModalService} from '../_services/modal.service';
import {StockHistoryModalComponent} from './_components/stock-history-modal/stock-history-modal.component';
import {DefaultModalOptions} from '../_models/default-modal-options';
import {EditStockModalComponent} from './_components/edit-stock-modal/edit-stock-modal.component';

@Component({
  selector: 'app-browse-stock',
  imports: [
    BadgeComponent,
    TableComponent,
    TranslocoDirective,
    NavBarComponent,
  ],
  templateUrl: './browse-stock.component.html',
  styleUrl: './browse-stock.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BrowseStockComponent implements OnInit {

  private readonly stockService = inject(StockService);
  private readonly modalService = inject(ModalService);

  stock = signal<Stock[]>([]);

  sortedStock = computed(() => this.stock().sort((a, b) => {
    if (a.product.isTracked && !b.product.isTracked) return -1;
    if (a.product.isTracked && !b.product.isTracked) return 1;

    return a.name.localeCompare(b.name);
  }));

  ngOnInit(): void {
    this.stockService.getAll().subscribe(stocks => {
      this.stock.set(stocks);
    })
  }

  trackStock(idx: number, stock: Stock) {
    return `${stock.id}`
  }

  viewHistory(stock: Stock) {
    const [modal, component] = this.modalService.open(StockHistoryModalComponent, DefaultModalOptions);
    component.stock.set(stock);
  }

  editStock(stock: Stock) {
    const [modal, component] = this.modalService.open(EditStockModalComponent, DefaultModalOptions);
    component.stock.set(stock);
  }



}
