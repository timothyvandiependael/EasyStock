import { Component, Input, Output, EventEmitter, SimpleChanges, ElementRef, ViewChild } from '@angular/core';
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
import { CheckboxData } from '../../checkbox';

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
  @Input() checkboxOptions: CheckboxData[] = [];

  @Output() buttonClicked = new EventEmitter<string>();
  @Output() sortChanged = new EventEmitter<Sort>();
  @Output() pageChanged = new EventEmitter<PageEvent>();
  @Output() filterChanged = new EventEmitter<FilterCondition[]>();
  @Output() checkboxChanged = new EventEmitter<any>();
  @Output() rowSelected = new EventEmitter<any>();
  @Output() rowDoubleClicked = new EventEmitter<any>();
  @Output() exportCsvClicked = new EventEmitter<any>();
  @Output() exportExcelClicked = new EventEmitter<any>();

  filterColumns: string[] = [];

  selectedRow: any;

  hoveredRowIndex: number = -1;
  selectedRowIndex: number = -1;

  startDate: string | null = null;
  endDate: string | null = null;

  @ViewChild('dtcontainer') container!: ElementRef<HTMLDivElement>;

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
          field: field.replace(/(>=|<=)$/, ''),
          operator: val.operator,
          value: val.value
        } as FilterCondition));

      this.filterChanged.emit(filterPayload);
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['data'] && this.data && this.data.length > 0) {
      this.onRowClick(this.data[0], 0);
    }

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

  ngAfterViewInit() {
    this.container.nativeElement.focus();
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

      if (btn.action == 'export') this.isExportMenuOpen = !this.isExportMenuOpen;
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

  onCheckboxChange(option: CheckboxData, event: Event) {
    var htmlInput = (event.target as HTMLInputElement)
    option.checked = htmlInput.checked;

    this.checkboxChanged.emit(option);
  }

  onDateRangeChange() {
    delete this.filters['CrDate>='];
    delete this.filters['CrDate<='];

    if (this.startDate) {
      this.filters['CrDate>='] = { operator: '>=', value: this.startDate };
    }

    if (this.endDate) {
      this.filters['CrDate<='] = { operator: '<=', value: this.endDate };
    }

    this.filterSubject.next();
  }

  onRowClick(row: any, index: number) {
    this.selectedRow = row;
    this.selectedRowIndex = index;
    this.rowSelected.emit(row);

  }

  onRowDoubleClick(row: any) {
    this.rowDoubleClicked.emit(row);
  }

  onKeydown(event: KeyboardEvent) {
    if (!this.data || this.data.length === 0) return;

    if (event.key === 'ArrowDown') {
      event.preventDefault();
      if (this.selectedRowIndex < this.data.length - 1) {
        this.selectedRowIndex++;
        this.selectedRow = this.data[this.selectedRowIndex];
        this.rowSelected.emit(this.selectedRow);
      }
    } else if (event.key === 'ArrowUp') {
      event.preventDefault();
      if (this.selectedRowIndex > 0) {
        this.selectedRowIndex--;
        this.selectedRow = this.data[this.selectedRowIndex];
        this.rowSelected.emit(this.selectedRow);
      }
    }
  }

  formatDateIfPossible(value: any): string {
    if (!value) return '';

    const date = new Date(value);
    if (!isNaN(date.getTime())) {
      return this.datePipe.transform(date, 'dd/MM/yyyy') || '';
    }

    return value;
  }

  isExportMenuOpen = false;

  exportCsv() {
    this.isExportMenuOpen = false;
    this.exportCsvClicked.emit();
  }

  exportExcel() {
    this.isExportMenuOpen = false;
    this.exportExcelClicked.emit();
  }

}
