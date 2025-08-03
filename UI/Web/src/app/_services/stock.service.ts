import {inject, Injectable} from '@angular/core';
import {environment} from '../../environments/environment';
import {HttpClient} from '@angular/common/http';
import {Stock} from '../_models/stock';

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
    return this.http.get(`${this.baseUrl}/history/${stockId}`);
  }

}
