import {
  ChangeDetectionStrategy,
  Component,
  computed,
  ContentChild, effect,
  input,
  OnInit,
  signal,
  TemplateRef
} from '@angular/core';
import {NgTemplateOutlet, TitleCasePipe} from '@angular/common';


@Component({
  selector: 'app-table',
  imports: [
    NgTemplateOutlet
  ],
  templateUrl: './table.component.html',
  styleUrl: './table.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TableComponent<T> implements OnInit {

  @ContentChild('header') headerTemplate!: TemplateRef<any>;
  @ContentChild('cell') cellTemplate!: TemplateRef<any>;

  trackByIdFunc = input.required<(index: number, value: T) => string>();
  items = input.required<T[]>();
  pageSize = input(10);

  currentPage = signal(1);

  totalPages = computed(() => Math.ceil(this.items().length / this.pageSize()));

  paginatedItems = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize();
    const end = start + this.pageSize();
    return this.items().slice(start, end);
  });

  range = (n: number) => Array.from({ length: n}, (_, i) => i);

  ngOnInit(): void {
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages()) {
      this.currentPage.set(page);
    }
  }

  nextPage(): void {
    this.goToPage(this.currentPage() + 1);
  }

  prevPage(): void {
    this.goToPage(this.currentPage() - 1);
  }

  getObjectKeys(obj: any): string[] {
    return Object.keys(obj);
  }

}
