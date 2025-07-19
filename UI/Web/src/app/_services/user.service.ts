import {inject, Injectable} from '@angular/core';
import {environment} from '../../environments/environment';
import {HttpClient} from '@angular/common/http';
import {User} from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private readonly httpClient = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl + 'user';

  currentUser() {
    return this.httpClient.get<User>(this.baseUrl);
  }

  search(query: string) {
    return this.httpClient.get<User[]>(this.baseUrl + '/search?query=' + query);
  }

}
