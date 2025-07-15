import {Routes} from "@angular/router";
import {OidcCallbackComponent} from '../oidc/oidc-callback/oidc-callback.component';

export const routes: Routes = [
  {
    path: 'callback',
    component: OidcCallbackComponent,
  }
]
