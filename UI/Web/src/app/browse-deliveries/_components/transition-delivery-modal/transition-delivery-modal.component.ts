import {ChangeDetectionStrategy, Component, computed, inject, model, signal} from '@angular/core';
import {Delivery, DeliveryState} from '../../../_models/delivery';
import {NgbActiveModal} from '@ng-bootstrap/ng-bootstrap';
import {LoadingSpinnerComponent} from '../../../shared/components/loading-spinner/loading-spinner.component';
import {translate, TranslocoDirective} from '@jsverse/transloco';
import {DeliveryStatePipe} from '../../../_pipes/delivery-state-pipe';
import {AuthService, Role} from '../../../_services/auth.service';
import {ToastrService} from 'ngx-toastr';
import {DeliveryStateTooltipPipe} from '../../../_pipes/delivert-state-tooltip-pipe';
import {DeliveryService} from '../../../_services/delivery.service';
import {tap} from 'rxjs';

@Component({
  selector: 'app-transition-delivery-modal',
  imports: [
    LoadingSpinnerComponent,
    TranslocoDirective,
    DeliveryStatePipe,
    DeliveryStateTooltipPipe
  ],
  templateUrl: './transition-delivery-modal.component.html',
  styleUrl: './transition-delivery-modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TransitionDeliveryModalComponent {

  private readonly modal = inject(NgbActiveModal);
  private readonly authService = inject(AuthService);
  private readonly toastR = inject(ToastrService);
  private readonly deliveryService = inject(DeliveryService);

  delivery = model.required<Delivery>();

  currentState = computed(() => this.delivery().state);
  canHandleDeliveries = computed(() => this.authService.roles().includes(Role.HandleDeliveries));

  stateOptions = computed(() => {
    const cur = this.currentState();
    const canHandleDeliveries = this.canHandleDeliveries();

    switch (cur) {
      case DeliveryState.InProgress:
        return [DeliveryState.Completed, DeliveryState.Cancelled];
      case DeliveryState.Completed:
        const states = [DeliveryState.Cancelled];
        if (canHandleDeliveries) {
          states.push(DeliveryState.Handled);
          states.push(DeliveryState.InProgress)
        }
        return states;
      case DeliveryState.Handled:
        return canHandleDeliveries ? [DeliveryState.Completed] : [];
      case DeliveryState.Cancelled:
        return [];
    }
  });

  selectedNextState = signal<DeliveryState | undefined>(undefined);
  saving = signal(false);

  save() {
    const nextState = this.selectedNextState();
    if (nextState === undefined) {
      this.close();
      return;
    }

    if (!this.stateOptions().includes(nextState)) {
      this.toastR.error(translate("transition-delivery-modal.invalid-transition"));
      return;
    }

    this.deliveryService.transitionDelivery(this.delivery().id, nextState).pipe(
      tap(() => this.modal.close(nextState))
    ).subscribe();
  }

  close() {
    this.modal.close();
  }

}
