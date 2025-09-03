import {ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal} from '@angular/core';
import {UnderConstructionComponent} from '../../shared/components/under-construction/under-construction.component';
import {TranslocoDirective} from '@jsverse/transloco';
import {
  NgbNav,
  NgbNavContent,
  NgbNavItem,
  NgbNavLink,
  NgbNavLinkButton,
  NgbNavOutlet
} from '@ng-bootstrap/ng-bootstrap';
import {SettingsService} from '../../_services/settings.service';
import {DeliveryExportField, LogLevel, LogLevelValues, Settings} from '../../_models/settings';
import {FormArray, FormControl, FormGroup, NonNullableFormBuilder, ReactiveFormsModule} from '@angular/forms';
import {SettingsItemComponent} from '../../shared/components/settings-item/settings-item.component';
import {LogLevelPipe} from '../../_pipes/log-level-pipe';
import {debounceTime, distinctUntilChanged, filter, switchMap, tap} from 'rxjs';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {ToastrService} from 'ngx-toastr';
import {ExportSettingsComponent} from './_components/export-settings/export-settings.component';

enum NavId {
  General = 'general',
  Export  = 'export',
}

type SettingsForm = FormGroup<{
  logLevel: FormControl<LogLevel>,
  csvExportConfiguration: FormGroup<{
    headerNames: FormArray<FormControl<string>>,
    headerOrder: FormArray<FormControl<DeliveryExportField>>,
  }>
}>

@Component({
  selector: 'app-management-server',
  imports: [
    TranslocoDirective,
    NgbNav,
    NgbNavItem,
    NgbNavContent,
    NgbNavOutlet,
    NgbNavLink,
    ReactiveFormsModule,
    SettingsItemComponent,
    LogLevelPipe,
    ExportSettingsComponent
  ],
  templateUrl: './management-server.component.html',
  styleUrl: './management-server.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManagementServerComponent implements OnInit {

  protected readonly NavId = NavId;

  private readonly settingsService = inject(SettingsService);
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly toastR = inject(ToastrService);

  activeId = NavId.General;

  settings = signal<Settings | undefined>(undefined);
  settingsForm!: SettingsForm;

  ngOnInit(): void {
    this.settingsService.getSettings().subscribe(settings => {
      this.settings.set(settings);
      this.setupForm(settings);
    });
  }

  private setupForm(settings: Settings) {
    const headerNames = settings.csvExportConfiguration.headerNames
      .map(hn => this.fb.control(hn));
    const headerOrder = settings.csvExportConfiguration.headerOrder
      .map(ho => this.fb.control(ho));

    this.settingsForm = this.fb.group({
      logLevel: this.fb.control(settings.logLevel),
      csvExportConfiguration: this.fb.group({
        headerNames: this.fb.array(headerNames),
        headerOrder: this.fb.array(headerOrder)
      }),
    });

    this.settingsForm.valueChanges.pipe(
      distinctUntilChanged(),
      debounceTime(100),
      takeUntilDestroyed(this.destroyRef),
      filter(() => this.settingsForm.valid),
      switchMap(() => this.settingsService.updateSettings(this.settingsForm.value as Settings)),
    ).subscribe();
  }

  protected readonly LogLevelValues = LogLevelValues;
}
