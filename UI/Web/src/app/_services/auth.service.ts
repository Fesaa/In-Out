import {computed, inject, Injectable, Signal, signal} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment';
import {toObservable} from '@angular/core/rxjs-interop';
import {catchError, map, of, switchMap, tap} from 'rxjs';
import {User} from '../_models/user';

export enum Role {
  CreateForOthers = 'CreateForOthers',
  HandleDeliveries = 'HandleDeliveries',
  ViewAllDeliveries = 'ViewAllDeliveries',

  ManageStock = 'ManageStock',
  ManageProducts = 'ManageProducts',
  ManageClients = 'ManageClients',
  ManageApplication = 'ManageApplication',
}

export type UserInfo = {
  UserName: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly httpClient = inject(HttpClient);

  private readonly baseUrl = environment.apiUrl;

  private readonly _loaded = signal(false);
  public readonly loaded = this._loaded.asReadonly();
  public readonly loaded$ = toObservable(this.loaded);

  public readonly roles: Signal<Role[]> = computed(() => {
    const userInfo = this.userInfo();
    if (!userInfo) return [];

    return userInfo.roles;
  });

  public readonly isAuthenticated= computed((): boolean => {
    return this.userInfo() !== undefined;
  });

  private readonly _userInfo = signal<User | undefined>(undefined);
  public readonly userInfo = this._userInfo.asReadonly();

  constructor() {
  }

  loadUser() {
    return this.httpClient.get(this.baseUrl + "user/has-cookie", {responseType: "text"}).pipe(
      map(t => t === 'true'),
      switchMap(hasCookie => {
        if (!hasCookie) {
          return of(false);
        }

        return this.httpClient.get<User>(this.baseUrl + 'user/').pipe(
          tap(user => {
            this._userInfo.set(user);
            this._loaded.set(true);
          }),
          map(() => true),
        );
      })
    );
  }

  logout() {
    window.location.href = "/Auth/logout";
  }

  private decodeJwt(token: string) {
    const payload = token.split('.')[1];
    const decoded = atob(payload);
    return JSON.parse(decoded);
  }

}
