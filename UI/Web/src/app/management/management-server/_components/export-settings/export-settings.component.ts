import {ChangeDetectionStrategy, Component, input, signal, computed, OnInit, inject} from '@angular/core';
import {Settings, DeliveryExportField, DeliveryExportFieldValues} from '../../../../_models/settings';
import {TranslocoDirective} from '@jsverse/transloco';
import {CdkDragDrop, CdkDrag, CdkDropList, moveItemInArray} from '@angular/cdk/drag-drop';
import {DeliveryExportFieldPipe} from '../../../../_pipes/delivery-export-field-pipe';
import {SettingsService} from '../../../../_services/settings.service';

@Component({
  selector: 'app-export-settings',
  imports: [
    TranslocoDirective,
    CdkDropList,
    CdkDrag,
    DeliveryExportFieldPipe
  ],
  templateUrl: './export-settings.component.html',
  styleUrl: './export-settings.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExportSettingsComponent implements OnInit {

  private readonly settingsService = inject(SettingsService);

  settings = input.required<Settings>();

  selectedFields = signal<DeliveryExportField[]>([]);
  headerNames = signal<string[]>([]);

  availableFields = computed(() => {
    const selected = this.selectedFields();
    return DeliveryExportFieldValues.filter(field => !selected.includes(field));
  });

  drop(event: CdkDragDrop<DeliveryExportField[]>) {
    if (event.previousContainer === event.container) {
      const fields = [...this.selectedFields()];
      const names = [...this.headerNames()];

      moveItemInArray(fields, event.previousIndex, event.currentIndex);
      moveItemInArray(names, event.previousIndex, event.currentIndex);

      this.selectedFields.set(fields);
      this.headerNames.set(names);
      this.updateSettings();
      return;
    }

    if (event.container.id === 'selectedList' || event.container.element.nativeElement.classList.contains('selections-row')) {
      const field = event.item.data as DeliveryExportField;
      const newSelected = [...this.selectedFields()];
      const newNames = [...this.headerNames()];

      newSelected.splice(event.currentIndex, 0, field);
      newNames.splice(event.currentIndex, 0, this.getDefaultFieldName(field));

      this.selectedFields.set(newSelected);
      this.headerNames.set(newNames);
      this.updateSettings();
      return;
    }

    const newSelected = [...this.selectedFields()];
    const newNames = [...this.headerNames()];

    newSelected.splice(event.previousIndex, 1);
    newNames.splice(event.previousIndex, 1);

    this.selectedFields.set(newSelected);
    this.headerNames.set(newNames);
    this.updateSettings();
  }

  private getDefaultFieldName(field: DeliveryExportField): string {
    const defaultNames: Record<DeliveryExportField, string> = {
      [DeliveryExportField.Id]: 'ID',
      [DeliveryExportField.State]: 'State',
      [DeliveryExportField.FromId]: 'From ID',
      [DeliveryExportField.From]: 'From',
      [DeliveryExportField.RecipientId]: 'Recipient ID',
      [DeliveryExportField.RecipientName]: 'Recipient Name',
      [DeliveryExportField.RecipientEmail]: 'Recipient Email',
      [DeliveryExportField.CompanyNumber]: 'Company Number',
      [DeliveryExportField.Message]: 'Message',
      [DeliveryExportField.Products]: 'Products',
      [DeliveryExportField.CreatedUtc]: 'Created',
      [DeliveryExportField.LastModifiedUtc]: 'Last Modified'
    };

    return defaultNames[field] || 'Unknown Field';
  }

  private updateSettings() {
    const updatedSettings: Settings = {
      ...this.settings(),
      csvExportConfiguration: {
        headerNames: this.headerNames(),
        headerOrder: this.selectedFields()
      }
    };

    this.settingsService.updateSettings(updatedSettings).subscribe();
  }

  ngOnInit(): void {
    const currentSettings = this.settings();
    this.selectedFields.set([...currentSettings.csvExportConfiguration.headerOrder]);
    this.headerNames.set([...currentSettings.csvExportConfiguration.headerNames]);
  }
}
