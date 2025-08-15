import {ChangeDetectionStrategy, Component, computed, input} from '@angular/core';

export type BadgeColour = 'primary' | 'secondary' | 'error' | 'warning';

@Component({
  selector: 'app-badge',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span
      [class]="badgeClass()"
      [class.clickable]="clickAble()"
      role="status"
    >
      <ng-content></ng-content>
    </span>
  `,
  styleUrl: `./badge.component.scss`,
})
export class BadgeComponent {

  colour = input<BadgeColour>('primary');
  clickAble = input(false);

  badgeClass = computed(() => `badge badge--${this.colour()}`);
}
