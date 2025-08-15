import {ChangeDetectionStrategy, Component, computed, effect, inject, OnInit, signal} from '@angular/core';
import {Delivery, DeliveryLine, DeliveryState} from '../_models/delivery';
import {Product, ProductCategory, ProductType} from '../_models/product';
import {catchError, forkJoin, of, switchMap, take, tap} from 'rxjs';
import {ProductService} from '../_services/product.service';
import {UserService} from '../_services/user.service';
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {DeliveryService} from '../_services/delivery.service';
import {CommonModule} from '@angular/common';
import {translate, TranslocoDirective} from '@jsverse/transloco';
import {TypeaheadComponent, TypeaheadSettings} from '../type-ahead/typeahead.component';
import {Client} from '../_models/client';
import {ClientService} from '../_services/client.service';
import {User} from '../_models/user';
import {AuthService, Role} from '../_services/auth.service';
import {ToastrService} from 'ngx-toastr';
import {
  TransitionDeliveryModalComponent
} from '../browse-deliveries/_components/transition-delivery-modal/transition-delivery-modal.component';
import {DefaultModalOptions} from '../_models/default-modal-options';
import {ModalService} from '../_services/modal.service';

@Component({
  selector: 'app-manage-delivery',
  imports: [CommonModule, ReactiveFormsModule, TranslocoDirective, TypeaheadComponent, RouterLink],
  templateUrl: './manage-delivery.component.html',
  styleUrl: './manage-delivery.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageDeliveryComponent implements OnInit {

  protected readonly authService = inject(AuthService);
  protected readonly ProductType = ProductType;
  protected readonly Role = Role;

  private readonly modalService = inject(ModalService);
  private readonly deliveryService = inject(DeliveryService);
  private readonly productService = inject(ProductService);
  private readonly userService = inject(UserService);
  private readonly clientService = inject(ClientService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toastr = inject(ToastrService);

  loading = signal(true);
  submitting = signal(false);
  totalItems = signal(0);
  collapsedCategories = signal<Set<number>>(new Set());

  sortedCategories = computed(() => {
    return this.categories().sort((a, b) => a.sortValue - b.sortValue);
  });
  groupedProducts = computed<Map<number, Product[]>>(() => {
    const products = this.products();
    const map = new Map<number, Product[]>();
    products.forEach(p => {
      let slice = map.get(p.categoryId) || [];
      slice.push(p);
      map.set(p.categoryId, slice);
    });
    return map;
  });

  canSubmit = computed(() => {
    const totalItems = this.totalItems();
    const submitting = this.submitting();
    const validState = this.delivery().state === DeliveryState.InProgress;

    return this.deliveryForm.valid
      && totalItems > 0
      && !submitting
      && validState;
  });

  delivery = signal<Delivery>({
    id: -1,
    state: DeliveryState.InProgress,
    fromId: -1,
    clientId: -1,
    message: '',
    systemMessages: [],
    lines: [],
  });
  categories = signal<ProductCategory[]>([]);
  products = signal<Product[]>([]);

  user = signal<User | undefined>(undefined);
  selectedClient = signal<Client | undefined>(undefined);
  selectedUser = signal<User | undefined>(undefined);

  clientTypeaheadSettings = signal<TypeaheadSettings<Client> | undefined>(undefined);
  fromTypeaheadSettings = signal<TypeaheadSettings<User> | undefined>(undefined);

  deliveryForm = new FormGroup({});

  constructor() {
    effect(() => {
      const delivery = this.delivery();
      const products = this.products();
      if (products.length > 0) {
        this.setupForm(delivery);
        this.setupClientTypeaheadSettings();
        this.setupFromTypeaheadSettings();
      }
    });

    effect(() => {
      const categories = this.categories();
      categories.forEach(category => {
        if (!category.autoCollapse) return;

        this.toggleCategory(category.id, true);
      })
    });

    effect(() => {
      const delivery = this.delivery();
      if (delivery && delivery.id !== -1) {
        this.router.navigateByUrl(`${this.router.url.split('?')[0]}?deliveryId=${delivery.id}`, { replaceUrl: true });
      }
    });
  }

  ngOnInit(): void {
    this.route.queryParams.pipe(
      take(1),
      switchMap(queryParams => {
        const deliveryId = parseInt(queryParams['deliveryId'], 10);
        const delivery$ = isNaN(deliveryId)
          ? of(null)
          : this.deliveryService.getDelivery(deliveryId).pipe(
            catchError(() => {
              this.router.navigate(['/dashboard']);
              return of(null);
            })
          );

        return forkJoin([
          this.userService.currentUser(),
          this.productService.getCategories(true),
          this.productService.allProducts(true),
          delivery$
        ]);
      })
    ).subscribe(([user, categories, products, delivery]) => {
      this.user.set(user);
      this.products.set(products);
      this.categories.set(categories);
      this.loading.set(false);

      if (delivery) {
        this.delivery.set(delivery);
      } else {
        this.setDefaultUser(user);
      }
    });
  }

  setDefaultUser(user: User) {
    if (this.delivery().id === -1) {
      this.delivery.update(d => {
        d.fromId = user.id;
        d.from = user;
        return d;
      });
    }
  }

  setupForm(delivery: Delivery) {
    Object.keys(this.deliveryForm.controls).forEach(key => {
      if (key.startsWith('product_')) {
        this.deliveryForm.removeControl(key);
      }
    });

    this.deliveryForm.setControl('fromId', new FormControl(delivery.fromId, [Validators.required]));
    this.deliveryForm.setControl('clientId', new FormControl(delivery.clientId, [Validators.required, Validators.min(0)]));
    this.deliveryForm.setControl('message', new FormControl(delivery.message));

    this.totalItems.set(0);
    this.products().forEach(product => {
      const existingLine = delivery.lines.find(l => l.productId === product.id);
      const quantity = existingLine ? existingLine.quantity : 0;

      this.totalItems.update(t => t + quantity);

      this.deliveryForm.addControl(
        `product_${product.id}`,
        new FormControl(quantity, [Validators.min(0)])
      );
    });
  }

  selectClient(client: Client | Client[]) {
    if (Array.isArray(client)) {
      client = client[0];
    }

    (this.deliveryForm.get('clientId')! as unknown as FormControl<number>).setValue(client.id);
    this.selectedClient.set(client);
  }

  selectFrom(from: User | User[]) {
    if (Array.isArray(from)) {
      from = from[0];
    }

    (this.deliveryForm.get('fromId')! as unknown as FormControl<number>).setValue(from.id);
    this.selectedUser.set(from);
  }

  getProductQuantity(productId: number): number {
    const control = this.deliveryForm.get(`product_${productId}`);
    return control ? control.value || 0 : 0;
  }

  updateProductQuantity(productId: number, quantity: number) {
    const control = this.deliveryForm.get(`product_${productId}`) as unknown as FormControl<number>;
    if (control) {
      control.setValue(Math.max(0, quantity));
      this.deliveryForm.markAsDirty();
    }
  }

  incrementProduct(productId: number) {
    const currentQuantity = this.getProductQuantity(productId);
    this.updateProductQuantity(productId, currentQuantity + 1);
    this.totalItems.update(t => t + 1);
  }

  decrementProduct(productId: number) {
    const currentQuantity = this.getProductQuantity(productId);
    this.updateProductQuantity(productId, Math.max(0, currentQuantity - 1));
    this.totalItems.update(t => t - 1);
  }

  toggleCategory(categoryId: number, forceHide: boolean = false) {
    this.collapsedCategories.update(collapsed => {
      const newSet = new Set(collapsed);

      if (forceHide) {
        newSet.add(categoryId);
      } else {
        if (newSet.has(categoryId)) {
          newSet.delete(categoryId);
        } else {
          newSet.add(categoryId);
        }
      }

      return newSet;
    });
  }

  isCategoryCollapsed(categoryId: number): boolean {
    return this.collapsedCategories().has(categoryId);
  }

  getCategoryTotal(categoryId: number): number {
    const products = this.groupedProducts().get(categoryId) || [];
    return products.reduce((total, product) => {
      return total + this.getProductQuantity(product.id);
    }, 0);
  }

  onSubmit() {
    if (!this.deliveryForm.valid || this.submitting()) {
      return;
    }

    if (!this.deliveryForm.dirty) {
      return;
    }

    const formValue: any = this.deliveryForm.value;

    const lines: DeliveryLine[] = [];
    this.products().forEach(product => {
      const quantity = formValue[`product_${product.id}`] || 0;
      if (quantity > 0) {
        lines.push({
          productId: product.id,
          quantity: quantity,
        });
      }
    });

    const deliveryData = {
      ...this.delivery(),
      fromId: formValue.fromId,
      from: this.selectedUser(),
      clientId: formValue.clientId,
      recipient: this.selectedClient(),
      message: formValue.message || '',
      lines: lines
    };

    const action$ = deliveryData.id === -1 ?
      this.deliveryService.createDelivery(deliveryData) :
      this.deliveryService.updateDelivery(deliveryData);

    this.submitting.set(true);
    action$.subscribe({
      next: (delivery) => {
        this.toastr.success(translate("manage-delivery.success"));
        this.submitting.set(false);
        this.delivery.set(delivery);

        this.promptTransition();
      },
      error: err => {
        console.error(err);
        this.toastr.error(translate("manage-delivery.failed"));
        this.submitting.set(false);
      }
    });
  }

  private promptTransition() {
    const [modal, component] = this.modalService.open(TransitionDeliveryModalComponent, DefaultModalOptions);
    component.delivery.set(this.delivery());

    modal.closed.pipe(
      tap((nextState: DeliveryState | undefined) => {
        if (nextState === undefined) return;

        this.delivery.update(d => {
          d.state = nextState;
          return d;
        });
      })
    ).subscribe();
  }

  private setupClientTypeaheadSettings(): void {
    const clientTypeaheadSettings = new TypeaheadSettings<Client>();
    clientTypeaheadSettings.id = 'client-typeahead';
    clientTypeaheadSettings.multiple = false;
    clientTypeaheadSettings.minCharacters = 1;

    const delivery = this.delivery();
    if (delivery.recipient) {
      clientTypeaheadSettings.savedData = delivery.recipient;
      this.selectClient(delivery.recipient);
    }

    clientTypeaheadSettings.selectionCompareFn = (c1, c2) => c1.id === c2.id;
    clientTypeaheadSettings.fetchFn = (f: string) => this.clientService.search(f);

    this.clientTypeaheadSettings.set(clientTypeaheadSettings);
  }

  private setupFromTypeaheadSettings(): void {
    const fromTypeaheadSettings = new TypeaheadSettings<User>();
    fromTypeaheadSettings.id = 'from-typeahead';
    fromTypeaheadSettings.multiple = false;
    fromTypeaheadSettings.minCharacters = 2;

    const delivery = this.delivery();
    if (delivery.from) {
      fromTypeaheadSettings.savedData = delivery.from;
      this.selectFrom(delivery.from);
    }

    fromTypeaheadSettings.selectionCompareFn = (u1, u2) => u1.id === u2.id;
    fromTypeaheadSettings.fetchFn = (f: string) => this.userService.search(f);

    this.fromTypeaheadSettings.set(fromTypeaheadSettings);
  }

  protected readonly DeliveryState = DeliveryState;
}
