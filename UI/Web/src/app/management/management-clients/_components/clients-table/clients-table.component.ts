import {ChangeDetectionStrategy, Component, ContentChild, input, TemplateRef, ViewChild} from '@angular/core';
import {TableComponent} from '../../../../shared/components/table/table.component';
import {Client} from '../../../../_models/client';
import {TranslocoDirective} from '@jsverse/transloco';
import {DefaultValuePipe} from '../../../../_pipes/default-value.pipe';
import {NgTemplateOutlet} from '@angular/common';

@Component({
  selector: 'app-clients-table',
  imports: [
    TableComponent,
    TranslocoDirective,
    DefaultValuePipe,
    NgTemplateOutlet
  ],
  templateUrl: './clients-table.component.html',
  styleUrl: './clients-table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ClientsTableComponent {

  @ContentChild("actions") actionsRef!: TemplateRef<any>;

  showActions = input.required<boolean>();
  clients = input.required<Client[]>();

  clientTracker(idx: number, client: Client): string {
    return `${client.id}`
  }

}
