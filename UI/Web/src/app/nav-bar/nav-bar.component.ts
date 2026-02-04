import {ChangeDetectionStrategy, Component, computed, HostListener, inject, signal} from '@angular/core';
import {toSignal} from "@angular/core/rxjs-interop";
import {Breakpoint, UtilityService} from "../_services/utility.service";
import {RouterLink} from "@angular/router";
import {ButtonGroup, ButtonGroupKey, ButtonGroupService} from "../button-grid/button-group.service";
import {translate, TranslocoPipe} from "@jsverse/transloco";
import {TitleCasePipe} from "@angular/common";
import {animate, style, transition, trigger} from "@angular/animations";
import {MobileGridComponent} from "../button-grid/mobile-grid/mobile-grid.component";
import {AuthService} from '@inout/_services/auth.service';
import {NavigationService} from '@inout/_services/navigation.service';
import {BadgeComponent} from '@inout/shared/components/badge/badge.component';

@Component({
  selector: 'app-nav-bar',
  templateUrl: './nav-bar.component.html',
  styleUrls: ['./nav-bar.component.scss'],
  imports: [
    RouterLink,
    TitleCasePipe,
    MobileGridComponent,
    TranslocoPipe,
    BadgeComponent
  ],
  animations: [
    trigger('dropdownAnimation', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-8px)' }),
        animate('150ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
      transition(':leave', [
        animate('100ms ease-in', style({ opacity: 0, transform: 'translateY(-8px)' })),
      ]),
    ]),
    trigger('expandCollapse', [
      transition(':enter', [
        style({ height: '0', opacity: 0, overflow: 'hidden' }),
        animate('200ms ease-out', style({ height: '*', opacity: 1 })),
      ]),
      transition(':leave', [
        style({ height: '*', opacity: 1, overflow: 'hidden' }),
        animate('200ms ease-in', style({ height: '0', opacity: 0 })),
      ]),
    ])
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NavBarComponent {

  private readonly authService = inject(AuthService);
  protected readonly navService = inject(NavigationService);
  protected readonly buttonGroupService = inject(ButtonGroupService);
  protected readonly utilityService = inject(UtilityService);

  currentUser = this.authService.user;
  showNav = this.navService.showNavBar;

  isMobileGridOpen = signal(false);
  isAccountDropdownOpen = signal(false);
  expandedGroup = signal<ButtonGroupKey | null>(ButtonGroupKey.Navigation);

  isMobile = computed(() => this.showNav() && this.utilityService.breakPoint() <= Breakpoint.Mobile);
  isDesktop = computed(() => this.showNav() && this.utilityService.breakPoint() > Breakpoint.Mobile);

  dashboardGroups = this.buttonGroupService.allGroups;

  mobileButtonGroups = computed<ButtonGroup[]>(() => [
    {
      key: ButtonGroupKey.Any,
      title: '',
      icon: '',
      buttons: [
        {
          title: translate('navigation.home'),
          icon: 'fa fa-home',
          navUrl: 'dashboard',
          standAlone: true,
        }
      ]
    },
    ...this.buttonGroupService.allGroups(),
  ])

  toggleMobileGrid() {
    this.isMobileGridOpen.update(v => !v);
  }

  toggleGroup(key: ButtonGroupKey) {
    const cur = this.expandedGroup();
    if (cur === key) {
      this.expandedGroup.set(null);
    } else {
      this.expandedGroup.set(key);
    }
  }

  isGroupExpanded(key: ButtonGroupKey): boolean {
    return this.expandedGroup() == key;
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (
      this.isAccountDropdownOpen() &&
      !target.closest('.account-dropdown') &&
      !target.closest('.account-toggle')
    ) {
      this.isAccountDropdownOpen.set(false);
    }
  }

}
