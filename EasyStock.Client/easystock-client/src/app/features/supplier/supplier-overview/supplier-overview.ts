import { Component } from '@angular/core';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import { ButtonConfig } from '../../../shared/button-config.model';
import { SupplierService } from '../supplier-service';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { Subscription } from 'rxjs';
import { AdvancedQueryParametersDto, FilterCondition, SortOption } from '../../../shared/query';
import { DataTable } from '../../../shared/components/data-table/data-table';
import { Router, ActivatedRoute } from '@angular/router';
import { CheckboxData } from '../../../shared/checkbox';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';
import { AuthService } from '../../auth/auth-service';
import { ProductService } from '../../product/product-service';
import { LookupDialog } from '../../../shared/components/lookup-dialog/lookup-dialog';
import { MatDialog } from '@angular/material/dialog';
import { PageTitleService } from '../../../shared/services/page-title-service';

@Component({
  selector: 'app-supplier-overview',
  imports: [DataTable],
  templateUrl: './supplier-overview.html',
  styleUrl: './supplier-overview.css'
})
export class SupplierOverview {
  private getColumnsSub?: Subscription;
  private getAdvancedSub?: Subscription;
  private blockSub?: Subscription;
  private unblockSub?: Subscription;
  private routeSub?: Subscription;
  private addToProductSub?: Subscription;
  private removeFromProductSub?: Subscription;

  data: any[] = [];
  columnsMeta: ColumnMetaData[] = [];
  displayedColumns: string[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  filters: FilterCondition[] = [];

  selectedRow: any;

  fromProductId?: number = undefined;

  buttons: ButtonConfig[] = [];

  normalButtons: ButtonConfig[] = [
    { label: 'Add', icon: 'add', action: 'add', color: 'primary', disabled: true },
    { label: 'Edit', icon: 'edit', action: 'edit', color: 'accent', disabled: true },
    { label: 'Block', icon: 'block', action: 'block', color: 'warn', disabled: true },
    { label: 'Export', icon: 'download', action: 'export', color: 'export', disabled: false },
    { label: 'Purchase Orders', icon: 'group', action: 'purchaseorders', color: 'detail', disabled: true },
    { label: 'Products', icon: 'inventory_2', action: 'products', color: 'detail', disabled: true }
  ]

  fromProductButtons: ButtonConfig[] = [
    { label: 'Add to Product', icon: 'add', action: 'addtoproduct', color: 'primary', disabled: true },
    { label: 'Remove from Product', icon: 'delete', action: 'removefromproduct', color: 'warn', disabled: true },
    { label: 'Edit', icon: 'edit', action: 'edit', color: 'accent', disabled: true },
    { label: 'Block', icon: 'block', action: 'block', color: 'warn', disabled: true },
    { label: 'Export', icon: 'download', action: 'export', color: 'export', disabled: false },
    { label: 'Purchase Orders', icon: 'group', action: 'purchaseorders', color: 'detail', disabled: true },
    { label: 'Products', icon: 'inventory_2', action: 'products', color: 'detail', disabled: true }
  ]

  checkboxOptions: CheckboxData[] = [
    { id: 'showblocked', label: 'Show blocked', checked: false }
  ]

  currentSort: Sort = { active: '', direction: '' };

  constructor(
    private supplierService: SupplierService,
    private productService: ProductService,
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
    if (addBtn) addBtn.disabled = !this.authService.canAdd("Supplier");

    this.loadRouteParams();
  }

  loadRouteParams() {
    this.routeSub = this.route.queryParamMap.subscribe(params => {
      var id = params.get('fromProductId');
      var name = params.get('fromProductName');

      if (!id) {
        this.fromProductId = undefined;
      }
      else {
        this.fromProductId = parseInt(id);
        this.buttons = this.fromProductButtons;

        const addBtn = this.buttons.find(b => b.action == 'addtoproduct');
        if (addBtn) addBtn.disabled = !this.authService.canAdd("Supplier");
      }

      if (name) {
        this.pageTitleService.setTitle('Suppliers for Product: ' + name);
      }
      else {
        this.pageTitleService.setTitle('Suppliers');
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
    this.addToProductSub?.unsubscribe();
    this.removeFromProductSub?.unsubscribe();
  }

  loadColumns() {
    this.getColumnsSub = this.supplierService.getColumns().subscribe({
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

    var blFilter = this.filters.find(f => f.field == 'BlUserId');
    if (!blFilter) {
      var chk = this.checkboxOptions.find(o => o.id == 'showblocked');
      if (chk) {
        this.onShowBlockedClicked({ id: 'showblocked', label: 'Show blocked', checked: chk.checked });
      }

    }

    if (this.fromProductId) {
      var fc: FilterCondition = {
        field: 'ProductId',
        operator: 'equals',
        value: this.fromProductId
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

    this.getAdvancedSub = this.supplierService.getAdvanced(query).subscribe({
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

    const posBtn = this.buttons.find(b => b.action == 'purchaseorders');
    if (posBtn) posBtn.disabled = !this.authService.canView('PurchaseOrder');
    const productsBtn = this.buttons.find(b => b.action == 'products');
    if (productsBtn) productsBtn.disabled = !this.authService.canView('Product');
    const removeBtn = this.buttons.find(b => b.action === "removefromproduct");
    if (removeBtn) removeBtn.disabled = !this.authService.canDelete("Supplier");
    const editBtn = this.buttons.find(b => b.action === 'edit');
    if (editBtn) editBtn.disabled = !this.authService.canEdit("Supplier");
    const blockBtn = this.buttons.find(b => b.action === 'block' || b.action === 'unblock');
    if (blockBtn) {
      blockBtn.disabled = !this.authService.canDelete("Supplier");
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
      case 'addtoproduct': this.onAddToProductClicked(); break;
      case 'removefromproduct': this.onRemoveFromProductClicked(); break;
      case 'products': this.onProductsClicked(); break;
      case 'purchaseorders': this.onPurchaseOrdersClicked(); break;
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
    this.router.navigate(['app/supplier/edit', 'add']);
  }

  onRowDoubleClicked(row: any) {
    this.onEditClicked();
  }

  onEditClicked() {
    var id = this.selectedRow.id;

    this.router.navigate(['app/supplier/edit', 'edit', id])
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
    this.blockSub = this.supplierService.block(id).subscribe({
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
    this.unblockSub = this.supplierService.unblock(id).subscribe({
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

    this.supplierService.export(query, format);
  }

  onAddToProductClicked() {
    const lookupType = "Supplier";

    var filters: FilterCondition[] = [
      {
        field: 'ProductId',
        operator: '<>',
        value: this.fromProductId
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
        var productId = this.fromProductId ? this.fromProductId : 0;
        this.addToProductSub = this.productService.addSupplier(productId, result.id).subscribe({
          next: () => {
            this.snackbar.open(`Supplier added to product.`, 'Close', {
              duration: 3000,
              horizontalPosition: 'right',
              verticalPosition: 'top',
            });
            this.loadData();
          },
          error: (err) => {
            this.persistentSnackbar.showError('Error while adding supplier to product.');
          }
        });
      }
    });
  }

  onRemoveFromProductClicked() {
    var productId = this.fromProductId ? this.fromProductId : 0;
    this.removeFromProductSub = this.productService.removeSupplier(productId, this.selectedRow.id).subscribe({
      next: () => {
        this.snackbar.open(`Supplier removed from product.`, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.loadData();
      },
      error: (err) => {
        this.persistentSnackbar.showError('Error while removing supplier from product.');
      }
    });
  }

  onProductsClicked() {
    this.router.navigate(['app/product'], {
      queryParams: {
        fromSupplierId: this.selectedRow.id,
        fromSupplierName: this.selectedRow.name
      }
    })
  }

  onPurchaseOrdersClicked() {
    this.router.navigate(['app/purchaseorder'], {
      queryParams: {
        fromSupplierId: this.selectedRow.id,
        fromSupplierName: this.selectedRow.name
      }
    })
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

