import {ChangeDetectionStrategy, Component, inject, OnDestroy, OnInit} from '@angular/core';
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
export class DashboardComponent implements OnInit, OnDestroy {

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

  ngOnInit(): void {
    this.navigationService.showNavBar.set(false);

    const items = this.navigationService.items();
    if (items.length === 1) {
      this.navigate(items[0]);
    }
  }

  ngOnDestroy(): void {
    this.navigationService.showNavBar.set(true);
  }

}
