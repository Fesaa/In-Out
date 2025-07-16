import {Routes} from '@angular/router';
import {DashboardComponent} from './dashboard/dashboard.component';
import {AuthGuard} from './_guards/auth-guard';
import {roleGuard} from './_guards/role-guard';
import {Role} from './_services/auth.service';
import {ManagementDashboardComponent} from './management/management-dashboard/management-dashboard.component';

export const routes: Routes = [
  {
    path: '',
    canActivate: [AuthGuard],
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent,
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
