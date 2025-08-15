import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment';
import {Delivery, DeliveryState} from '../_models/delivery';
import {Filter} from '../_models/filter';

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
    return this.httpClient.post<Delivery>(`${this.baseUrl}/`, data);
  }

  updateDelivery(data: Delivery) {
    return this.httpClient.put<Delivery>(`${this.baseUrl}/`, data);
  }

  deleteDelivery(id: number) {
    return this.httpClient.delete<Delivery>(`${this.baseUrl}/${id}`);
  }

  filter(filter: Filter) {
    return this.httpClient.post<Delivery[]>(this.baseUrl+'/filter', filter);
  }

  transitionDelivery(id: number, nextState: DeliveryState) {
    return this.httpClient.post(`${this.baseUrl}/transition?deliveryId=${id}&nextState=${nextState}`, {})
  }

}
