import {computed, inject, Injectable} from '@angular/core';
import {AuthService, Role} from './auth.service';

export type NavigationItem = {
  translationKey: string;
  icon: string;
  routerLink?: string;
  action?: () => void;
  /**
   * If not empty, requires the user to have them
   */
  requiredRoles: Role[];
  /**
   * If not empty, requires the user to not have them
   */
  blacklistedRoles?: Role[];
}

@Injectable({
  providedIn: 'root'
})
export class NavigationService {

  private readonly auth = inject(AuthService);

  private _items: NavigationItem[] = [
    {
      translationKey: 'navigation.items.new-delivery',
      icon: 'fas fa-truck-loading',
      requiredRoles: [],
      routerLink: 'delivery/new'
    },
    {
      translationKey: 'navigation.items.deliveries',
      icon: 'fas fa-shipping-fast',
      requiredRoles: [],
      routerLink: 'delivery/browse'
    },
    {
      translationKey: 'navigation.items.stock',
      icon: 'fas fa-warehouse',
      requiredRoles: [],
      routerLink: 'stock/browse'
    },
    {
      translationKey: 'navigation.items.management',
      icon: 'fas fa-cogs',
      requiredRoles: [Role.ManageApplication],
      routerLink: 'management'
    },
  ];

  public items = computed(() => {
    const roles = this.auth.roles();
    return this._items.filter(item => this.canAccess(item, roles));
  });

  private canAccess(item: NavigationItem, roles: Role[]): boolean {
    if (item.requiredRoles.length === 0 && (!item.blacklistedRoles || item.blacklistedRoles.length === 0)) {
      return true;
    }

    if (item.blacklistedRoles && item.blacklistedRoles.length > 0 && item.blacklistedRoles.filter(r => roles.includes(r)).length > 0) {
      return false;
    }

    if (item.requiredRoles.length > 0 && item.requiredRoles.filter(r => roles.includes(r)).length > 0) {
      return true;
    }

    return false;
  }


}
