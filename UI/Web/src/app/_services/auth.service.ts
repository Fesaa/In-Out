import {computed, DestroyRef, effect, inject, Injectable, Injector, runInInjectionContext, Signal, signal} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {OAuthErrorEvent, OAuthService} from 'angular-oauth2-oidc';
import {environment} from '../../environments/environment';
import {OidcConfiguration} from '../_models/configuration';
import {takeUntilDestroyed, toObservable} from '@angular/core/rxjs-interop';
import {from} from 'rxjs';

/**
 * Enum mirror of angular-oauth2-oidc events which are used in Kavita
 */
export enum OidcEvents {
  /**
   * Fired on token refresh, and when the first token is received
   */
  TokenRefreshed = "token_refreshed"
}

export enum Role {
  CreateForOthers = 'CreateForOthers',
  ExportDeliveryRapport = 'ExportDeliveryRapport',
  ManageStock = 'ManageStock',
  ViewAllDeliveries = 'ViewAllDeliveries',
  ManageApplication = 'ManageApplication',
}

export type UserInfo = {
  UserName: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private readonly oauth2 = inject(OAuthService);
  private readonly httpClient = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);

  private readonly apiBaseUrl = environment.apiUrl;

  private readonly _loaded = signal(false);
  public readonly loaded = this._loaded.asReadonly();
  public readonly loaded$ = toObservable(this.loaded);

  /**
   * Public OIDC settings
   */
  private readonly _settings = signal<OidcConfiguration | undefined>(undefined);
  public readonly settings = this._settings.asReadonly();

  private readonly token = signal('');

  public readonly roles: Signal<Role[]> = computed(() => {
    const settings = this.settings();
    const token = this.token();
    if (!token || !settings) return [];

    const claims = this.decodeJwt(token);
    const resourceAccess = claims['resource_access'];
    if (!resourceAccess || !resourceAccess[settings.clientId]) return [];

    const roles = resourceAccess[settings.clientId].roles;
    return roles as Role[];
  });

  public readonly isAuthenticated= computed((): boolean => {
    this.token(); // Retrigger for new tokens
    return this.oauth2.hasValidAccessToken();
  });

  public readonly userInfo = computed<UserInfo | undefined>(() => {
    const token = this.token(); // Refresher

    const info = this.oauth2.getIdentityClaims();
    if (!info) return undefined;
    return {
      UserName: info["name"]
    };
  });

  constructor() {
    this.setupRefresh();
    this.setupOAuth();
  }

  login() {
    if (this.oauth2.hasValidAccessToken()) return;

    this.oauth2.initLoginFlow();
  }

  logout() {
    this.oauth2.logOut();
  }

  private setupOAuth() {
    // log events in dev
    if (!environment.production) {
      this.oauth2.events.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(event => {
        if (event instanceof OAuthErrorEvent) {
          console.error('OAuthErrorEvent:', event);
        } else {
          console.debug('OAuthEvent:', event);
        }
      });
    }

    this.oauth2.setStorage(localStorage);

    this.configuration().subscribe(cfg => {
      this._settings.set(cfg);

      this.oauth2.configure({
        issuer: cfg.authority,
        clientId: cfg.clientId,
        // Require https in production unless localhost
        requireHttps: environment.production ? 'remoteOnly' : false,
        redirectUri: window.location.origin + "/oidc/callback",
        postLogoutRedirectUri: window.location.origin,
        showDebugInformation: !environment.production,
        responseType: 'code',
        scope: "openid profile email roles offline_access",
        useSilentRefresh: false,
      });
      this.oauth2.setupAutomaticSilentRefresh();

      from(this.oauth2.loadDiscoveryDocumentAndTryLogin()).subscribe({
        next: _ => {
          this._loaded.set(true);

          if (this.oauth2.hasValidAccessToken()) {
            this.token.set(this.oauth2.getAccessToken());
            return;
          }

          if (!this.oauth2.hasValidAccessToken() && this.oauth2.getRefreshToken()) {
            this.oauth2.refreshToken()
              .catch(e => console.error(e))
              .then(() => {
                if (this.oauth2.hasValidAccessToken()) return;
                this.login();
              });
          } else if (!this.oauth2.hasValidAccessToken()) {
            this.login();
          }
        },
        error: error => {
          console.log(error);
        }
      });

      this.oauth2.events.subscribe(event => {
        if (event.type === OidcEvents.TokenRefreshed && this.oauth2.hasValidAccessToken()) {
          this.token.set(this.oauth2.getAccessToken());
        }
      });

      if (this.oauth2.hasValidAccessToken()) {
        this.token.set(this.oauth2.getAccessToken());
      }
    });
  }

  private setupRefresh() {
    window.addEventListener('online', () => {
      if (!this.oauth2.hasValidAccessToken() && this.oauth2.getRefreshToken()) {
        this.oauth2.refreshToken().catch(e => console.error(e));
      }
    });
  }

  private configuration() {
    return this.httpClient.get<OidcConfiguration>(this.apiBaseUrl + 'Configuration/oidc');
  }

  private decodeJwt(token: string) {
    const payload = token.split('.')[1];
    const decoded = atob(payload);
    return JSON.parse(decoded);
  }

}
