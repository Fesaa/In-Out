import {ChangeDetectionStrategy, Component, computed, inject, OnInit, signal} from '@angular/core';
import {TranslocoDirective} from "@jsverse/transloco";
import {ModalService} from '../../_services/modal.service';
import {Client} from '../../_models/client';
import {ClientService} from '../../_services/client.service';
import {LoadingSpinnerComponent} from '../../shared/components/loading-spinner/loading-spinner.component';
import {ClientModalComponent} from './_components/client-modal/client-modal.component';
import {DefaultModalOptions} from '../../_models/default-modal-options';
import {ImportClientModalComponent} from './_components/import-client-modal/import-client-modal.component';
import {ClientsTableComponent} from './_components/clients-table/clients-table.component';

@Component({
  selector: 'app-management-clients',
  imports: [
    TranslocoDirective,
    LoadingSpinnerComponent,
    ClientsTableComponent,
  ],
  templateUrl: './management-clients.component.html',
  styleUrl: './management-clients.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManagementClientsComponent implements OnInit {

  private readonly modalService = inject(ModalService);
  private readonly clientSerivce = inject(ClientService);

  loading = signal(true);
  clients = signal<Client[]>([]);

  clientWarning = computed(() => !this.loading() && this.clients().length === 0);

  ngOnInit(): void {
    this.loadClients();
  }

  private loadClients() {
    this.loading.set(true);
    this.clientSerivce.getAll().subscribe(clients => {
      this.clients.set(clients);
      this.loading.set(false);
    });
  }

  clientTracker(idx: number, client: Client): string {
    return `${client.id}`
  }

  importClients() {
    const [modal, component] = this.modalService.open(ImportClientModalComponent, DefaultModalOptions);

    modal.closed.subscribe(() => this.loadClients());
  }

  createOrUpdateClient(client?: Client) {
    const [modal, component] = this.modalService.open(ClientModalComponent, DefaultModalOptions);
    if (client) {
      component.client.set(client);
    }

    modal.closed.subscribe(() => this.loadClients());
  }

  async deleteClient(client: Client) {
    if (!await this.modalService.confirm({})) {
      return;
    }

    this.clientSerivce.delete(client.id).subscribe(() => this.loadClients());
  }

}
