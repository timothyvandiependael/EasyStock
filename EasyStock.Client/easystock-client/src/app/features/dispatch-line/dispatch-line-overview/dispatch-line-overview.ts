import { Component } from '@angular/core';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import { ButtonConfig } from '../../../shared/button-config.model';
import { DispatchLineService } from '../dispatch-line-service';
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

@Component({
  selector: 'app-dispatch-line-overview',
  imports: [DataTable],
  templateUrl: './dispatch-line-overview.html',
  styleUrl: './dispatch-line-overview.css'
})
export class DispatchLineOverview {
  private getColumnsSub?: Subscription;
  private getAdvancedSub?: Subscription;
  private blockSub?: Subscription;
  private unblockSub?: Subscription;
  private routeSub?: Subscription;

  data: any[] = [];
  columnsMeta: ColumnMetaData[] = [];
  displayedColumns: string[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  filters: FilterCondition[] = [];

  selectedRow: any;

  dispatchId?: number = undefined;

  buttons: ButtonConfig[] = [
    { label: 'Add', icon: 'add', action: 'add', color: 'primary', disabled: true },
    { label: 'Edit', icon: 'edit', action: 'edit', color: 'accent', disabled: true },
    { label: 'Block', icon: 'block', action: 'block', color: 'warn', disabled: true },
    { label: 'Export', icon: 'download', action: 'export', color: 'export', disabled: false }
  ]

  checkboxOptions: CheckboxData[] = [
    { id: 'showblocked', label: 'Show blocked', checked: false }
  ]

  currentSort: Sort = { active: '', direction: '' };

  constructor(
    private dispatchLineService: DispatchLineService,
    private router: Router,
    private route: ActivatedRoute,
    private snackbar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService,
    private authService: AuthService) { }

  ngOnInit() {
    const addBtn = this.buttons.find(b => b.action === 'add');
    if (addBtn) addBtn.disabled = !this.authService.canAdd("DispatchLine");

    this.loadRouteParams();
  }

  ngOnDestroy() {
    this.getColumnsSub?.unsubscribe();
    this.getAdvancedSub?.unsubscribe();
    this.blockSub?.unsubscribe();
    this.unblockSub?.unsubscribe();
    this.routeSub?.unsubscribe();
  }

  loadRouteParams() {
    this.routeSub = this.route.queryParamMap.subscribe(params => {
      var id = params.get('parentId');

      if (!id) {
        this.dispatchId = undefined;
      }
      else {
        this.dispatchId = parseInt(id);
      }

      this.loadColumns();
    })

  }

  loadColumns() {
    this.getColumnsSub = this.dispatchLineService.getColumns().subscribe({
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

    if (this.dispatchId) {
      var fc: FilterCondition = {
        field: 'DispatchId',
        operator: 'equals',
        value: this.dispatchId
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

    this.getAdvancedSub = this.dispatchLineService.getAdvanced(query).subscribe({
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

    const editBtn = this.buttons.find(b => b.action === 'edit');
    if (editBtn) editBtn.disabled = !this.authService.canEdit("DispatchLine");
    const blockBtn = this.buttons.find(b => b.action === 'block' || b.action === 'unblock');
    if (blockBtn) {
      blockBtn.disabled = !this.authService.canDelete("DispatchLine");
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
    if (this.dispatchId) {
      this.router.navigate(['app/dispatchline/edit', 'add'], {
        queryParams: {
          parentId: this.dispatchId
        }
      });
    }
    else {
      this.router.navigate(['app/dispatchline/edit', 'add']);
    }
  }

  onRowDoubleClicked(row: any) {
    this.onEditClicked();
  }

  onEditClicked() {
    var id = this.selectedRow.id;

    this.router.navigate(['app/dispatchline/edit', 'edit', id])
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
    this.blockSub = this.dispatchLineService.block(id).subscribe({
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

        this.persistentSnackbar.showError(`Error blocking line ${this.selectedRow.lineNumber}. If the problem persists, please contact support.`);
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
    this.unblockSub = this.dispatchLineService.unblock(id).subscribe({
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

        this.persistentSnackbar.showError(`Error unblocking line ${this.selectedRow.lineNumber}. If the problem persists, please contact support.`);
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

    this.dispatchLineService.export(query, format);
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

