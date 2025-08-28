import {Pipe, PipeTransform} from '@angular/core';
import {translate} from '@jsverse/transloco';
import {FilterField} from '../_models/filter';

@Pipe({
  name: 'filterField',
  standalone: true,
  pure: true,
})
export class FilterFieldPipe implements PipeTransform {
  transform(value: FilterField): string {
    switch (value) {
      case FilterField.DeliveryState:
        return translate('filter-field-pipe.delivery-state');
      case FilterField.From:
        return translate('filter-field-pipe.from');
      case FilterField.Recipient:
        return translate('filter-field-pipe.recipient');
      case FilterField.Lines:
        return translate('filter-field-pipe.lines');
      case FilterField.Products:
        return translate('filter-field-pipe.products');
      case FilterField.Created:
        return translate('filter-field-pipe.created');
      case FilterField.LastModified:
        return translate('filter-field-pipe.last-modified');
      default:
        return translate('filter-field-pipe.unknown');
    }
  }
}
