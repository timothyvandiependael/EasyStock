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
import { StorageService } from '../../../shared/storage/storage-service';
import { CreatePurchaseOrderDto } from '../../purchase-order/dtos/create-purchase-order.dto';
import { PurchaseOrderService } from '../../purchase-order/purchase-order-service';
import { PurchaseOrderDetailDto } from '../../purchase-order/dtos/purchase-order-detail.dto';

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
  private purchaseOrderSaveSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedPurchaseOrderLine?: PurchaseOrderLineDetailDto;

  parentId?: number = undefined;

  filledInFields: any = {};

  procedureStep2 = false;

  addModeHideFields = [
    'lineNumber', 'status', 'deliveredQuantity'
  ]

  @ViewChild(EditView) detailView!: EditView<PurchaseOrderLineDetailDto>;


  constructor(
    private purchaseOrderLineService: PurchaseOrderLineService,
    private purchaseOrderService: PurchaseOrderService,
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

      if (mode == 'add') {
        const fromParent = params.get('id');
        if (fromParent) {
          this.procedureStep2 = true;
          this.addModeHideFields.push('orderNumber');
        }
        else {
          this.addModeHideFields = [
            'lineNumber', 'status', 'deliveredQuantity'
          ]
        }


        this.route.queryParamMap.subscribe(queryParams => {
          const parentId = queryParams.get('parentId');
          const parentNumber = queryParams.get('parentNumber');
          if (!parentId) {
            this.parentId = undefined;
          }
          else {
            this.parentId = parseInt(parentId);
          }

          if (parentNumber) {
            this.filledInFields = {
              orderNumber: parentNumber
            }
          }
        });
      }

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
    this.purchaseOrderSaveSub?.unsubscribe();
  }

  handleSaveAndAddAnother(purchaseOrderLine: CreatePurchaseOrderLineDto) {
    if (this.parentId) {
      purchaseOrderLine.purchaseOrderId = this.parentId;
    }
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
    if (this.parentId) {
      purchaseOrderLine.purchaseOrderId = this.parentId;
    }
    this.saveNewExitSub = this.purchaseOrderLineService.add(purchaseOrderLine).subscribe({
      next: (saved: PurchaseOrderLineDetailDto) => {
        this.selectedPurchaseOrderLine = undefined;
        this.snackBar.open(`Purchase order line saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });

        if (this.parentId) {
          this.router.navigate(['app/purchaseorderline'], {
            queryParams: {
              parentId: this.parentId
            }
          });
        }
        else {
          this.router.navigate(['app/purchaseorderline']);
        }

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

  handleAddMoreLines(line: CreatePurchaseOrderLineDto) {
    this.addLineToOrder(line);
    this.selectedPurchaseOrderLine = undefined;
    this.detailView.clearForm();
  }

  handleSaveAllAndExit(line: CreatePurchaseOrderLineDto) {
    this.addLineToOrder(line);
    this.saveOrder();

  }

  saveOrder() {
    var po = this.getOrder();

    this.purchaseOrderSaveSub = this.purchaseOrderService.add(po).subscribe({
      next: (saved: PurchaseOrderDetailDto) => {
        this.selectedPurchaseOrderLine = undefined;
        this.snackBar.open(`Purchase order saved`, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/purchaseorder']);
      },
      error: (err) => {
        this.persistentSnackbar.showError(`Error saving purchase order. If the problem persists, please contact support.`);
        this.removeLastLineAfterError();
      }
    })
  }

  removeLastLineAfterError() {
    var po = this.getOrder();
    if (po.lines.length > 0) {
      po.lines.pop();
    }
    this.storage.store('PurchaseOrder', po);
  }

  addLineToOrder(line: CreatePurchaseOrderLineDto) {
    var po = this.getOrder();
    po.lines.push(line);
    this.storage.store('PurchaseOrder', po);
  }

  getOrder() {
    var po = this.storage.retrieve('PurchaseOrder') as CreatePurchaseOrderDto;
    if (!po) {
      this.persistentSnackbar.showError(`Error retrieving purchase order. If the problem persist, contact support.`);
    }
    if (po.lines == null) po.lines = [];
    return po;
  }

  handleProcedureCancel() {
    this.confirmDialogService.open({
      title: 'Discard order?',
      message: 'If you exit now, the entire order will not be saved. Discard?',
      confirmText: 'Yes, discard',
      cancelText: 'Keep editing'
    }).subscribe(cancelled => {
      if (cancelled) {
        this.router.navigate(['app/purchaseorder']);
      }
    });

  }
}
