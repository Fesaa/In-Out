import { CanActivateFn } from '@angular/router';
import {AuthService, Role} from '../_services/auth.service';
import {inject} from '@angular/core';
import {filter, map} from 'rxjs';

export const roleGuard: (role: Role) => CanActivateFn = (role) => {
  return (route, state) => {
    const authService = inject(AuthService);
    return authService.loaded$.pipe(filter(x => x), map(() => {
      return authService.isAuthenticated() && authService.roles().includes(role);
    }));
  };
}
