import { Component } from '@angular/core';
import { Sort } from '@angular/material/sort';
import { PageEvent } from '@angular/material/paginator';
import { ButtonConfig } from '../../../shared/button-config.model';
import { ReceptionService } from '../reception-service';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { Subscription } from 'rxjs';
import { AdvancedQueryParametersDto, FilterCondition, SortOption } from '../../../shared/query';
import { DataTable } from '../../../shared/components/data-table/data-table';
import { Router } from '@angular/router';
import { CheckboxData } from '../../../shared/checkbox';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';
import { AuthService } from '../../auth/auth-service';
import { PageTitleService } from '../../../shared/services/page-title-service';

@Component({
  selector: 'app-reception-overview',
  imports: [DataTable],
  templateUrl: './reception-overview.html',
  styleUrl: './reception-overview.css'
})
export class ReceptionOverview {
  private getColumnsSub?: Subscription;
  private getAdvancedSub?: Subscription;
  private blockSub?: Subscription;
  private unblockSub?: Subscription;

  data: any[] = [];
  columnsMeta: ColumnMetaData[] = [];
  displayedColumns: string[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;
  filters: FilterCondition[] = [];

  selectedRow: any;

  buttons: ButtonConfig[] = [
    { label: 'Add', icon: 'add', action: 'add', color: 'primary', disabled: true },
    { label: 'Edit', icon: 'edit', action: 'edit', color: 'accent', disabled: true },
    { label: 'Block', icon: 'block', action: 'block', color: 'warn', disabled: true },
    { label: 'Export', icon: 'download', action: 'export', color: 'export', disabled: false },
    { label: 'Lines', icon: 'receipt_long', action: 'suppliers', color: 'detail', disabled: false }
  ]

  checkboxOptions: CheckboxData[] = [
    { id: 'showblocked', label: 'Show blocked', checked: false }
  ]

  currentSort: Sort = { active: '', direction: '' };

  constructor(
    private receptionService: ReceptionService,
    private router: Router,
    private snackbar: MatSnackBar,
    private pageTitleService: PageTitleService,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService,
    private authService: AuthService) { }

  ngOnInit() {
    this.pageTitleService.setTitle('Receptions');
    const addBtn = this.buttons.find(b => b.action === 'add');
    if (addBtn) addBtn.disabled = !this.authService.canAdd("Reception");

    this.loadColumns();
  }

  ngOnDestroy() {
    this.getColumnsSub?.unsubscribe();
    this.getAdvancedSub?.unsubscribe();
    this.blockSub?.unsubscribe();
    this.unblockSub?.unsubscribe();
  }

  loadColumns() {
    this.getColumnsSub = this.receptionService.getColumns().subscribe({
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

    const query: AdvancedQueryParametersDto = {
      filters: this.filters,
      sorting: sortOptions,
      pagination: {
        pageNumber: this.pageIndex,
        pageSize: this.pageSize
      }
    };

    this.getAdvancedSub = this.receptionService.getAdvanced(query).subscribe({
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

    const linesBtn = this.buttons.find(b => b.action === 'lines');
    if (linesBtn) linesBtn.disabled = false;
    const editBtn = this.buttons.find(b => b.action === 'edit');
    if (editBtn) editBtn.disabled = !this.authService.canEdit("Reception");
    const blockBtn = this.buttons.find(b => b.action === 'block' || b.action === 'unblock');
    if (blockBtn) {
      blockBtn.disabled = !this.authService.canDelete("Reception");
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
      case 'lines': this.onLinesClicked(); break;
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
    this.router.navigate(['app/reception/edit', 'add']);
  }

  onRowDoubleClicked(row: any) {
    this.onEditClicked();
  }

  onEditClicked() {
    var id = this.selectedRow.id;

    this.router.navigate(['app/reception/edit', 'edit', id])
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
    this.blockSub = this.receptionService.block(id).subscribe({
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

        this.persistentSnackbar.showError(`Error blocking ${this.selectedRow.receptionNumber}. If the problem persists, please contact support.`);
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
    this.unblockSub = this.receptionService.unblock(id).subscribe({
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

        this.persistentSnackbar.showError(`Error unblocking ${this.selectedRow.receptionNumber}. If the problem persists, please contact support.`);
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

    this.receptionService.export(query, format);
  }

  onLinesClicked() {
    const id = this.selectedRow.id;
    this.router.navigate(['app/receptionline'], {
      queryParams: {
        parentId: id,
        parentReceptionNumber: this.selectedRow.receptionNumber
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

