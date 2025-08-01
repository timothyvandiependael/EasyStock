import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ColumnMetaData } from '../../column-meta-data';
import { ButtonConfig } from '../../button-config.model';
import { DataTable } from '../data-table/data-table';
import { AdvancedQueryParametersDto, FilterCondition } from '../../query';
import { Sort } from '@angular/material/sort';
import { Subscription } from 'rxjs';

import { ClientService } from '../../../features/client/client-service';
import { ProductService } from '../../../features/product/product-service';
import { SupplierService } from '../../../features/supplier/supplier-service';
import { CategoryService } from '../../../features/category/category-service';
import { PurchaseOrderLineService } from '../../../features/purchase-order-line/purchase-order-line-service';
import { SalesOrderLineService } from '../../../features/sales-order-line/sales-order-line-service';

import { PersistentSnackbarService } from '../../services/persistent-snackbar.service';
import { PageEvent } from '@angular/material/paginator';
import { PurchaseOrderService } from '../../../features/purchase-order/purchase-order-service';
import { SalesOrderService } from '../../../features/sales-order/sales-order-service';
import { ReceptionService } from '../../../features/reception/reception-service';
import { DispatchService } from '../../../features/dispatch/dispatch-service';


@Component({
  selector: 'app-lookup-dialog',
  imports: [DataTable],
  templateUrl: './lookup-dialog.html',
  styleUrl: './lookup-dialog.css'
})
export class LookupDialog {
  private getAdvancedSub?: Subscription;

  data: any[] = [];

  columnsMeta: ColumnMetaData[] = [];

  displayedColumns: string[] = [];

  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  filters: FilterCondition[] = [];

  buttons: ButtonConfig[] = [
    { label: 'Select', action: 'select', color: 'primary', icon: 'check' },
    { label: 'Cancel', action: 'cancel', color: 'warn', icon: 'close' }
  ];

  currentFilters: FilterCondition[] = [];
  currentSort: Sort = { active: 'id', direction: 'asc' };
  selectedRow: any = null;

  private activeService: any;

  constructor(
    private categoryService: CategoryService,
    private productService: ProductService,
    private clientService: ClientService,
    private supplierService: SupplierService,
    private purchaseOrderLineService: PurchaseOrderLineService,
    private salesOrderLineService: SalesOrderLineService,
    private purchaseOrderService: PurchaseOrderService,
    private salesOrderService: SalesOrderService,
    private receptionService: ReceptionService,
    private dispatchService: DispatchService,
    private persistentSnackbar: PersistentSnackbarService,
    private dialogRef: MatDialogRef<LookupDialog>,
    @Inject(MAT_DIALOG_DATA) public dataInput: { type: string, filters: FilterCondition[] }
  ) { }

  ngOnInit(): void {
    if (this.dataInput.type.toLowerCase() == 'salesorderline' || this.dataInput.type.toLowerCase() == 'purchaseorderline') {
      this.columnsMeta = [
        {
          name: 'orderNumber',
          displayName: 'Order',
          type: 'string',
          isSortable: true,
          isFilterable: true,
          isEditable: false,
          isLookup: false,
          isOnlyDetail: false
        },
        {
          name: 'lineNumber',
          displayName: 'Line',
          type: 'number',
          isSortable: true,
          isFilterable: true,
          isEditable: false,
          isLookup: false,
          isOnlyDetail: false
        },
        {
          name: 'productName',
          displayName: 'Product',
          type: 'string',
          isSortable: true,
          isFilterable: true,
          isEditable: false,
          isLookup: false,
          isOnlyDetail: false
        }
      ];

      this.displayedColumns = ['orderNumber', 'lineNumber', 'productName'];
    }
    else if (this.dataInput.type.toLowerCase() == 'salesorder' || this.dataInput.type.toLowerCase() == 'purchaseorder') {
      this.columnsMeta = [
        {
          name: 'orderNumber',
          displayName: 'Order',
          type: 'string',
          isSortable: true,
          isFilterable: true,
          isEditable: false,
          isLookup: false,
          isOnlyDetail: false
        }
      ];

      this.displayedColumns = ['orderNumber'];
    }
    else if (this.dataInput.type.toLowerCase() == 'reception') {
      this.columnsMeta = [
        {
          name: 'receptionNumber',
          displayName: 'Order',
          type: 'string',
          isSortable: true,
          isFilterable: true,
          isEditable: false,
          isLookup: false,
          isOnlyDetail: false
        }
      ];

      this.displayedColumns = ['receptionNumber'];
    }
    else if (this.dataInput.type.toLowerCase() == 'dispatch') {
      this.columnsMeta = [
        {
          name: 'dispatchNumber',
          displayName: 'Order',
          type: 'string',
          isSortable: true,
          isFilterable: true,
          isEditable: false,
          isLookup: false,
          isOnlyDetail: false
        }
      ];

      this.displayedColumns = ['dispatchNumber'];
    }
    else {
      this.columnsMeta = [
        {
          name: 'name',
          displayName: 'Name',
          type: 'string',
          isSortable: true,
          isFilterable: true,
          isEditable: false,
          isLookup: false,
          isOnlyDetail: false
        }
      ]

      this.displayedColumns = ['name'];
    }

    switch (this.dataInput.type.toLowerCase()) {
      case 'client': this.activeService = this.clientService; break;
      case 'product': this.activeService = this.productService; break;
      case 'supplier': this.activeService = this.supplierService; break;
      case 'category': this.activeService = this.categoryService; break;
      case 'purchaseorderline': this.activeService = this.purchaseOrderLineService; break;
      case 'salesorderline': this.activeService = this.salesOrderLineService; break;
      case 'purchaseorder': this.activeService = this.purchaseOrderService; break;
      case 'salesorder': this.activeService = this.salesOrderService; break;
      case 'reception': this.activeService = this.receptionService; break;
      case 'dispatch': this.activeService = this.dispatchService; break;
      default:
        console.error('Unknown lookup type', this.dataInput.type);
        return;
    }

    if (this.dataInput.filters && this.dataInput.filters.length > 0) {
      this.filters = this.dataInput.filters;
    }
    else this.filters = [];

    this.loadData();
  }

  ngOnDestroy(): void {
    this.getAdvancedSub?.unsubscribe();
  }

  loadData() {
    const direction = this.currentSort.direction;
    const sortOptions = direction === 'asc' || direction === 'desc'
      ? [{ field: this.currentSort.active, direction: direction as 'asc' | 'desc' }]
      : [];

    var fc: FilterCondition = {
      field: 'BlUserId',
      operator: '=',
      value: ''
    }
    this.filters.push(fc);

    const query: AdvancedQueryParametersDto = {
      filters: this.filters,
      sorting: sortOptions,
      pagination: {
        pageNumber: this.pageIndex,
        pageSize: this.pageSize
      }
    };

    this.getAdvancedSub = this.activeService.getAdvanced(query).subscribe({
      next: (result: any) => {
        this.data = result.data;
        this.totalCount = result.totalCount;
      },
      error: (err: any) => {
        console.error('Error retrieving data: ', err);
        this.persistentSnackbar.showError('Error loading data.');
      }
    });
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

  onFilterChanged(filters: FilterCondition[]) {
    this.filters = filters;
    this.pageIndex = 0;
    this.loadData();
  }

  onRowSelected(row: any) {
    this.selectedRow = row;
  }

  onButtonClicked(action: string) {
    switch (action) {
      case 'select': this.onSelectClicked(); break;
      case 'cancel': this.onCloseClicked(); break;
      default: break;
    }
  }

  onSelectClicked() {
    if (!this.selectedRow) return;
    this.dialogRef.close(this.selectedRow);
  }

  onCloseClicked() {
    this.dialogRef.close(null);
  }

  onRowDoubleClicked() {
    this.onSelectClicked();
  }
}
