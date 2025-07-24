import {ChangeDetectionStrategy, Component, DestroyRef, EventEmitter, inject, OnInit, Output} from '@angular/core';
import {
  AllFilterComparisons,
  AllFilterFields,
  deserializeFilterFromQuery,
  Filter,
  FilterCombination,
  FilterComparison,
  FilterField,
  FilterInputType,
  serializeFilterToQuery,
  SortField
} from '../_models/filter';
import {FormArray, FormControl, FormGroup, ReactiveFormsModule} from '@angular/forms';
import {ActivatedRoute, Router} from '@angular/router';
import {DeliveryService} from '../_services/delivery.service';
import {FilterService} from '../_services/filter.service';
import {ToastrService} from 'ngx-toastr';
import {catchError, debounceTime, distinctUntilChanged, of, tap} from 'rxjs';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {FilterComparisonPipe} from '../_pipes/filter-comparison-pipe';
import {FilterFieldPipe} from '../_pipes/filter-field-pipe';
import {TypeaheadComponent} from '../type-ahead/typeahead.component';
import {TranslocoDirective} from '@jsverse/transloco';

export type FilterStatementFormGroup = FormGroup<{
  comparison: FormControl<FilterComparison>,
  field: FormControl<FilterField>,
  value: FormControl<string>
}>

@Component({
  selector: 'app-filter',
  imports: [
    FilterComparisonPipe,
    FilterFieldPipe,
    ReactiveFormsModule,
    TypeaheadComponent,
    TranslocoDirective
  ],
  templateUrl: './filter.component.html',
  styleUrl: './filter.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FilterComponent implements OnInit {

  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly filterService = inject(FilterService);

  @Output() search = new EventEmitter<Filter>();

  filterForm = new FormGroup({
    statements: new FormArray<FilterStatementFormGroup>([]),
    combination: new FormControl<FilterCombination>(FilterCombination.And, {nonNullable: true}),
    sortOptions: new FormGroup({
      sortField: new FormControl<SortField>(SortField.None, {nonNullable: true}),
      isAscending: new FormControl<boolean>(false, {nonNullable: true}),
    }),
    limit: new FormControl<number>(0, {nonNullable: true}),
  });

  get statementArray() {
    return this.filterForm.get('statements') as FormArray<FilterStatementFormGroup>;
  }

  constructor() {
    this.filterForm.valueChanges.pipe(
      debounceTime(200),
      takeUntilDestroyed(this.destroyRef),
      distinctUntilChanged(),
      tap(() => {
        const filter = this.filterForm.value as Filter;
        const query = serializeFilterToQuery(filter);
        console.log(query)

        this.router.navigateByUrl(`${this.router.url.split('?')[0]}?${query}`, { replaceUrl: true });
      }), catchError(err => {
        console.log(err)
        return of(null);
      })).subscribe();
  }

  ngOnInit(): void {
    const query = this.router.url.split('?')[1] ?? '';
    if (!query) {
      this.submit();
      return;
    }

    const filter = deserializeFilterFromQuery(query);
    filter.statements.forEach(() => {
      this.addFilterStatement();
    });

    this.filterForm.setValue(filter);
    this.submit();
  }

  addFilterStatement() {
    const group = new FormGroup({
      comparison: new FormControl<FilterComparison>(FilterComparison.Contains, {nonNullable: true}),
      field: new FormControl<FilterField>(FilterField.Recipient, {nonNullable: true}),
      value: new FormControl<string>('', {nonNullable: true}),
    });

    group.valueChanges.pipe(
      takeUntilDestroyed(this.destroyRef),
      tap(() => {
        const field = group.get('field')!.value;
        const comparison = group.get('comparison')!.value;

        const options = this.filterService.comparisonsForField(field);
        if (options.find(c => c === comparison) !== undefined) return;

        group.get('comparison')!.setValue(options[0]);
      })
    ).subscribe();

    this.statementArray.push(group);
  }

  removeFilterStatement(idx: number) {
    this.statementArray.removeAt(idx);
  }

  patchControlValue($event: any, filterStatementFormGroup: FilterStatementFormGroup) {
    filterStatementFormGroup.get('value')!.setValue(this.filterService.toFormValue($event, filterStatementFormGroup));
  }

  submit() {
    const filter = this.filterForm.value as Filter;

    filter.statements.forEach(s => {
      s.value = s.value+''; // force string
    });

    this.search.emit(filter);
  }

  trackGroup(idx: number, group: FilterStatementFormGroup) {
    const stmt = group.value;
    return `${idx}_${stmt.field}_${stmt.value}_${stmt.comparison}`;
  }

  protected readonly FilterCombination = FilterCombination;
  protected readonly AllFilterFields = AllFilterFields;
  protected readonly FilterInputType = FilterInputType;
}
