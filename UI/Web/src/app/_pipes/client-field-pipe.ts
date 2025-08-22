import {Pipe, PipeTransform} from '@angular/core';
import {
  ClientField
} from '../management/management-clients/_components/import-client-modal/import-client-modal.component';
import {translate} from '@jsverse/transloco';

@Pipe({
  name: 'clientField'
})
export class ClientFieldPipe implements PipeTransform {

  transform(value: ClientField): string {
    switch (value) {
      case ClientField.Name:
        return translate('client-field-pipe.name');
      case ClientField.Address:
        return translate('client-field-pipe.address');
      case ClientField.CompanyNumber:
        return translate('client-field-pipe.company-number');
      case ClientField.ContactEmail:
        return translate('client-field-pipe.contact-email');
      case ClientField.ContactName:
        return translate('client-field-pipe.contact-name');
      case ClientField.ContactNumber:
        return translate('client-field-pipe.contact-number');
      case ClientField.InvoiceEmail:
        return translate('client-field-pipe.invoice-email');
    }
  }

}
