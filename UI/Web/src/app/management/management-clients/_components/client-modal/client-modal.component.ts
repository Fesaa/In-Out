import {ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, model, OnInit, signal} from '@angular/core';
import {TranslocoDirective} from '@jsverse/transloco';
import {LoadingSpinnerComponent} from '../../../../shared/components/loading-spinner/loading-spinner.component';
import {Client} from '../../../../_models/client';
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {ModalDismissReasons, NgbActiveModal} from '@ng-bootstrap/ng-bootstrap';
import {ClientService} from '../../../../_services/client.service';
import {SettingsItemComponent} from '../../../../shared/components/settings-item/settings-item.component';

@Component({
  selector: 'app-client-modal',
  imports: [
    TranslocoDirective,
    LoadingSpinnerComponent,
    FormsModule,
    ReactiveFormsModule,
    SettingsItemComponent
  ],
  templateUrl: './client-modal.component.html',
  styleUrl: './client-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientModalComponent implements OnInit {

  private readonly clientService = inject(ClientService);
  private readonly cdRef = inject(ChangeDetectorRef);
  protected readonly modal = inject(NgbActiveModal);

  client = model<Client>({
    id: -1,
    name: '',
    address: '',
    companyNumber: '',
    contactEmail: '',
    contactName: '',
    contactNumber: '',
    invoiceEmail: ''
  });

  isSaving = signal(false);

  clientForm = new FormGroup({})

  ngOnInit(): void {
    const client = this.client();

    this.clientForm.addControl('name', new FormControl(client.name, [Validators.required]),);
    this.clientForm.addControl('address', new FormControl(client.address));
    this.clientForm.addControl('companyNumber', new FormControl(client.companyNumber));
    this.clientForm.addControl('contactEmail', new FormControl(client.contactEmail));
    this.clientForm.addControl('contactName', new FormControl(client.contactName));
    this.clientForm.addControl('contactNumber', new FormControl(client.contactNumber));
    this.clientForm.addControl('invoiceEmail', new FormControl(client.invoiceEmail));
  }

  save() {
    const client = this.clientForm.value as Client;
    if (this.client().id !== -1) {
      client.id = this.client().id;
    }

    const action$ = this.client().id === -1
      ? this.clientService.create(client)
      : this.clientService.update(client);

    action$.subscribe({
      next: () => this.close(),
      error: error => {
        console.error(error);
      }
    });
  }

  close() {
    this.modal.close();
  }

}
