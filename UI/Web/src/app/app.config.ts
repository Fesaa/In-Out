import {
  ApplicationConfig,
  importProvidersFrom,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection, isDevMode
} from '@angular/core';
import {provideRouter} from '@angular/router';
import {provideOAuthClient} from "angular-oauth2-oidc";
import {routes} from './app.routes';
import {HTTP_INTERCEPTORS, provideHttpClient, withFetch, withInterceptorsFromDi} from '@angular/common/http';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import { TranslocoHttpLoader } from './_services/transloco-loader';
import { provideTransloco } from '@jsverse/transloco';
import {provideToastr} from 'ngx-toastr';
import {ErrorInterceptor} from './_interceptors/error-interceptor';
import {DeliveryStatePipe} from './_pipes/delivery-state-pipe';

export const appConfig: ApplicationConfig = {
  providers: [
    DeliveryStatePipe,

    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideToastr(),
    provideOAuthClient({
      resourceServer: {
        sendAccessToken: true,
      }
    }),
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    provideHttpClient(withInterceptorsFromDi(), withFetch()),
    importProvidersFrom(BrowserAnimationsModule),
    provideAnimationsAsync(),
    provideTransloco({
      config: {
        availableLangs: ['en'],
        defaultLang: 'en',
        missingHandler: {
          useFallbackTranslation: true,
          allowEmpty: true,
        },
        reRenderOnLangChange: true,
        prodMode: !isDevMode(),
      },
      loader: TranslocoHttpLoader,
    }),
  ]
};
