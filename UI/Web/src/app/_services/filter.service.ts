import {inject, Injectable} from '@angular/core';
import {FilterComparison, FilterField, FilterInputType} from '../_models/filter';
import {TypeaheadSettings} from '../type-ahead/typeahead.component';
import {ClientService} from './client.service';
import {forkJoin, map, Observable, of, tap} from 'rxjs';
import {UserService} from './user.service';
import {ProductService} from './product.service';
import {DeliveryStatePipe} from '../_pipes/delivery-state-pipe';
import {AllDeliveryStates, DeliveryState} from '../_models/delivery';
import {Product} from '../_models/product';
import {Client} from '../_models/client';
import {User} from '../_models/user';
import {FilterStatementFormGroup} from '../filter/filter.component';

@Injectable({
  providedIn: 'root'
})
export class FilterService {

  private readonly clientService = inject(ClientService);
  private readonly userService = inject(UserService);
  private readonly productService = inject(ProductService);
  private readonly deliveryStatePipe = inject(DeliveryStatePipe);

  getTypeaheadSettings(group: FilterStatementFormGroup): TypeaheadSettings<unknown> {
    const comparison = group.get('comparison')!.value;

    const settings = new TypeaheadSettings();
    settings.multiple = this.isMultiComparison(comparison);
    settings.unique = true;
    settings.minCharacters = 0;
    settings.preLoadMethod = this.fromFormValue(group);
    settings.fetchFn = this.fetchFn(group);

    return settings;
  }

  private fetchFn(group: FilterStatementFormGroup): (f: string) => Observable<any> {
    return (f) => {
      const field = group.get('field')!.value;
      switch (field) {
        case FilterField.Recipient:
          return this.clientService.search(f);
        case FilterField.From:
          return this.userService.search(f);
        case FilterField.Products:
          return this.productService.allProducts(true).pipe(
            map(products => products.filter(p => p.name.toLowerCase().includes(f.toLowerCase())))
          );
        case FilterField.DeliveryState:
          return of(AllDeliveryStates.filter(d => this.deliveryStatePipe.transform(d).toLowerCase().includes(f.toLowerCase())))
      }

      return of([]);
    }
  }

  private isMultiComparison(comparison: FilterComparison): boolean {
    switch (comparison) {
      case FilterComparison.Contains:
      case FilterComparison.NotContains:
      case FilterComparison.StartsWith:
      case FilterComparison.EndsWith:
        return true;

      case FilterComparison.Equals:
      case FilterComparison.NotEquals:
      case FilterComparison.GreaterThan:
      case FilterComparison.GreaterThanOrEquals:
      case FilterComparison.LessThan:
      case FilterComparison.LessThanOrEquals:
        return false;

      default:
        return false;
    }
  }

  comparisonsForField(field: FilterField): FilterComparison[] {
    switch (field) {
      case FilterField.DeliveryState:
        return [
          FilterComparison.Equals,
          FilterComparison.NotEquals,
          FilterComparison.Contains,
          FilterComparison.NotContains,
        ];

      case FilterField.From:
      case FilterField.Recipient:
        return [
          FilterComparison.Equals,
          FilterComparison.NotEquals,
          FilterComparison.Contains,
          FilterComparison.NotContains,
        ];

      case FilterField.Products:
        return [
          FilterComparison.Equals,
          FilterComparison.NotEquals,
          FilterComparison.Contains,
          FilterComparison.NotContains,
        ];

      case FilterField.Lines:
        return [
          FilterComparison.Equals,
          FilterComparison.NotEquals,
          FilterComparison.GreaterThan,
          FilterComparison.GreaterThanOrEquals,
          FilterComparison.LessThan,
          FilterComparison.LessThanOrEquals,
        ];

      case FilterField.Created:
      case FilterField.LastModified:
        return [
          FilterComparison.Equals,
          FilterComparison.NotEquals,
          FilterComparison.GreaterThan,
          FilterComparison.GreaterThanOrEquals,
          FilterComparison.LessThan,
          FilterComparison.LessThanOrEquals,
        ];

      default:
        return [FilterComparison.Equals];
    }
  }

  fieldOptionLabel(field: FilterField, value: any) {
    switch (field) {
      case FilterField.Recipient:
        return (value as Client).name;
      case FilterField.From:
        return (value as User).name
      case FilterField.DeliveryState:
        return this.deliveryStatePipe.transform((value as DeliveryState));
      case FilterField.Products:
        return (value as Product).name;
      default:
        return value;
    }
  }

  getFieldInputType(field: FilterField): FilterInputType {
    switch (field) {
      case FilterField.DeliveryState:
      case FilterField.Products:
      case FilterField.Recipient:
      case FilterField.From:
        return FilterInputType.Typeahead;
      case FilterField.LastModified:
      case FilterField.Created:
        return FilterInputType.Date;
      case FilterField.Lines:
        return FilterInputType.FreeNumber;
      default:
          throw new Error(`Unknown filter type: ${field}`);
    }
  }

  toFormValue(value: any, group: FilterStatementFormGroup): string {
    const field = group.get('field')!.value;
    const comparison = group.get('comparison')!.value;

    const isMulti = this.isMultiComparison(comparison);

    switch (field) {
      case FilterField.Recipient:
        return isMulti ? (value as Client[]).map(c => c.id).join(',') : (value as Client).id+'';
      case FilterField.From:
        return isMulti ? (value as User[]).map(u => u.id).join(',') : (value as User).id+'';
      case FilterField.DeliveryState:
        return isMulti ? (value as DeliveryState[]).join(',') : (value as DeliveryState)+'';
      case FilterField.Products:
        return isMulti ? (value as Product[]).map(u => u.id).join(',') : (value as Product[])+'';
      case FilterField.LastModified:
      case FilterField.Created:
        return (value as Date).toISOString();
      case FilterField.Lines:  // Always a number
        return value+'';

    }
  }

  private fromFormValue(group: FilterStatementFormGroup): Observable<unknown> {
    const field = group.get('field')!.value;
    const comparison = group.get('comparison')!.value;
    const value = group.get('value')!.value;
    if (!value) return of();

    const isMulti = this.isMultiComparison(comparison);
    const ids = isMulti ? value.split(',') : [value];

    const multiTransformer$ = map((data: any[]) => {
      if (isMulti) return data;

      return data.length > 0 ? data[0] : undefined;
    });

    switch (field) {
      case FilterField.Recipient:
        return this.clientService.getByIds(ids.map(id => parseInt(id))).pipe(multiTransformer$);
      case FilterField.From:
        return this.userService.getByIds(ids.map(id => parseInt(id))).pipe(multiTransformer$);
      case FilterField.DeliveryState:
        return of(ids.map(v => parseInt(v) as DeliveryState)).pipe(multiTransformer$);
      case FilterField.Products:
        return this.productService.getByIds(ids.map(id => parseInt(id))).pipe(multiTransformer$);
      case FilterField.LastModified:
      case FilterField.Created:
        return of(new Date(value));

      case FilterField.Lines:
        return of(Number(value));
    }

  }


}
