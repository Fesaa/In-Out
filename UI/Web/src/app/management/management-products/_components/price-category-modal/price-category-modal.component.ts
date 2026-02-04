import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, model, OnInit, signal } from '@angular/core';
import { PriceCategory } from '../../../../_models/product';
import { ProductService } from '../../../../_services/product.service';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner.component';
import { TranslocoDirective } from '@jsverse/transloco';
import { SettingsItemComponent } from '../../../../shared/components/settings-item/settings-item.component';
import {DefaultValuePipe} from '@inout/_pipes/default-value.pipe';

@Component({
  selector: 'app-price-category-modal',
  standalone: true,
  imports: [
    LoadingSpinnerComponent,
    TranslocoDirective,
    ReactiveFormsModule,
    SettingsItemComponent,
    DefaultValuePipe
  ],
  templateUrl: './price-category-modal.component.html',
  styleUrl: './price-category-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PriceCategoryModalComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly cdRef = inject(ChangeDetectorRef);
  protected readonly modal = inject(NgbActiveModal);

  priceCategory = model<PriceCategory>({ id: -1, name: '' });
  existingPriceCategories = model<PriceCategory[]>([]);

  isSaving = signal(false);
  priceCategoryForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.minLength(2)])
  });

  ngOnInit(): void {
    if (this.priceCategory().id !== -1) {
      this.priceCategoryForm.patchValue({
        name: this.priceCategory().name
      });
    }
    this.cdRef.markForCheck();
  }

  save() {
    if (this.priceCategoryForm.invalid) return;

    this.isSaving.set(true);
    const id = this.priceCategory().id;
    const name = this.priceCategoryForm.value.name!;

    const payload: PriceCategory = {
      id: id,
      name: name
    };

    const action$ = id === -1
      ? this.productService.createPriceCategory(payload)
      : this.productService.updatePriceCategory(payload);

    action$.subscribe({
      next: () => this.modal.close(),
      error: (err) => {
        console.error(err);
        this.isSaving.set(false);
        this.cdRef.markForCheck();
      }
    });
  }

  close() {
    this.modal.dismiss();
  }
}
