import {ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit} from '@angular/core';
import {FormArray, FormControl, FormGroup, NonNullableFormBuilder, ReactiveFormsModule} from '@angular/forms';
import {AuthService, Role} from '../../_services/auth.service';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {debounceTime, distinctUntilChanged, map, switchMap} from 'rxjs';
import {AllLanguages, User} from '../../_models/user';
import {SettingsItemComponent} from '../../shared/components/settings-item/settings-item.component';
import {TranslocoDirective} from '@jsverse/transloco';
import {LanguagePipe} from '../../_pipes/language-pipe';

type UserForm = FormGroup<{
  id: FormControl<number>
  name: FormControl<string>,
  language: FormControl<string>,
  roles: FormArray<FormControl<Role>>,
}>;

@Component({
  selector: 'app-manage-user',
  imports: [
    ReactiveFormsModule,
    SettingsItemComponent,
    TranslocoDirective,
    LanguagePipe
  ],
  templateUrl: './manage-user.component.html',
  styleUrl: './manage-user.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageUserComponent implements OnInit {

  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly fb = inject(NonNullableFormBuilder);

  userForm!: UserForm;

  ngOnInit(): void {
    const user = this.authService.user();


    this.userForm = this.fb.group({
      id: this.fb.control(user.id),
      name: this.fb.control(user.name),
      language: this.fb.control(user.language),
      roles: this.fb.array(user.roles.map(r => this.fb.control(r))),
    });

    this.userForm.valueChanges.pipe(
      takeUntilDestroyed(this.destroyRef),
      distinctUntilChanged(),
      debounceTime(100),
      map(() => this.userForm.getRawValue() as User),
      switchMap(user => this.authService.update(user)),
    ).subscribe();
  }

  protected readonly AllLanguages = AllLanguages;
}
