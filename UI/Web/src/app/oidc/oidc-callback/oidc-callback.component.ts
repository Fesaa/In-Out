import {ChangeDetectionStrategy, Component, effect, inject, OnInit, signal} from '@angular/core';
import {LoadingSpinnerComponent} from '../../shared/components/loading-spinner/loading-spinner.component';
import {TranslocoDirective} from '@jsverse/transloco';
import {AuthService} from '../../_services/auth.service';
import {Router} from '@angular/router';
import {AuthGuard} from '../../_guards/auth-guard';

@Component({
  selector: 'app-oidc-callback',
  imports: [
    LoadingSpinnerComponent,
    TranslocoDirective
  ],
  templateUrl: './oidc-callback.component.html',
  styleUrl: './oidc-callback.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OidcCallbackComponent implements OnInit{

  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  showErrorMessage = signal(false);

  constructor() {
    effect(() => {
      if (this.authService.isAuthenticated()) {
        this.redirect();
      }
    });
  }

  ngOnInit(): void {
    setTimeout(() => {
      if (!this.authService.isAuthenticated()) {
        this.showErrorMessage.set(true);
        setTimeout(() => this.authService.login(), 1000);
      }
    }, 1000);
  }


  private redirect() {
    const path = localStorage.getItem(AuthGuard.urlKey);
    if (path && path !== '') {
      this.router.navigate([path]);
    } else {
      this.router.navigate(['/dashboard']);
    }
  }

}
