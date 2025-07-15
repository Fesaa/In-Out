import {inject, Injectable} from '@angular/core';
import {ActivatedRouteSnapshot, CanActivate, GuardResult, MaybeAsync, RouterStateSnapshot} from '@angular/router';
import {AuthService} from '../_services/auth.service';


@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  public static readonly urlKey = 'in-out--auth-interceptor--url';

  private readonly authService = inject(AuthService);

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): MaybeAsync<GuardResult> {
    const isAuthenticated = this.authService.isAuthenticated();
    if (isAuthenticated) {
      return true;
    }

    const path = window.location.pathname;
    if (path !== '/login' && !path.startsWith("oidc") && path !== '') {
      localStorage.setItem(AuthGuard.urlKey, path);
    }

    return false;
  }

}
