import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { SalesOrderService } from '../sales-order-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { SalesOrderDetailDto } from '../dtos/sales-order-detail.dto';
import { CreateSalesOrderDto } from '../dtos/create-sales-order.dto';
import { UpdateSalesOrderDto } from '../dtos/update-sales-order.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';
import { StorageService } from '../../../shared/storage/storage-service';

@Component({
  selector: 'app-sales-order-edit',
  imports: [EditView],
  templateUrl: './sales-order-edit.html',
  styleUrl: './sales-order-edit.css'
})
export class SalesOrderEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedSalesOrder?: SalesOrderDetailDto;

  procedureStep1 = true;
  procedureStep2 = false;

  addModeHideFields = [
    'orderNumber', 'status'
  ]

  @ViewChild(EditView) detailView!: EditView<SalesOrderDetailDto>;

  constructor(
    private salesOrderService: SalesOrderService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService,
    private storage: StorageService) { }

  ngOnInit() {
    this.loadColumnMeta();
  }

  loadColumnMeta() {
    this.getColumnsSub = this.salesOrderService.getColumns().subscribe({
      next: (columns: ColumnMetaData[]) => {
        this.columnMetaData = columns;

        this.loadRouteParams();
      },
      error: (err) => {
        console.error('Error retrieving columns: ', err);
        this.persistentSnackbar.showError(`Error retrieving fields. Please refresh the page or try again later.`);
      }
    });
  }

  loadRouteParams() {
    this.routeSub = this.route.paramMap.subscribe(params => {
      const mode = params.get('mode') as 'add' | 'edit';
      this.detailMode = mode;

      if (mode === 'edit') {
        const id = Number(params.get('id'));
        this.getByIdSub = this.salesOrderService.getById(id).subscribe({
          next: (dto: SalesOrderDetailDto) => {
            this.selectedSalesOrder = dto;
          },
          error: (err) => {
            console.error('Error retrieving salesOrder with id ' + id + ': ', err);
            this.persistentSnackbar.showError(`Error retrieving record with id ${id}. If the problem persists, please contact support.`);
          }
        })

      }
    });
  }

  ngOnDestroy() {
    this.routeSub?.unsubscribe();
    this.getColumnsSub?.unsubscribe();
    this.saveAndAddSub?.unsubscribe();
    this.saveNewExitSub?.unsubscribe();
    this.saveExitSub?.unsubscribe();
  }

  handleSaveAndAddAnother(salesOrder: CreateSalesOrderDto) {
    this.saveAndAddSub = this.salesOrderService.add(salesOrder).subscribe({
      next: (saved: SalesOrderDetailDto) => {
        this.selectedSalesOrder = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`Sales order saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving salesOrder ', err);
        this.persistentSnackbar.showError(`Error saving sales order. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(salesOrder: CreateSalesOrderDto) {
    this.saveNewExitSub = this.salesOrderService.add(salesOrder).subscribe({
      next: (saved: SalesOrderDetailDto) => {
        this.selectedSalesOrder = undefined;
        this.snackBar.open(`Sales order saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/salesorder']);
      },
      error: (err) => {
        console.error('Error saving salesOrder ', err);
        this.persistentSnackbar.showError(`Error saving sales order. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(salesOrder: UpdateSalesOrderDto) {
    this.saveExitSub = this.salesOrderService.edit(salesOrder.id, salesOrder).subscribe({
      next: () => {
        this.snackBar.open(`Sales order updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/salesorder']);
      },
      error: (err) => {
        console.error('Error updating salesOrder: ', err);
        this.persistentSnackbar.showError(`Error saving sales order. If the problem persists, please contact support.`);
      }
    })
  }

  handleCancel() {
    if (this.detailView.form.dirty) {
      this.confirmDialogService.open({
        title: this.detailMode === 'add' ? 'Discard new entry?' : 'Discard changes?',
        message: 'You have unsaved changes. Are you sure you want to cancel?',
        confirmText: 'Yes, discard',
        cancelText: 'Keep editing'
      }).subscribe(cancelled => {
        if (cancelled) this.executeCancel();
      });
    }
    else {
      this.executeCancel();
    }
  }

  executeCancel() {
    this.router.navigate(['app/salesorder']);
  }

  handleCreateLines(salesOrder: CreateSalesOrderDto) {
      this.storage.store('SalesOrder', salesOrder);
      this.router.navigate(['app/salesorderline/edit', 'add', 1]);
    }
}

