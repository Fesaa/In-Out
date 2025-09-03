import { Pipe, PipeTransform } from '@angular/core';
import { translate } from '@jsverse/transloco';
import {LogLevel} from '../_models/settings';

@Pipe({
  name: 'logLevel'
})
export class LogLevelPipe implements PipeTransform {

  transform(value: LogLevel): string {
    switch (value) {
      case LogLevel.Verbose:
        return translate('log-level-pipe.verbose');
      case LogLevel.Debug:
        return translate('log-level-pipe.debug');
      case LogLevel.Information:
        return translate('log-level-pipe.information');
      case LogLevel.Warning:
        return translate('log-level-pipe.warning');
      case LogLevel.Error:
        return translate('log-level-pipe.error');
      case LogLevel.Fatal:
        return translate('log-level-pipe.fatal');
      default:
        return '';
    }
  }

}
