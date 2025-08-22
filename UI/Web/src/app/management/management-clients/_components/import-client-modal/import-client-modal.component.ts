import {ChangeDetectionStrategy, Component, computed, inject, signal} from '@angular/core';
import {LoadingSpinnerComponent} from '../../../../shared/components/loading-spinner/loading-spinner.component';
import {translate, TranslocoDirective} from '@jsverse/transloco';
import {ClientService} from '../../../../_services/client.service';
import {NgbActiveModal} from '@ng-bootstrap/ng-bootstrap';
import {FormArray, FormControl, FormGroup, NonNullableFormBuilder, ReactiveFormsModule} from '@angular/forms';
import {FileUploadComponent, FileUploadValidators} from '@iplab/ngx-file-upload';
import {map} from 'rxjs';
import {toSignal} from '@angular/core/rxjs-interop';
import {ToastrService} from 'ngx-toastr';
import Papa from 'papaparse';
import {SettingsItemComponent} from '../../../../shared/components/settings-item/settings-item.component';
import {ClientFieldPipe} from '../../../../_pipes/client-field-pipe';
import {Client} from '../../../../_models/client';
import {TableComponent} from '../../../../shared/components/table/table.component';
import {DefaultValuePipe} from '../../../../_pipes/default-value.pipe';
import {ClientsTableComponent} from '../clients-table/clients-table.component';

enum StageId {
  FileImport = 'file-import',
  HeaderMatch = 'header-match',
  Confirm  = 'confirm',
}

export enum ClientField {
  Name = 0,
  Address = 1,
  CompanyNumber = 2,
  InvoiceEmail = 3,
  ContactName = 4,
  ContactEmail = 5,
  ContactNumber = 6,
}

const fields: ClientField[] = [ClientField.Name, ClientField.Address, ClientField.CompanyNumber,
ClientField.InvoiceEmail, ClientField.ContactName, ClientField.ContactEmail, ClientField.ContactNumber];

type HeaderMappingControl = FormGroup<{
  header: FormControl<string>,
  field: FormControl<ClientField>,
  index: FormControl<number>
}>;

@Component({
  selector: 'app-import-client-modal',
  imports: [
    LoadingSpinnerComponent,
    TranslocoDirective,
    ReactiveFormsModule,
    FileUploadComponent,
    SettingsItemComponent,
    ClientFieldPipe,
    TableComponent,
    DefaultValuePipe,
    ClientsTableComponent,
  ],
  templateUrl: './import-client-modal.component.html',
  styleUrl: './import-client-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ImportClientModalComponent {

  private readonly clientService = inject(ClientService);
  protected readonly modal = inject(NgbActiveModal);
  private readonly toastr = inject(ToastrService);
  private readonly fb = inject(NonNullableFormBuilder);

  fileUploadControl = new FormControl<undefined | Array<File>>(undefined, [
    FileUploadValidators.accept(['.csv']), FileUploadValidators.filesLimit(1)
  ]);

  uploadForm = new FormGroup({
    files: this.fileUploadControl,
  });

  headerMatchForm: FormGroup<{headers: FormArray<HeaderMappingControl>}> = new FormGroup({
    headers: new FormArray<HeaderMappingControl>([])
  });

  isSaving = signal(false);
  currentStage = signal(StageId.FileImport);
  // CSV data without header
  data = signal<string[][]>([]);
  clients = signal<Client[]>([]);

  isFileSelected = toSignal(this.uploadForm.get('files')!.valueChanges
    .pipe(map((files) => !!files && files.length == 1)), {initialValue: false});

  buttonLabel = computed(() => {
    switch (this.currentStage()) {
      case StageId.FileImport:
        return translate('import-client-modal.next');
      case StageId.HeaderMatch:
        return translate('import-client-modal.next');
      case StageId.Confirm :
        return translate('import-client-modal.import');
    }
  });
  canMoveToNext = computed(() => {
    switch (this.currentStage()) {
      case StageId.FileImport:
        return this.isFileSelected();
      case StageId.HeaderMatch:
        return this.headerMatchForm.valid;
      case StageId.Confirm :
        return true;
    }
  });

  get headerArray(): FormArray<HeaderMappingControl> {
    return this.headerMatchForm.get('headers')! as FormArray<HeaderMappingControl>;
  }

  async nextStep() {
    switch (this.currentStage()) {
      case StageId.FileImport:
        await this.handleFileImport();
        break;
      case StageId.HeaderMatch:
        await this.constructClients();
        break;
      case StageId.Confirm:
        this.import();
        break;
    }
  }

  private async handleFileImport() {
    const files = this.fileUploadControl.value;
    if (!files || files.length === 0) {
      this.toastr.error(translate('import-client-modal.select-files-warning'));
      return;
    }

    const file = files[0];
    const text = await file.text();
    const res = Papa.parse<string[]>(text, {header: false, delimiter: ','});
    if (res.errors.length > 0) {
      console.log(res);
      this.toastr.error(translate('import-client-modal.parse-error'));
      return;
    }

    const data = res.data;
    if (data.length < 2) {
      this.toastr.error(translate('import-client-modal.header-only-or-none'));
      return;
    }

    const headers = data[0];
    this.headerArray.setValue([]);

    headers.forEach((header, idx) => {
      this.headerArray.push(this.fb.group({
        header: this.fb.control(header),
        field: this.fb.control(fields[idx % fields.length]),
        index: this.fb.control(idx),
      }));
    });

    this.data.set(res.data.slice(1).filter(d => d.length === headers.length));
    this.currentStage.set(StageId.HeaderMatch);
  }

  private async constructClients() {
    const mappings = this.headerArray.controls.map(c => c.value);

    const clients: Client[] = [];

    for (const dataRow of this.data()) {

      const client: Partial<Client> = {};


      for (const field of fields) {
        const mapping = mappings.find(mapping => mapping.field === field);
        const value = (mapping !== undefined && mapping.index !== undefined) ? dataRow[mapping.index] : '';

        switch (field) {
          case ClientField.Name:
            client.name = value;
            break;
          case ClientField.Address:
            client.address = value;
            break;
          case ClientField.ContactEmail:
            client.contactEmail = value;
            break;
          case ClientField.CompanyNumber:
            client.companyNumber = value;
            break;
          case ClientField.InvoiceEmail:
            client.invoiceEmail = value;
            break;
          case ClientField.ContactNumber:
            client.contactNumber = value;
            break;
          case ClientField.ContactName:
            client.contactName = value;
            break;
        }
      }

      clients.push(client as Client);
    }

    this.clients.set(clients);
    this.currentStage.set(StageId.Confirm);
  }

  private import() {
    this.clientService.createBulk(this.clients()).subscribe({
      next: () => this.close(),
    });
  }

  close() {
    this.modal.close();
  }

  protected readonly StageId = StageId;
  protected readonly fields = fields;
  protected readonly JSON = JSON;
}
