import {Routes} from '@angular/router';
import {DashboardComponent} from './dashboard/dashboard.component';
import {AuthGuard} from './_guards/auth-guard';
import {roleGuard} from './_guards/role-guard';
import {Role} from './_services/auth.service';
import {ManageDeliveryComponent} from './manage-delivery/manage-delivery.component';
import {BrowseDeliveriesComponent} from './browse-deliveries/browse-deliveries.component';
import {BrowseStockComponent} from './browse-stock/browse-stock.component';
import {ManagementProductsComponent} from './management/management-products/management-products.component';
import {ManagementClientsComponent} from './management/management-clients/management-clients.component';
import {ManagementServerComponent} from './management/management-server/management-server.component';

export const routes: Routes = [
  {
    path: '',
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard',
      },
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
    children: [
      {
        path: 'products',
        component: ManagementProductsComponent,
      },
      {
        path: 'clients',
        component: ManagementClientsComponent
      },
      {
        path: 'server',
        component: ManagementServerComponent,
      }
    ],
  },
  {
    path: 'oidc',
    loadChildren: () => import('./_routes/oidc.routes').then(m => m.routes)
  }
];
