import { Pipe, PipeTransform } from '@angular/core';
import {DeliveryState} from '../_models/delivery';
import {translate} from '@jsverse/transloco';

@Pipe({
  name: 'deliveryState',
  standalone: true,
  pure: true,
})
export class DeliveryStatePipe implements PipeTransform {
  transform(value: DeliveryState): string {
    switch (value) {
      case DeliveryState.InProgress:
        return translate('delivery-state-pipe.in-progress');
      case DeliveryState.Completed:
        return translate('delivery-state-pipe.completed');
      case DeliveryState.Handled:
        return translate('delivery-state-pipe.handled');
      case DeliveryState.Cancelled:
        return translate('delivery-state-pipe.cancelled');
      default:
        return translate('delivery-state-pipe.unknown');
    }
  }
}
