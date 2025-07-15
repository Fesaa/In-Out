import {computed, DestroyRef, effect, inject, Injectable, Signal, signal} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {OAuthErrorEvent, OAuthService} from 'angular-oauth2-oidc';
import {environment} from '../../environments/environment';
import {OidcConfiguration} from '../_models/configuration';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {from} from 'rxjs';

/**
 * Enum mirror of angular-oauth2-oidc events which are used in Kavita
 */
export enum OidcEvents {
  /**
   * Fired on token refresh, and when the first token is recieved
   */
  TokenRefreshed = "token_refreshed"
}

export enum Role {
  CreateForOthers = 'CreateForOthers',
  ExportDeliveryRapport = 'ExportDeliveryRapport',
  ManageStock = 'ManageStock',
  ViewAllDeliveries = 'ViewAllDeliveries',
}

@Injectable({
  providedIn: 'root'
})
export class AuthorizationService {

  private readonly oauth2 = inject(OAuthService);
  private readonly httpClient = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);

  private readonly apiBaseUrl = environment.apiUrl;

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
    if (!resourceAccess) return [];

    const roles = resourceAccess[settings.clientId].roles;
    return roles as Role[];
  });

  constructor() {
    this.setupRefresh();
    this.setupOAuth();
  }

  hasRole(role: Role): boolean {
    return this.roles().includes(role);
  }

  login() {
    if (this.oauth2.hasValidAccessToken()) return;

    this.oauth2.initLoginFlow();
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
        postLogoutRedirectUri: window.location.origin + "login",
        showDebugInformation: !environment.production,
        responseType: 'code',
        scope: "openid profile email roles offline_access",
        // Not all OIDC providers follow this nicely
        strictDiscoveryDocumentValidation: false,
        useSilentRefresh: false,
      });
      this.oauth2.setupAutomaticSilentRefresh();

      from(this.oauth2.loadDiscoveryDocumentAndTryLogin()).subscribe({
        next: _ => {
          if (!this.oauth2.hasValidAccessToken() && this.oauth2.getRefreshToken()) {
            this.oauth2.refreshToken()
              .catch(e => console.error(e))
              .then(() => {
                if (this.oauth2.hasValidAccessToken()) return;

                console.log("Failed to get valid token");
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
