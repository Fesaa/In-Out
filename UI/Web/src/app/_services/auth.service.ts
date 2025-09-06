import {computed, effect, inject, Injectable, Signal, signal} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {environment} from '../../environments/environment';
import {toObservable} from '@angular/core/rxjs-interop';
import {catchError, map, of, switchMap, tap} from 'rxjs';
import {AllLanguages, User} from '../_models/user';
import {TranslocoService} from '@jsverse/transloco';

export enum Role {
  CreateForOthers = 'CreateForOthers',
  HandleDeliveries = 'HandleDeliveries',
  ViewAllDeliveries = 'ViewAllDeliveries',

  ManageStock = 'ManageStock',
  ManageProducts = 'ManageProducts',
  ManageClients = 'ManageClients',
  ManageApplication = 'ManageApplication',
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly httpClient = inject(HttpClient);
  private readonly transLoco = inject(TranslocoService);

  private readonly baseUrl = environment.apiUrl;

  private readonly _loaded = signal(false);
  public readonly loaded = this._loaded.asReadonly();
  public readonly loaded$ = toObservable(this.loaded);

  public readonly roles: Signal<Role[]> = computed(() => {
    const userInfo = this.user();
    if (!userInfo) return [];

    return userInfo.roles;
  });

  public readonly isAuthenticated= computed((): boolean => {
    return this.user() !== null;
  });

  // There is always a user, as we load it before the app inits
  private readonly _user = signal<User>(null!);
  public readonly user = this._user.asReadonly();

  constructor() {
    effect(() => {
      const user = this._user();
      if (user == null) return;

      const language = user.language || 'en';
      if (!AllLanguages.includes(language)) return;

      this.transLoco.setActiveLang(language);
      this.transLoco.load(language).subscribe();
    });
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
            this._user.set(user);
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

  update(user: User) {
    return this.httpClient.post<User>(`${this.baseUrl}user/`, user).pipe(
      tap(user => this._user.set(user)),
    )
  }

}
