import {computed, inject, Injectable, signal} from '@angular/core';
import {AuthService, Role} from './auth.service';

export enum ManagementSettingsId {
  Overview = 'overview',
  Products = 'products',
  Clients = 'clients',
  Server= 'server',
}

export enum NavigationsId {
  Dashboard = 'dashboard',
  NewDelivery = 'newDelivery',
  Deliveries = 'deliveries',
  Stock = 'stock',
  Management = 'management',
  Logout = 'logout',
}

export type NavigationItemId = ManagementSettingsId | NavigationsId;

export type NavigationItem = {
  /**
   * Identifying id
   */
  id: NavigationItemId;
  /**
   * Translation key with full prefix
   */
  translationKey: string;
  /**
   * Icon to use
   */
  icon: string;
  /**
   * If set, clicking on it should route to this page (always internal)
   */
  routerLink?: string;
  /**
   * If set, Action to perform when hit
   */
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
      id: NavigationsId.NewDelivery,
      translationKey: 'navigation.items.new-delivery',
      icon: 'fas fa-truck-loading',
      requiredRoles: [],
      routerLink: '/delivery/manage'
    },
    {
      id: NavigationsId.Deliveries,
      translationKey: 'navigation.items.deliveries',
      icon: 'fas fa-shipping-fast',
      requiredRoles: [],
      routerLink: '/delivery/browse'
    },
    {
      id: NavigationsId.Stock,
      translationKey: 'navigation.items.stock',
      icon: 'fas fa-warehouse',
      requiredRoles: [],
      routerLink: '/stock/browse'
    },
    {
      id: ManagementSettingsId.Products,
      translationKey: 'navigation.management.items.products',
      icon: 'fas fa-boxes-stacked',
      requiredRoles: [Role.ManageStock],
      routerLink: '/management/products'
    },
    {
      id: ManagementSettingsId.Clients,
      translationKey: 'navigation.management.items.clients',
      icon: 'fas fa-users',
      requiredRoles: [],
      routerLink: '/management/clients'
    },
    {
      id: ManagementSettingsId.Server,
      translationKey: 'navigation.management.items.server',
      icon: 'fas fa-server',
      requiredRoles: [Role.ManageApplication],
      routerLink: '/management/server'
    },
    {
      id: NavigationsId.Logout,
      translationKey: 'navigation.items.logout',
      icon: 'fas fa-right-from-bracket',
      requiredRoles: [],
      action: () => {
        this.auth.logout();
      }
    }
  ];

  public items = computed(() => {
    const roles = this.auth.roles();
    return this._items.filter(item => this.canAccess(item, roles));
  });

  public showNavBar = signal(true);
  isMobileMenuOpen = signal(false);
  isAccountDropdownOpen = signal(false);

  close() {
    this.isMobileMenuOpen.set(false);
    this.isAccountDropdownOpen.set(false);
  }

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
