import {computed, inject, Injectable, signal, TemplateRef} from '@angular/core';
import {NavigationExtras, Router} from '@angular/router';
import {translate, TranslocoService} from '@jsverse/transloco';
import {toSignal} from '@angular/core/rxjs-interop';
import {filter, tap} from 'rxjs';
import {AuthService, Role} from '@inout/_services/auth.service';
import {Breakpoint, UtilityService} from '@inout/_services/utility.service';
import {ListSelectModalComponent} from '@inout/shared/components/list-select-modal/list-select-modal.component';
import {ModalService} from '@inout/_services/modal.service';

export enum ManagementSettingsId {
  Products = 'products',
  Clients = 'clients',
  Server = 'server',
}

export enum NavigationsId {
  Dashboard = 'dashboard',
  NewDelivery = 'newDelivery',
  Deliveries = 'deliveries',
  Stock = 'stock',
  User = 'user',
  Logout = 'logout',
}

export type NavigationItemId = ManagementSettingsId | NavigationsId;

export interface Button {
  id?: NavigationItemId;
  standAlone?: boolean;
  icon: string;
  title: string;
  navUrl?: string;
  navExtras?: NavigationExtras;
  onClick?: () => void;
  requiredRoles?: Role[];
  blacklistedRoles?: Role[];
  badge?: string;
}

export enum ButtonGroupKey {
  Navigation = 0,
  Management = 1,
  Account = 2,

  Any = 999999,
}

export interface ButtonGroup {
  icon: string;
  title: string;
  key: ButtonGroupKey;
  buttons: Button[];
}

@Injectable({
  providedIn: 'root'
})
export class ButtonGroupService {

  private readonly auth = inject(AuthService);
  private readonly transloco = inject(TranslocoService);
  private readonly router = inject(Router);
  private readonly utilityService = inject(UtilityService);
  private readonly modalService = inject(ModalService);

  translationReloaded = toSignal(this.transloco.events$.pipe(
    filter(event => event.type === 'translationLoadSuccess')
  ));

  public showNavBar = signal(true);
  public isMobileMenuOpen = signal(false);
  public isAccountDropdownOpen = signal(false);

  navigationGroup = computed<ButtonGroup>(() => {
    this.translationReloaded();

    return {
      key: ButtonGroupKey.Navigation,
      title: translate('navigation.items.title'),
      icon: 'fas fa-bars',
      buttons: [
        {
          id: NavigationsId.NewDelivery,
          title: translate('navigation.items.new-delivery'),
          icon: 'fas fa-truck-loading',
          requiredRoles: [],
          navUrl: '/delivery/manage',
          standAlone: true
        },
        {
          id: NavigationsId.Deliveries,
          title: translate('navigation.items.deliveries'),
          icon: 'fas fa-shipping-fast',
          requiredRoles: [],
          navUrl: '/delivery/browse',
          standAlone: true
        },
        {
          id: NavigationsId.Stock,
          title: translate('navigation.items.stock'),
          icon: 'fas fa-warehouse',
          requiredRoles: [],
          navUrl: '/stock/browse',
          standAlone: true
        },
      ],
    };
  });

  managementGroup = computed<ButtonGroup>(() => {
    this.translationReloaded();

    return {
      key: ButtonGroupKey.Management,
      title: translate('navigation.management.title'),
      icon: 'fas fa-cog',
      buttons: [
        {
          id: ManagementSettingsId.Products,
          title: translate('navigation.management.items.products'),
          icon: 'fas fa-boxes-stacked',
          requiredRoles: [Role.ManageProducts],
          navUrl: '/management/products'
        },
        {
          id: ManagementSettingsId.Clients,
          title: translate('navigation.management.items.clients'),
          icon: 'fas fa-users',
          requiredRoles: [Role.ManageClients],
          navUrl: '/management/clients'
        },
        {
          id: ManagementSettingsId.Server,
          title: translate('navigation.management.items.server'),
          icon: 'fas fa-server',
          requiredRoles: [Role.ManageApplication],
          navUrl: '/management/server'
        },
      ],
    };
  });

  accountGroup = computed<ButtonGroup>(() => {
    this.translationReloaded();

    return {
      key: ButtonGroupKey.Account,
      title: translate('navigation.account.title'),
      icon: 'fas fa-user',
      buttons: [
        {
          id: NavigationsId.User,
          title: translate('navigation.items.user'),
          icon: 'fas fa-user',
          requiredRoles: [],
          navUrl: '/management/user'
        },
        {
          id: NavigationsId.Logout,
          title: translate('navigation.items.logout'),
          icon: 'fas fa-right-from-bracket',
          requiredRoles: [],
          onClick: () => this.auth.logout()
        },
      ],
    };
  });

  allGroups = computed<ButtonGroup[]>(() => [
    this.navigationGroup(),
    this.managementGroup(),
    this.accountGroup(),
  ]);

  /**
   * Legacy computed for backward compatibility
   */
  items = computed(() => {
    return this.allGroups()
      .flatMap(group => group.buttons)
      .filter(button => this.shouldRender(button));
  });

  shouldRender(button: Button): boolean {
    const roles = this.auth.roles();

    if (button.blacklistedRoles && button.blacklistedRoles.length > 0) {
      if (button.blacklistedRoles.some(role => roles.includes(role))) {
        return false;
      }
    }

    if (!button.requiredRoles || button.requiredRoles.length === 0) {
      return true;
    }

    return button.requiredRoles.some(role => roles.includes(role));
  }

  anyVisible(buttons: Button[]): boolean {
    return buttons.some(button => this.shouldRender(button));
  }

  visibleButtons(group: ButtonGroup): Button[] {
    return group.buttons.filter(btn => this.shouldRender(btn));
  }

  mobileMode = computed(() => this.utilityService.breakPoint() < Breakpoint.Desktop );

  groupedButtons(group: ButtonGroup) {
    return (this.mobileMode() ? group.buttons.filter(btn => !btn.standAlone) : [])
      .filter(btn => this.shouldRender(btn));
  }

  standAloneButtons(group: ButtonGroup) {
    return (this.mobileMode()
      ? group.buttons.filter(btn => !!btn.standAlone)
      : group.buttons)
      .filter(btn => this.shouldRender(btn));
  }

  groupBadge(group: ButtonGroup): string | undefined {
    const counts = this.groupedButtons(group)
      .map(btn => btn.badge)
      .filter(badge => !!badge)
      .map(badge => parseInt(badge!))
      .filter(num => !isNaN(num));

    if (counts.length === 0) return undefined;

    const total = counts.reduce((acc, curr) => acc + curr, 0);
    return total > 0 ? `${total}` : undefined;
  }

  handleButtonClick(button: Button, event?: Event): void {
    if (button.onClick) {
      event?.preventDefault();
      button.onClick();
    }

    if (button.navUrl) {
      event?.preventDefault();
      this.router.navigate([button.navUrl], button.navExtras)
        .catch(err => console.error(err));
    }
  }

  handleGroupClick(group: ButtonGroup, templ?: TemplateRef<any>): void {
    const [modal, component] = this.modalService.open(ListSelectModalComponent, {
      size: 'lg', centered: true,
    });

    component.title.set(group.title);
    component.inputItems.set(this.groupedButtons(group).map(btn => ({label: btn.title, value: btn})));
    component.showFooter.set(false);
    component.requireConfirmation.set(false);

    if (templ) {
      component.itemTemplate.set(templ);
    }

    this.modalService.onClose$<Button>(modal).pipe(
      tap(btn => this.handleButtonClick(btn))
    ).subscribe();
  }

  close(): void {
    this.isMobileMenuOpen.set(false);
    this.isAccountDropdownOpen.set(false);
  }
}
