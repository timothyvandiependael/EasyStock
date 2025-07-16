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

@Component({
  selector: 'app-data-table',
  templateUrl: './data-table.html',
  styleUrls: ['./data-table.css'],
  imports: [
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    CommonModule
  ]
})
export class DataTable {
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

}
