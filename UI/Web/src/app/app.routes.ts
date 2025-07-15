import { Routes } from '@angular/router';
import {DashboardComponent} from './dashboard/dashboard.component';
import {AuthGuard} from './_guards/auth-guard';

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
    path: 'oidc',
    loadChildren: () => import('./_routes/oidc.routes').then(m => m.routes)
  }
];
