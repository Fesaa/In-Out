import {inject, Injectable} from '@angular/core';
import {environment} from '../../environments/environment';
import {HttpClient} from '@angular/common/http';
import {Stock, StockHistory, UpdateStock} from '../_models/stock';

@Injectable({
  providedIn: 'root'
})
export class StockService {

  private readonly baseUrl = environment.apiUrl + 'stock/';
  private readonly http = inject(HttpClient);

  getAll() {
    return this.http.get<Stock[]>(this.baseUrl);
  }

  getHistory(stockId: number) {
    return this.http.get<StockHistory[]>(`${this.baseUrl}history/${stockId}`);
  }

  update(stock: Stock) {
    return this.http.put(`${this.baseUrl}`, stock);
  }

  doOperation(dto: UpdateStock) {
    return this.http.post(`${this.baseUrl}`, dto);
  }

}
