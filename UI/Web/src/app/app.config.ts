import {
  ApplicationConfig,
  importProvidersFrom, inject,
  isDevMode, provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection
} from '@angular/core';
import {provideRouter} from '@angular/router';
import {routes} from './app.routes';
import {HTTP_INTERCEPTORS, provideHttpClient, withFetch, withInterceptorsFromDi} from '@angular/common/http';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {provideAnimationsAsync} from '@angular/platform-browser/animations/async';
import {TranslocoHttpLoader} from './_services/transloco-loader';
import {provideTransloco, TranslocoService} from '@jsverse/transloco';
import {provideToastr} from 'ngx-toastr';
import {ErrorInterceptor} from './_interceptors/error-interceptor';
import {DeliveryStatePipe} from './_pipes/delivery-state-pipe';
import {AuthService} from './_services/auth.service';
import {filter, firstValueFrom, map, of, switchMap, tap} from 'rxjs';
import {AllLanguages} from './_models/user';

export const appConfig: ApplicationConfig = {
  providers: [
    DeliveryStatePipe,

    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideToastr(),
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    provideHttpClient(withInterceptorsFromDi(), withFetch()),
    importProvidersFrom(BrowserAnimationsModule),
    provideAnimationsAsync(),
    provideTransloco({
      config: {
        availableLangs: AllLanguages,
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
    provideAppInitializer(() => {
      const authService = inject(AuthService);
      return firstValueFrom(authService.loadUser().pipe(
        tap(loggedIn => {
          if (!loggedIn) {
            authService.logout();
          }
        }),
      ));
    }),
  ]
};
