import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { Observable } from 'rxjs';
import {Client} from '../_models/client';

@Injectable({
  providedIn: 'root',
})
export class ClientService {
  private readonly baseUrl = environment.apiUrl + 'client/';
  private readonly http = inject(HttpClient);

  getAll(): Observable<Client[]> {
    return this.http.get<Client[]>(`${this.baseUrl}dto`);
  }

  search(search: string): Observable<Client[]> {
    return this.http.get<Client[]>(`${this.baseUrl}search?query=${search}`);
  }

  getById(id: number): Observable<Client> {
    return this.http.get<Client>(`${this.baseUrl}${id}`);
  }

  getByCompanyNumber(companyNumber: string): Observable<Client> {
    return this.http.get<Client>(`${this.baseUrl}company/${companyNumber}`);
  }

  create(dto: Client): Observable<void> {
    return this.http.post<void>(this.baseUrl, dto);
  }

  update(dto: Client): Observable<void> {
    return this.http.put<void>(this.baseUrl, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}${id}`);
  }
}
