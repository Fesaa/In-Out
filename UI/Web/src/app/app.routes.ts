import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'oidc',
    loadChildren: () => import('./_routes/oidc.routes').then(m => m.routes)
  }
];
