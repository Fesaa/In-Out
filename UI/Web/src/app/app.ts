import {Component, DestroyRef, HostListener, inject, OnInit} from '@angular/core';
import {NavigationStart, Router, RouterOutlet} from '@angular/router';
import {AuthService} from './_services/auth.service';
import {TranslocoModule, TranslocoService} from '@jsverse/transloco';
import {NavBarComponent} from './nav-bar/nav-bar.component';
import {NavigationService} from './_services/navigation.service';
import {filter} from 'rxjs';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {NgbModal} from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, TranslocoModule, NavBarComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected title = 'Web';

  protected readonly oidcService = inject(AuthService);
  private readonly transLoco = inject(TranslocoService);
  protected readonly navService = inject(NavigationService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly ngbModal = inject(NgbModal)

  ngOnInit(): void {
    this.updateVh();

    this.router.events
      .pipe(
        filter(event => event instanceof NavigationStart),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(async (event) => {
        if (this.ngbModal.hasOpenModals()) {
          this.ngbModal.dismissAll();
        }

        this.navService.close();

        if ((event as any).navigationTrigger === 'popstate') {
          const currentRoute = this.router.routerState;
          await this.router.navigateByUrl(currentRoute.snapshot.url, { skipLocationChange: true });
        }
      });
  }

  @HostListener('window:resize')
  @HostListener('window:orientationchange')
  setDocHeight() {
    this.updateVh();
  }

  private updateVh(): void {
    // Sets a CSS variable for the actual device viewport height. Needed for mobile dev.
    const vh = window.innerHeight * 0.01;
    document.documentElement.style.setProperty('--vh', `${vh}px`);
  }


}
