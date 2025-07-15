import {ChangeDetectionStrategy, Component, inject} from '@angular/core';
import {NavigationItem, NavigationService} from '../_services/navigation.service';
import {TranslocoDirective} from '@jsverse/transloco';
import {Router} from '@angular/router';

@Component({
  selector: 'app-dashboard',
  imports: [
    TranslocoDirective
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent {

  private readonly router = inject(Router);
  protected readonly navigationService = inject(NavigationService);

  navigate(item: NavigationItem): void {
    if (item.routerLink) {
      this.router.navigateByUrl(item.routerLink);
      return;
    }

    if (item.action) {
      item.action();
    }
  }

}
