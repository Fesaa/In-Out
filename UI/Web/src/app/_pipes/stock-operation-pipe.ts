import {Pipe, PipeTransform} from '@angular/core';
import {StockOperation} from '../_models/stock';
import {translate} from '@jsverse/transloco';

@Pipe({
  name: 'stockOperation'
})
export class StockOperationPipe implements PipeTransform {

  transform(value: StockOperation): string {
    switch (value) {
      case StockOperation.Add:
        return translate('stock-operation-pipe.add');
      case StockOperation.Remove:
        return translate('stock-operation-pipe.remove');
      case StockOperation.Set:
        return translate('stock-operation-pipe.set');
    }
  }

}
