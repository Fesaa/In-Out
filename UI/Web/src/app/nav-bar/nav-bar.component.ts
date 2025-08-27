import {ChangeDetectionStrategy, Component, computed, HostListener, inject, OnInit, Signal} from '@angular/core';
import {NgTemplateOutlet, TitleCasePipe} from "@angular/common";
import {animate, style, transition, trigger} from "@angular/animations";
import {RouterLink} from '@angular/router';
import {AuthService} from '../_services/auth.service';
import {NavigationItem, NavigationService, NavigationsId} from '../_services/navigation.service';
import {TranslocoPipe} from '@jsverse/transloco';
import {main} from '@popperjs/core';

const drawerAnimation = trigger('drawerAnimation', [
  transition(':enter', [
    style({ transform: 'translateX(-100%)', opacity: 0 }),
    animate('250ms ease-out', style({ transform: 'translateX(0)', opacity: 1 })),
  ]),
  transition(':leave', [
    animate('200ms ease-in', style({ transform: 'translateX(-100%)', opacity: 0 })),
  ]),
]);

const dropdownAnimation = trigger('dropdownAnimation', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateY(-8px)' }),
    animate('150ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
  ]),
  transition(':leave', [
    animate('100ms ease-in', style({ opacity: 0, transform: 'translateY(-8px)' })),
  ]),
]);



@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.scss'],
  imports: [
    RouterLink,
    TitleCasePipe,
    TranslocoPipe,
    NgTemplateOutlet
  ],
  animations: [drawerAnimation, dropdownAnimation],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavBarComponent implements OnInit {

  protected authService = inject(AuthService);
  protected navService = inject(NavigationService);

  mainItems: Signal<NavigationItem[]> = computed(() => {
    const fromNav = this.navService.items().filter(i => i.id !== NavigationsId.Logout);
    return [{
      id: NavigationsId.Dashboard,
      translationKey: 'navigation.items.dashboard',
      icon: 'fas fa-home',
      requiredRoles: [],
      routerLink: '/dashboard'
    }, ...fromNav]
  });
  accountItems = computed(() => {
    const main = this.mainItems();
    return this.navService.items().filter(i => !main.includes(i));
  })

  ngOnInit(): void {
  }

  logout() {
    this.authService.logout();
  }

  toggleMobileMenu() {
    this.navService.isMobileMenuOpen.update(v => !v);
  }

  toggleAccountDropdown() {
    this.navService.isAccountDropdownOpen.update(v => !v);
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (
      this.navService.isAccountDropdownOpen() &&
      !target.closest('.account-dropdown') &&
      !target.closest('.account-toggle')
    ) {
      this.navService.isAccountDropdownOpen.set(false);
    }
  }

  protected readonly main = main;
}
