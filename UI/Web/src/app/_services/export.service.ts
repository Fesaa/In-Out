import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment';
import {ExportRequest} from '../_models/export';

@Injectable({
  providedIn: 'root'
})
export class ExportService {

  private readonly httpClient = inject(HttpClient);

  private readonly baseUrl = environment.apiUrl;

  export(req: ExportRequest) {
    return this.httpClient.post(this.baseUrl + "export", req, {responseType: 'text'})
  }

}
