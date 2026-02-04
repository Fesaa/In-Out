import {inject, Injectable, TemplateRef, Type} from '@angular/core';
import {NgbModal, NgbModalOptions, NgbModalRef} from '@ng-bootstrap/ng-bootstrap';
import {ConfirmModalComponent} from '../shared/components/confirm-modal/confirm-modal.component';
import {DefaultModalOptions} from '../_models/default-modal-options';
import {filter, firstValueFrom, from, map, take, takeUntil} from 'rxjs';
import {translate} from '@jsverse/transloco';


@Injectable({
  providedIn: 'root'
})
export class ModalService {

  private modal = inject(NgbModal);

  open<T>(content: Type<T>, options?: NgbModalOptions): [NgbModalRef, T]  {
    const modal = this.modal.open(content, options);
    return [modal, modal.componentInstance as T]
  }

  onClose$<T>(modal: NgbModalRef, requireResponse: boolean = true) {
    return modal.closed.pipe(
      takeUntil(modal.dismissed),
      take(1),
      filter(x => !requireResponse || !(x === undefined || x === null)),
      map(obj => obj as T)
    );
  }

  hasOpenModals() {
    return this.modal.hasOpenModals()
  }

  get activeInstances() {
    return this.modal.activeInstances
  }

  dismissAll(reason?: any) {
    this.modal.dismissAll(reason);
  }

  confirm(options: {
    question?: string;
    title?: string;
    bodyTemplate?: TemplateRef<unknown>;
    templateData?: unknown;
  }) {
    const [_, component] = this.open(ConfirmModalComponent, DefaultModalOptions);

    component.question.set(options.question ?? translate('confirm-modal.generic'));

    if (options.title) {
      component.title.set(options.title);
    }

    if (options.bodyTemplate) {
      component.bodyTemplate.set(options.bodyTemplate);
    }

    if (options.templateData) {
      component.templateData.set(options.templateData);
    }

    return component.result$.pipe(take(1));
  }


}
