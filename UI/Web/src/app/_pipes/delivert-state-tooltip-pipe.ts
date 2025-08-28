import {Pipe, PipeTransform} from '@angular/core';
import {DeliveryState} from '../_models/delivery';
import {translate} from '@jsverse/transloco';

@Pipe({
  name: 'deliveryStateTooltip'
})
export class DeliveryStateTooltipPipe implements PipeTransform {

  transform(value: DeliveryState | string | undefined): string {
    if (!value) return '';

    if (typeof value === "string") {
      value = parseInt(value) as DeliveryState;
    }

    switch (value) {
      case DeliveryState.InProgress:
        return translate('delivery-state-tooltips.in-progress');
      case DeliveryState.Completed:
        return translate('delivery-state-tooltips.completed');
      case DeliveryState.Handled:
        return translate('delivery-state-tooltips.handled');
      case DeliveryState.Cancelled:
        return translate('delivery-state-tooltips.cancelled');
    }
  }

}
