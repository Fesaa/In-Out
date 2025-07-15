import {Component, inject, OnInit} from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {AuthService} from './_services/auth.service';
import {DashboardComponent} from './dashboard/dashboard.component';
import {TranslocoModule, TranslocoService} from '@jsverse/transloco';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, TranslocoModule],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  protected title = 'Web';

  protected readonly oidcService = inject(AuthService);
  private readonly transLoco = inject(TranslocoService);

  ngOnInit(): void {

  }



}
