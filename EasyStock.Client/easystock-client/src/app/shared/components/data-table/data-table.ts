import { Component, Input, Output, EventEmitter } from '@angular/core';
import { PageEvent } from '@angular/material/paginator';
import { Sort } from '@angular/material/sort';
import { ButtonConfig } from '../../button-config.model';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { CommonModule } from '@angular/common';
import { ColumnMetaData } from '../../column-meta-data';
import { stringOperators, numberOperators, dateOperators, booleanOperators, FilterOperator } from '../../filter-operators';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { FilterCondition } from '../../query';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-data-table',
  templateUrl: './data-table.html',
  styleUrls: ['./data-table.css'],
  providers: [DatePipe],
  imports: [
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    CommonModule,
    FormsModule
  ]
})
export class DataTable {
  private filterSubjectSub?: Subscription;
  private filterSubject = new Subject<void>();
  filters: { [key: string]: { operator: string; value: any } } = {};

  @Input() data: any[] = [];
  @Input() columnsMeta: ColumnMetaData[] = [];
  @Input() displayedColumns: string[] = [];
  @Input() totalCount = 0;
  @Input() pageSize = 10;
  @Input() pageIndex = 0;
  @Input() buttons: ButtonConfig[] = [];

  @Output() buttonClicked = new EventEmitter<string>();
  @Output() sortChanged = new EventEmitter<Sort>();
  @Output() pageChanged = new EventEmitter<PageEvent>();
  @Output() filterChanged = new EventEmitter<FilterCondition[]>();

  filterColumns: string[] = [];

  constructor(private datePipe: DatePipe) { }

  ngOnInit() {
    this.filterColumns = this.columnsMeta.map(c => 'filter_' + c.name);
    this.filterSubject.pipe(debounceTime(300)).subscribe(() => {
      const filterPayload: FilterCondition[] = Object.entries(this.filters)
        .filter(([_, val]) => {
          // For booleans, skip 'All'
          if (val.operator === 'All') return false;
          return val.value !== null && val.value !== undefined && val.value !== '';
        })
        .map(([field, val]) => ({
          field: field,
          operator: val.operator,
          value: val.value
        } as FilterCondition));

      this.filterChanged.emit(filterPayload);
    });
  }

  ngOnChanges() {
    this.filterColumns = this.columnsMeta.map(c => 'filter_' + c.name);

    for (const col of this.columnsMeta) {
      if (col.isFilterable) {
        if (!this.filters[col.name]) {
          this.filters[col.name] = {
            operator: this.getDefaultOperator(col.type),
            value: col.type === 'boolean' ? 'All' : ''
          };
        }
      }
    }
  }

  ngOnDestroy() {
    this.filterSubjectSub?.unsubscribe();
  }

  getDefaultOperator(type: string): string {
    switch (type.toLowerCase()) {
      case 'string': return 'Contains';
      case 'number': return 'Equals';
      case 'date': return 'Equals';
      case 'boolean': return 'All';
      default: return 'Contains';
    }
  }

  getOperatorsForType(type: string): FilterOperator[] {
    switch (type.toLowerCase()) {
      case 'string': return stringOperators;
      case 'number': return numberOperators;
      case 'date': return dateOperators;
      case 'boolean': return booleanOperators;
      default: return stringOperators;
    }
  }

  getMeta(columnName: string) {
    return this.columnsMeta.find(c => c.name === columnName);
  }

  onButtonClick(btn: ButtonConfig) {
    if (!btn.disabled && !btn.hidden) {
      this.buttonClicked.emit(btn.action);
    }
  }

  onSortChange(sort: Sort) {
    this.sortChanged.emit(sort);
  }

  onPageChange(event: PageEvent) {
    this.pageChanged.emit(event);
  }

  onFilterChange(columnName: string) {
    this.filterSubject.next();
  }

  formatDateIfPossible(value: any): string {
    if (!value) return '';

    // Check if value is ISO date string
    // A quick and safe way: try parsing it to Date and check if valid
    const date = new Date(value);
    if (!isNaN(date.getTime())) {
      // Format the date as yyyy-MM-dd or whatever you want
      return this.datePipe.transform(date, 'dd/MM/yyyy') || '';
    }

    // Not a date, just return as is
    return value;
  }

}
