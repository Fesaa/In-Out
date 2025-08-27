import {Pipe, PipeTransform} from '@angular/core';
import {ProductType} from '../_models/product';
import {translate} from '@jsverse/transloco';

@Pipe({
  name: 'productType'
})
export class ProductTypePipe implements PipeTransform {

  transform(value: ProductType | string): string {
    if (typeof value === 'string') {
      value = parseInt(value, 10) as ProductType;
    }

    switch (value) {
      case ProductType.OneTime:
        return translate('product-type-pipe.one-time');
      case ProductType.Consumable:
        return translate('product-type-pipe.consumable');
    }
  }

}
