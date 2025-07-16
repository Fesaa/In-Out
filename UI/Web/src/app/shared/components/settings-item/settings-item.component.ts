/**
 * This component has been adjusted from https://github.com/Kareadita/Kavita
 */
import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component, ContentChild,
  ElementRef, HostListener,
  inject,
  input, model,
  TemplateRef
} from '@angular/core';
import {AbstractControl} from '@angular/forms';
import {TranslocoDirective} from '@jsverse/transloco';
import {NgClass, NgTemplateOutlet} from '@angular/common';
import {SafeHtmlPipe} from '../../../_pipes/safe-html-pipe';

@Component({
  selector: 'app-settings-item',
  imports: [
    TranslocoDirective,
    NgTemplateOutlet,
    NgClass,
    SafeHtmlPipe,
  ],
  templateUrl: './settings-item.component.html',
  styleUrl: './settings-item.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SettingsItemComponent {

  private readonly cdRef = inject(ChangeDetectorRef);
  private readonly elementRef = inject(ElementRef);

  control = input.required<AbstractControl>();

  title = input<string>();
  subtitle = input<string>();
  labelId = input<string>();

  editLabel = input<string>();
  canEdit = input(true);
  showEdit = input(true);
  isEditMode = model(false);

  toggleOnViewClick = input(true);

  /**
   * View in View mode
   */
  @ContentChild('view') valueViewRef!: TemplateRef<any>;
  /**
   * View in Edit mode
   */
  @ContentChild('edit') valueEditRef!: TemplateRef<any>;

  @HostListener('click', ['$event'])
  onClickInside(event: MouseEvent) {
    event.stopPropagation(); // Prevent the click from bubbling up
  }

  toggleEditMode() {
    if (!this.toggleOnViewClick()) return;
    if (!this.canEdit()) return;
    if (this.isEditMode() && this.control().dirty && this.control().invalid) return;

    this.isEditMode.update(b => !b);
  }

}
