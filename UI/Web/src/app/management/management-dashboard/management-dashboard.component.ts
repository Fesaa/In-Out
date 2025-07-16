import {ChangeDetectionStrategy, Component, inject, signal, WritableSignal} from '@angular/core';
import {
  ManagementSettingsId,
  NavigationItem, NavigationItemId,
  NavigationService
} from '../../_services/navigation.service';
import {Router, RouterLink} from '@angular/router';
import {TranslocoDirective} from '@jsverse/transloco';
import {AuthService} from '../../_services/auth.service';
import {ManagementOverviewComponent} from '../_components/management-overview/management-overview.component';
import {ManagementProductsComponent} from '../_components/management-products/management-products.component';
import {ManagementClientsComponent} from '../_components/management-clients/management-clients.component';
import {ManagementDeliveriesComponent} from '../_components/management-deliveries/management-deliveries.component';
import {ManagementServerComponent} from '../_components/management-server/management-server.component';

@Component({
  selector: 'app-management-dashboard',
  imports: [
    TranslocoDirective,
    RouterLink,
    ManagementOverviewComponent,
    ManagementProductsComponent,
    ManagementClientsComponent,
    ManagementDeliveriesComponent,
    ManagementServerComponent
  ],
  templateUrl: './management-dashboard.component.html',
  styleUrl: './management-dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManagementDashboardComponent {

  private router = inject(Router);
  protected navigationService = inject(NavigationService);
  protected authService = inject(AuthService);

  activeItem: WritableSignal<NavigationItemId> = signal(ManagementSettingsId.Overview)
  sidebarOpen = signal(false);

  toggleSidebar() {
    this.sidebarOpen.update(v => !v);
  }

  onNavItemClick(item: NavigationItem, event: Event) {
    event.preventDefault();

    this.activeItem.set(item.id);
    this.sidebarOpen.set(false)

    if (item.routerLink) {
      this.router.navigate([item.routerLink]);
    } else if (item.action) {
      item.action();
    }
  }

  protected readonly ManagementSettingsId = ManagementSettingsId;
}
