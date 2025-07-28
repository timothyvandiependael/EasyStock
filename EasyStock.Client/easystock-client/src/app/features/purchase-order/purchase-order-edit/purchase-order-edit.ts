import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { PurchaseOrderService } from '../purchase-order-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { PurchaseOrderDetailDto } from '../dtos/purchase-order-detail.dto';
import { CreatePurchaseOrderDto } from '../dtos/create-purchase-order.dto';
import { UpdatePurchaseOrderDto } from '../dtos/update-purchase-order.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';

@Component({
  selector: 'app-purchase-order-edit',
  imports: [EditView],
  templateUrl: './purchase-order-edit.html',
  styleUrl: './purchase-order-edit.css'
})
export class PurchaseOrderEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedPurchaseOrder?: PurchaseOrderDetailDto;

  @ViewChild(EditView) detailView!: EditView<PurchaseOrderDetailDto>;


  constructor(
    private purchaseOrderService: PurchaseOrderService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService) { }

  ngOnInit() {
    this.loadColumnMeta();
  }

  loadColumnMeta() {
    this.getColumnsSub = this.purchaseOrderService.getColumns().subscribe({
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
        this.getByIdSub = this.purchaseOrderService.getById(id).subscribe({
          next: (dto: PurchaseOrderDetailDto) => {
            this.selectedPurchaseOrder = dto;
          },
          error: (err) => {
            console.error('Error retrieving purchaseOrder with id ' + id + ': ', err);
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

  handleSaveAndAddAnother(purchaseOrder: CreatePurchaseOrderDto) {
    this.saveAndAddSub = this.purchaseOrderService.add(purchaseOrder).subscribe({
      next: (saved: PurchaseOrderDetailDto) => {
        this.selectedPurchaseOrder = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`Purchase order saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving purchaseOrder ', err);
        this.persistentSnackbar.showError(`Error saving purchase order. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(purchaseOrder: CreatePurchaseOrderDto) {
    this.saveNewExitSub = this.purchaseOrderService.add(purchaseOrder).subscribe({
      next: (saved: PurchaseOrderDetailDto) => {
        this.selectedPurchaseOrder = undefined;
        this.snackBar.open(`${saved.orderNumber} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/purchaseorder']);
      },
      error: (err) => {
        console.error('Error saving purchaseOrder ', err);
        this.persistentSnackbar.showError(`Error saving purchase order. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(purchaseOrder: UpdatePurchaseOrderDto) {
    this.saveExitSub = this.purchaseOrderService.edit(purchaseOrder.id, purchaseOrder).subscribe({
      next: () => {
        this.snackBar.open(`Purchase order updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/purchaseorder']);
      },
      error: (err) => {
        console.error('Error updating purchaseOrder: ', err);
        this.persistentSnackbar.showError(`Error saving purchase order. If the problem persists, please contact support.`);
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
    this.router.navigate(['app/purchaseorder']);
  }
}

