import {
  ApplicationConfig,
  importProvidersFrom,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection, isDevMode
} from '@angular/core';
import {provideRouter} from '@angular/router';
import {provideOAuthClient} from "angular-oauth2-oidc";
import {routes} from './app.routes';
import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import { TranslocoHttpLoader } from './_services/transloco-loader';
import { provideTransloco } from '@jsverse/transloco';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideOAuthClient({
      resourceServer: {
        allowedUrls: ["localhost:5000"],
        sendAccessToken: true,
      }
    }),
    provideHttpClient(withInterceptorsFromDi()),
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
