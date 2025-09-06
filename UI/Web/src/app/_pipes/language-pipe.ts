import { Pipe, PipeTransform } from '@angular/core';
import {AllLanguages} from '../_models/user';
import {translate} from '@jsverse/transloco';

@Pipe({
  name: 'language'
})
export class LanguagePipe implements PipeTransform {

  transform(value: string): string {
    if (!AllLanguages.includes(value)) return translate('language-pipe.nal')

    switch (value.toLowerCase()) {
      case 'en':
        return translate('language-pipe.en')
      case 'nl':
        return translate('language-pipe.nl')
    }

    return translate(value)
  }

}
