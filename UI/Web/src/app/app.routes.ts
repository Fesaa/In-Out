import {Routes} from '@angular/router';
import {DashboardComponent} from './dashboard/dashboard.component';
import {AuthGuard} from './_guards/auth-guard';
import {roleGuard} from './_guards/role-guard';
import {Role} from './_services/auth.service';
import {ManagementDashboardComponent} from './management/management-dashboard/management-dashboard.component';
import {ManageDeliveryComponent} from './manage-delivery/manage-delivery.component';
import {BrowseDeliveriesComponent} from './browse-deliveries/browse-deliveries.component';
import {BrowseStockComponent} from './browse-stock/browse-stock.component';

export const routes: Routes = [
  {
    path: '',
    canActivate: [AuthGuard],
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent,
      },
      {
        path: 'delivery',
        children: [
          {
            path: 'manage',
            component: ManageDeliveryComponent,
          },
          {
            path: 'browse',
            component: BrowseDeliveriesComponent,
          }
        ]
      },
      {
        path: 'stock',
        children: [
          {
            path: 'browse',
            component: BrowseStockComponent
          }
        ]
      }
    ]
  },
  {
    path: 'management',
    canActivate: [roleGuard(Role.ManageApplication)],
    component: ManagementDashboardComponent,
  },
  {
    path: 'oidc',
    loadChildren: () => import('./_routes/oidc.routes').then(m => m.routes)
  }
];
