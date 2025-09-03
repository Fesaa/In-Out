import { ChangeDetectionStrategy, Component } from '@angular/core';
import {RouterLink} from "@angular/router";
import {TranslocoDirective} from "@jsverse/transloco";

@Component({
  selector: 'app-under-construction',
    imports: [
        RouterLink,
        TranslocoDirective
    ],
  templateUrl: './under-construction.component.html',
  styleUrl: './under-construction.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UnderConstructionComponent {

}
