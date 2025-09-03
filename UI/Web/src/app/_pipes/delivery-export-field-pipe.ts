import { Pipe, PipeTransform } from '@angular/core';
import { translate } from '@jsverse/transloco';
import {DeliveryExportField} from '../_models/settings';

@Pipe({
  name: 'deliveryExportField'
})
export class DeliveryExportFieldPipe implements PipeTransform {

  transform(value: DeliveryExportField): string {
    switch (value) {
      case DeliveryExportField.Id:
        return translate('delivery-export-field-pipe.id');
      case DeliveryExportField.State:
        return translate('delivery-export-field-pipe.state');
      case DeliveryExportField.FromId:
        return translate('delivery-export-field-pipe.from-id');
      case DeliveryExportField.From:
        return translate('delivery-export-field-pipe.from');
      case DeliveryExportField.RecipientId:
        return translate('delivery-export-field-pipe.recipient-id');
      case DeliveryExportField.RecipientName:
        return translate('delivery-export-field-pipe.recipient-name');
      case DeliveryExportField.RecipientEmail:
        return translate('delivery-export-field-pipe.recipient-email');
      case DeliveryExportField.CompanyNumber:
        return translate('delivery-export-field-pipe.company-number');
      case DeliveryExportField.Message:
        return translate('delivery-export-field-pipe.message');
      case DeliveryExportField.Products:
        return translate('delivery-export-field-pipe.products');
      case DeliveryExportField.CreatedUtc:
        return translate('delivery-export-field-pipe.created-utc');
      case DeliveryExportField.LastModifiedUtc:
        return translate('delivery-export-field-pipe.last-modified-utc');
      default:
        return '';
    }
  }

}
