import { Component } from '@angular/core';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import { ButtonConfig } from '../../../shared/button-config.model';
import { ProductService } from '../product-service';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { Subscription } from 'rxjs';
import { AdvancedQueryParametersDto, FilterCondition, SortOption } from '../../../shared/query';
import { DataTable } from '../../../shared/components/data-table/data-table';
import { ActivatedRoute, Router } from '@angular/router';
import { CheckboxData } from '../../../shared/checkbox';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';
import { AuthService } from '../../auth/auth-service';
import { PageTitleService } from '../../../shared/services/page-title-service';
import { SupplierService } from '../../supplier/supplier-service';
import { LookupDialog } from '../../../shared/components/lookup-dialog/lookup-dialog';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-product-overview',
  imports: [DataTable],
  templateUrl: './product-overview.html',
  styleUrl: './product-overview.css'
})
export class ProductOverview {
  private getColumnsSub?: Subscription;
  private getAdvancedSub?: Subscription;
  private blockSub?: Subscription;
  private unblockSub?: Subscription;
  private routeSub?: Subscription;
  private addToSupplierSub?: Subscription;
  private removeFromSupplierSub?: Subscription;

  data: any[] = [];
  columnsMeta: ColumnMetaData[] = [];
  displayedColumns: string[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  filters: FilterCondition[] = [];

  selectedRow: any;

  fromSupplierId?: number = undefined;

  buttons: ButtonConfig[] = [];

  normalButtons: ButtonConfig[] = [
    { label: 'Add', icon: 'add', action: 'add', color: 'primary', disabled: true },
    { label: 'Edit', icon: 'edit', action: 'edit', color: 'accent', disabled: true },
    { label: 'Block', icon: 'block', action: 'block', color: 'warn', disabled: true },
    { label: 'Export', icon: 'download', action: 'export', color: 'export', disabled: false },
    { label: 'Suppliers', icon: 'group', action: 'suppliers', color: 'detail', disabled: true }
  ]

  fromSupplierButtons: ButtonConfig[] = [
    { label: 'Add to Supplier', icon: 'add', action: 'addtosupplier', color: 'primary', disabled: true },
    { label: 'Remove from Supplier', icon: 'delete', action: 'removefromsupplier', color: 'warn', disabled: true },
    { label: 'Edit', icon: 'edit', action: 'edit', color: 'accent', disabled: true },
    { label: 'Block', icon: 'block', action: 'block', color: 'warn', disabled: true },
    { label: 'Export', icon: 'download', action: 'export', color: 'export', disabled: false },
    { label: 'Suppliers', icon: 'group', action: 'suppliers', color: 'detail', disabled: true }
  ]

  checkboxOptions: CheckboxData[] = [
    { id: 'showblocked', label: 'Show blocked', checked: false }
  ]

  currentSort: Sort = { active: '', direction: '' };

  constructor(
    private productService: ProductService,
    private supplierService: SupplierService,
    private router: Router,
    private route: ActivatedRoute,
    private snackbar: MatSnackBar,
    private pageTitleService: PageTitleService,
    private dialog: MatDialog,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService,
    private authService: AuthService) { }

  ngOnInit() {
    this.buttons = this.normalButtons;

    const addBtn = this.buttons.find(b => b.action === 'add');
    if (addBtn) addBtn.disabled = !this.authService.canAdd("Product");

    this.loadRouteParams();
  }

  loadRouteParams() {
    this.routeSub = this.route.queryParamMap.subscribe(params => {
      var id = params.get('fromSupplierId');
      var name = params.get('fromSupplierName');

      if (!id) {
        this.fromSupplierId = undefined;
      }
      else {
        this.fromSupplierId = parseInt(id);
        this.buttons = this.fromSupplierButtons;

        const addBtn = this.buttons.find(b => b.action == 'addtosupplier');
        if (addBtn) addBtn.disabled = !this.authService.canAdd("Product");
      }

      if (name) {
        this.pageTitleService.setTitle('Products for Supplier: ' + name);
      }
      else {
        this.pageTitleService.setTitle('Products');
      }

      this.loadColumns();
    })
  }

  ngOnDestroy() {
    this.getColumnsSub?.unsubscribe();
    this.getAdvancedSub?.unsubscribe();
    this.blockSub?.unsubscribe();
    this.unblockSub?.unsubscribe();
    this.routeSub?.unsubscribe();
    this.addToSupplierSub?.unsubscribe();
    this.removeFromSupplierSub?.unsubscribe();
  }

  loadColumns() {
    this.getColumnsSub = this.productService.getColumns().subscribe({
      next: (columns: ColumnMetaData[]) => {
        var overviewColumns = columns.filter(c => !c.isOnlyDetail);

        this.columnsMeta = overviewColumns;
        this.displayedColumns = overviewColumns.map(c => c.name);

        this.onShowBlockedClicked({ id: 'showblocked', label: 'Show blocked', checked: false });
      },
      error: (err) => {
        console.error('Error retrieving columns: ', err);
        this.persistentSnackbar.showError('Error retrieving columns. Please refresh the page or try again later.');
      }
    });
  }

  loadData() {
    const direction = this.currentSort.direction;
    const sortOptions = direction === 'asc' || direction === 'desc'
      ? [{ field: this.currentSort.active, direction: direction as 'asc' | 'desc' }]
      : [];

    if (this.fromSupplierId) {
      var fc: FilterCondition = {
        field: 'SupplierId',
        operator: 'equals',
        value: this.fromSupplierId
      }
      this.filters.push(fc);
    }

    const query: AdvancedQueryParametersDto = {
      filters: this.filters,
      sorting: sortOptions,
      pagination: {
        pageNumber: this.pageIndex,
        pageSize: this.pageSize
      }
    };

    this.getAdvancedSub = this.productService.getAdvanced(query).subscribe({
      next: (result) => {
        this.data = result.data;
        this.totalCount = result.totalCount;
      },
      error: (err) => {
        console.log('Error retrieving data: ', err);
        this.persistentSnackbar.showError('Error loading data. Please refresh the page or try again later.');
      }
    })
  }

  onRowSelected(row: any) {
    this.selectedRow = row;

    const suppliersBtn = this.buttons.find(b => b.action === 'suppliers');
    if (suppliersBtn) suppliersBtn.disabled = !this.authService.canView("Supplier");
    const removeBtn = this.buttons.find(b => b.action === "removefromsupplier");
    if (removeBtn) removeBtn.disabled = !this.authService.canDelete("Product")
    const editBtn = this.buttons.find(b => b.action === 'edit');
    if (editBtn) editBtn.disabled = !this.authService.canEdit("Product");
    const blockBtn = this.buttons.find(b => b.action === 'block' || b.action === 'unblock');
    if (blockBtn) {
      blockBtn.disabled = !this.authService.canDelete("Product");
      if (row.blUserId) {
        blockBtn.label = 'Unblock';
        blockBtn.icon = 'radio_button_unchecked';
        blockBtn.action = 'unblock';
      }
      else {
        blockBtn.label = 'Block';
        blockBtn.icon = 'block';
        blockBtn.action = 'block';
      }
    }
  }

  onButtonClicked(action: string) {
    switch (action) {
      case 'add': this.onAddClicked(); break;
      case 'edit': this.onEditClicked(); break;
      case 'block': this.onBlockClicked(); break;
      case 'unblock': this.onUnblockClicked(); break;
      case 'export': this.onExportClicked(); break;
      case 'suppliers': this.onSuppliersClicked(); break;
      case 'addtosupplier': this.onAddToSupplierClicked(); break;
      case 'removefromsupplier': this.onRemoveFromSupplierClicked(); break;
      default: break;
    }
  }

  onCheckboxChanged(box: CheckboxData) {
    switch (box.id) {
      case "showblocked": this.onShowBlockedClicked(box); break;
      default: break;
    }
  }

  onShowBlockedClicked(box: CheckboxData) {
    var filter = this.filters.find(f => f.field == 'BlUserId')

    if (!filter) {
      filter = {
        field: 'BlUserId',
        operator: '=',
        value: ''
      }
      this.filters.push(filter);
    }
    else {
      filter.operator = '=';
    }

    if (box.checked) {
      filter.operator = '<>';
    }

    this.loadData();
  }

  onAddClicked() {
    this.router.navigate(['app/product/edit', 'add']);
  }

  onRowDoubleClicked(row: any) {
    this.onEditClicked();
  }

  onEditClicked() {
    var id = this.selectedRow.id;

    this.router.navigate(['app/product/edit', 'edit', id])
  }

  onBlockClicked() {
    var id = this.selectedRow.id;

    this.confirmDialogService.open({
      title: 'Block record?',
      message: 'Are you sure you want to block this record?',
      confirmText: 'Yes, block',
      cancelText: 'No, cancel'
    }).subscribe(yes => {
      if (yes) this.executeBlock(id);
    });
  }

  executeBlock(id: number) {
    this.blockSub = this.productService.block(id).subscribe({
      next: () => {
        this.snackbar.open(`${this.selectedRow.name} blocked`, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.loadData();
      },
      error: (err) => {
        console.error(err);

        this.persistentSnackbar.showError(`Error blocking ${this.selectedRow.name}. If the problem persists, please contact support.`);
      }
    })
  }

  onUnblockClicked() {
    var id = this.selectedRow.id;

    this.confirmDialogService.open({
      title: 'Unblock record?',
      message: 'Are you sure you want to unblock this record?',
      confirmText: 'Yes, unblock',
      cancelText: 'No, cancel'
    }).subscribe(yes => {
      if (yes) this.executeUnblock(id);
    });
  }

  executeUnblock(id: number) {
    this.unblockSub = this.productService.unblock(id).subscribe({
      next: () => {
        this.snackbar.open(`${this.selectedRow.name} unblocked`, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.loadData();
      },
      error: (err) => {
        console.error(err);

        this.persistentSnackbar.showError(`Error unblocking ${this.selectedRow.name}. If the problem persists, please contact support.`);
      }
    })
  }

  onExportClicked() {

  }

  onExportCsv() {
    this.export('csv')
  }

  onExportExcel() {
    this.export('excel');
  }

  export(format: 'csv' | 'excel') {
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

    this.productService.export(query, format);
  }

  onSuppliersClicked() {
    const id = this.selectedRow.id;
    this.router.navigate(['app/supplier'], {
      queryParams: {
        fromProductId: id,
        fromProductName: this.selectedRow.name
      }
    });
  }

  onAddToSupplierClicked() {
    const lookupType = "Product";

    var filters: FilterCondition[] = [
      {
        field: 'SupplierId',
        operator: '<>',
        value: this.fromSupplierId
      }
    ];

    const dialogRef = this.dialog.open(LookupDialog, {
      width: '1000px',
      height: '800px',
      maxWidth: '1000px',
      data: { type: lookupType, filters: filters }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        var supplierId = this.fromSupplierId ? this.fromSupplierId : 0;
        this.addToSupplierSub = this.supplierService.addProduct(supplierId, result.id).subscribe({
          next: () => {
            this.snackbar.open(`Product added to supplier.`, 'Close', {
              duration: 3000,
              horizontalPosition: 'right',
              verticalPosition: 'top',
            });
            this.loadData();
          },
          error: (err) => {
            this.persistentSnackbar.showError('Error while adding product to supplier.');
          }
        });
      }
    });
  }

  onRemoveFromSupplierClicked() {
    var supplierId = this.fromSupplierId ? this.fromSupplierId : 0;
    this.removeFromSupplierSub = this.supplierService.removeProduct(supplierId, this.selectedRow.id).subscribe({
      next: () => {
        this.snackbar.open(`Product removed from supplier.`, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.loadData();
      },
      error: (err) => {
        this.persistentSnackbar.showError('Error while removing product from supplier.');
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

  onFilterChanged(filterPayload: FilterCondition[]) {
    this.filters = filterPayload;
    this.pageIndex = 0;
    this.loadData();
  }
}
