import {ChangeDetectionStrategy, Component} from '@angular/core';
import {UnderConstructionComponent} from '../../shared/components/under-construction/under-construction.component';

@Component({
  selector: 'app-management-server',
  imports: [
    UnderConstructionComponent
  ],
  templateUrl: './management-server.component.html',
  styleUrl: './management-server.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManagementServerComponent {

}
