import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment';
import {Settings} from '../_models/settings';

@Injectable({
  providedIn: 'root'
})
export class SettingsService {

  private readonly httpClient = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  getSettings() {
    return this.httpClient.get<Settings>(this.baseUrl + 'settings');
  }

  updateSettings(settings: Settings) {
    return this.httpClient.post<Settings>(this.baseUrl + 'settings', settings);
  }

}
