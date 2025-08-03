import {ChangeDetectionStrategy, Component, inject, OnInit, signal} from '@angular/core';
import {StockService} from '../_services/stock.service';
import {Stock} from '../_models/stock';
import {BadgeComponent} from '../shared/components/badge/badge.component';
import {TableComponent} from '../shared/components/table/table.component';
import {TranslocoDirective} from '@jsverse/transloco';
import {NavBarComponent} from '../nav-bar/nav-bar.component';

@Component({
  selector: 'app-browse-stock',
  imports: [
    BadgeComponent,
    TableComponent,
    TranslocoDirective,
    NavBarComponent
  ],
  templateUrl: './browse-stock.component.html',
  styleUrl: './browse-stock.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BrowseStockComponent implements OnInit {

  private readonly stockService = inject(StockService);

  stock = signal<Stock[]>([]);

  ngOnInit(): void {
    this.stockService.getAll().subscribe(stocks => {
      this.stock.set(stocks);
    })
  }

  trackStock(idx: number, stock: Stock) {
    return `${stock.id}`
  }



}
