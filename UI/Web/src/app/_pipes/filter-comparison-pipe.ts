import { Pipe, PipeTransform } from '@angular/core';
import {translate} from '@jsverse/transloco';
import {FilterComparison} from '../_models/filter';

@Pipe({
  name: 'filterComparison',
  standalone: true,
  pure: true,
})
export class FilterComparisonPipe implements PipeTransform {
  transform(value: FilterComparison): string {
    switch (value) {
      case FilterComparison.Contains:
        return translate('filter-comparison-pipe.contains');
      case FilterComparison.NotContains:
        return translate('filter-comparison-pipe.not-contains');
      case FilterComparison.Equals:
        return translate('filter-comparison-pipe.equals');
      case FilterComparison.NotEquals:
        return translate('filter-comparison-pipe.not-equals');
      case FilterComparison.StartsWith:
        return translate('filter-comparison-pipe.starts-with');
      case FilterComparison.EndsWith:
        return translate('filter-comparison-pipe.ends-with');
      case FilterComparison.GreaterThan:
        return translate('filter-comparison-pipe.greater-than');
      case FilterComparison.GreaterThanOrEquals:
        return translate('filter-comparison-pipe.greater-than-or-equals');
      case FilterComparison.LessThan:
        return translate('filter-comparison-pipe.less-than');
      case FilterComparison.LessThanOrEquals:
        return translate('filter-comparison-pipe.less-than-or-equals');
      default:
        return translate('filter-comparison-pipe.unknown');
    }
  }
}
