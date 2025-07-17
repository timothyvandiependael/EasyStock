import { Component, OnInit } from '@angular/core';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import { ButtonConfig } from '../../../shared/button-config.model';
import { PurchaseOrderService } from './purchase-order-service';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { Subscription } from 'rxjs';
import { AdvancedQueryParametersDto, FilterCondition, SortOption } from '../../../shared/query';
import { DataTable } from '../../../shared/components/data-table/data-table';

@Component({
  selector: 'app-purchase-order-overview',
  imports: [ DataTable ],
  templateUrl: './purchase-order-overview.html',
  styleUrl: './purchase-order-overview.css'
})
export class PurchaseOrderOverview {
  private getColumnsSub?: Subscription;
  private getAdvancedSub?: Subscription;

  data: any[] = [];
  columnsMeta: ColumnMetaData[] = [];
  displayedColumns: string[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  filters: FilterCondition[] = [];

  buttons: ButtonConfig[] = [
    { label: 'Add', icon: 'add', action: 'add', color: 'primary' },
    { label: 'Edit', icon: 'edit', action: 'edit', color: 'accent', disabled: true },
    { label: 'Block', icon: 'block', action: 'block', color: 'warn', disabled: true }
  ]

  currentSort: Sort = { active: '', direction: '' };

  constructor(private purchaseOrderService: PurchaseOrderService) {}

  ngOnInit() {
    this.loadColumns();
  }

  ngOnDestroy() {
    this.getColumnsSub?.unsubscribe();
    this.getAdvancedSub?.unsubscribe();
  }

  loadColumns() {
    this.getColumnsSub = this.purchaseOrderService.getColumns().subscribe({
      next: (columns: ColumnMetaData[]) => {
        this.columnsMeta = columns;
        this.displayedColumns = columns.map(c => c.name);

        this.loadData();
      },
      error: (err) => {
        console.error('Error retrieving columns: ', err);
        // TODO visible error
      }
    });
  }

  loadData() {
    const direction = this.currentSort.direction;
    const sortOptions = direction === 'asc' || direction === 'desc'
      ? [{ field: this.currentSort.active, direction: direction as 'asc' | 'desc' }]
      : [];

    const query: AdvancedQueryParametersDto = {
      filters: this.filters,
      sorting: sortOptions,
      pagination: {
        pageIndex: this.pageIndex,
        pageSize: this.pageSize
      }
    };

    this.getAdvancedSub = this.purchaseOrderService.getAdvanced(query).subscribe({
      next: (result) => {
        this.data = result.data;
        this.totalCount = result.totalCount;
      },
      error: (err) => {
        console.log('Error retrieving data: ', err);
        // TODO visible error
      }
    })
  }

  onButtonClicked(action: string) {
    // TODO button handling
  }

  onSortChanged(sort: Sort) {
    this.currentSort = sort;
    this.loadData();
  }

  onPageChanged(event: PageEvent) {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadData();
  }

  onFilterChanged(filterPayload: FilterCondition[]) {
    this.filters = filterPayload;
    this.pageIndex = 0;
    this.loadData();
  }
}
