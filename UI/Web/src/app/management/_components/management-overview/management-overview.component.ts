import { ChangeDetectionStrategy, Component } from '@angular/core';
import {TableComponent} from "../../../shared/components/table/table.component";
import {TranslocoDirective} from "@jsverse/transloco";

@Component({
  selector: 'app-management-overview',
    imports: [
      TranslocoDirective
    ],
  templateUrl: './management-overview.component.html',
  styleUrl: './management-overview.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManagementOverviewComponent {

}
