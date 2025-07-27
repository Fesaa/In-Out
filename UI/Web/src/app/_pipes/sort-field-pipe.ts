import {Pipe, PipeTransform} from '@angular/core';
import {SortField} from '../_models/filter';
import {translate} from '@jsverse/transloco';

@Pipe({
  name: 'sortField'
})
export class SortFieldPipe implements PipeTransform {

  transform(value: SortField): string {
    switch (value) {
      case SortField.From:
        return translate('sort-field-pipe.from');
      case SortField.Recipient:
        return translate('sort-field-pipe.recipient');
      case SortField.CreationDate:
        return translate('sort-field-pipe.creation-date')
    }
  }

}
