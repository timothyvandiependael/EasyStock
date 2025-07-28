import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { PurchaseOrderLineService } from '../purchase-order-line-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { PurchaseOrderLineDetailDto } from '../dtos/purchase-order-line-detail.dto';
import { CreatePurchaseOrderLineDto } from '../dtos/create-purchase-order-line.dto';
import { UpdatePurchaseOrderLineDto } from '../dtos/update-purchase-order-line.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';

@Component({
  selector: 'app-purchase-order-line-edit',
  imports: [EditView],
  templateUrl: './purchase-order-line-edit.html',
  styleUrl: './purchase-order-line-edit.css'
})
export class PurchaseOrderLineEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedPurchaseOrderLine?: PurchaseOrderLineDetailDto;

  @ViewChild(EditView) detailView!: EditView<PurchaseOrderLineDetailDto>;


  constructor(
    private purchaseOrderLineService: PurchaseOrderLineService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService) { }

  ngOnInit() {
    this.loadColumnMeta();
  }

  loadColumnMeta() {
    this.getColumnsSub = this.purchaseOrderLineService.getColumns().subscribe({
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
        this.getByIdSub = this.purchaseOrderLineService.getById(id).subscribe({
          next: (dto: PurchaseOrderLineDetailDto) => {
            this.selectedPurchaseOrderLine = dto;
          },
          error: (err) => {
            console.error('Error retrieving purchaseOrderLine with id ' + id + ': ', err);
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

  handleSaveAndAddAnother(purchaseOrderLine: CreatePurchaseOrderLineDto) {
    this.saveAndAddSub = this.purchaseOrderLineService.add(purchaseOrderLine).subscribe({
      next: (saved: PurchaseOrderLineDetailDto) => {
        this.selectedPurchaseOrderLine = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`Purchase order line saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving purchaseOrderLine ', err);
        this.persistentSnackbar.showError(`Error saving purchase order line. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(purchaseOrderLine: CreatePurchaseOrderLineDto) {
    this.saveNewExitSub = this.purchaseOrderLineService.add(purchaseOrderLine).subscribe({
      next: (saved: PurchaseOrderLineDetailDto) => {
        this.selectedPurchaseOrderLine = undefined;
        this.snackBar.open(`Purchase order line saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/purchaseorderline']);
      },
      error: (err) => {
        console.error('Error saving purchaseOrderLine ', err);
        this.persistentSnackbar.showError(`Error saving purchase order line. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(purchaseOrderLine: UpdatePurchaseOrderLineDto) {
    this.saveExitSub = this.purchaseOrderLineService.edit(purchaseOrderLine.id, purchaseOrderLine).subscribe({
      next: () => {
        this.snackBar.open(`Purchase order line updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/purchaseorderline']);
      },
      error: (err) => {
        console.error('Error updating purchaseOrderLine: ', err);
        this.persistentSnackbar.showError(`Error saving purchase order line. If the problem persists, please contact support.`);
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
    this.router.navigate(['app/purchaseorderline']);
  }
}
