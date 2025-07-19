import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment';
import {Delivery} from '../_models/delivery';

@Injectable({
  providedIn: 'root'
})
export class DeliveryService {

  private readonly httpClient = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl + 'delivery';

  getDelivery(id: number) {
    return this.httpClient.get<Delivery>(`${this.baseUrl}/${id}`);
  }

  createDelivery(data: Delivery) {
    return this.httpClient.post(`${this.baseUrl}/`, data);
  }

  updateDelivery(data: Delivery) {
    return this.httpClient.put(`${this.baseUrl}/`, data);
  }

  deleteDelivery(id: number) {
    return this.httpClient.delete<Delivery>(`${this.baseUrl}/${id}`);
  }

}
