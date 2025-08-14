import {Component, HostListener, inject, OnInit} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {AuthService} from './_services/auth.service';
import {DashboardComponent} from './dashboard/dashboard.component';
import {TranslocoModule, TranslocoService} from '@jsverse/transloco';
import {NavBarComponent} from './nav-bar/nav-bar.component';
import {NavigationService} from './_services/navigation.service';

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

  ngOnInit(): void {
    this.updateVh();
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
