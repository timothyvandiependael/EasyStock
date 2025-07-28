import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { ColumnMetaData } from '../../column-meta-data';
import { ButtonConfig } from '../../button-config.model';
import { DataTable } from '../data-table/data-table';
import { AdvancedQueryParametersDto, FilterCondition } from '../../query';
import { Sort } from '@angular/material/sort';
import { Subscription } from 'rxjs';

//import { ClientService } from '../../../features/client/client-service';
import { ProductService } from '../../../features/product/product-service';
//import { SupplierService } from '../../../features/supplier/supplier-service';
import { CategoryService } from '../../../features/category/category-service';

import { PersistentSnackbarService } from '../../services/persistent-snackbar.service';
import { PageEvent } from '@angular/material/paginator';


@Component({
  selector: 'app-lookup-dialog',
  imports: [DataTable],
  templateUrl: './lookup-dialog.html',
  styleUrl: './lookup-dialog.css'
})
export class LookupDialog {
  private getAdvancedSub?: Subscription; 

  data: any[] = [];
  columnsMeta: ColumnMetaData[] = [
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
  ];

  displayedColumns: string[] = ['name'];

  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  filters: FilterCondition[] = [];

  buttons: ButtonConfig[] = [
    { label: 'Select', action: 'select', color: 'primary', icon: 'check' },
    { label: 'Cancel', action: 'cancel', color: 'warn', icon: 'close' }
  ];

  currentFilters: FilterCondition[] = [];
  currentSort: Sort = { active: 'Name', direction: 'asc' };
  selectedRow: any = null;

  private activeService: any;

  constructor(
    private categoryService: CategoryService,
    private productService: ProductService,
    // private clientService: ClientService,
    //private supplierService: SupplierService,
    private persistentSnackbar: PersistentSnackbarService,
    private dialogRef: MatDialogRef<LookupDialog>,
    @Inject(MAT_DIALOG_DATA) public dataInput: { type: string }
  ) { }

  ngOnInit(): void {
    // pick which service to use
    switch (this.dataInput.type.toLowerCase()) {
      //case 'client': this.activeService = this.clientService; break;
      case 'product': this.activeService = this.productService; break;
      //case 'supplier': this.activeService = this.supplierService; break;
      case 'category': this.activeService = this.categoryService; break;
      default:
        console.error('Unknown lookup type', this.dataInput.type);
        return;
    }

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
