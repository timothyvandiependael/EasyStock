import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { SalesOrderLineService } from '../sales-order-line-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { SalesOrderLineDetailDto } from '../dtos/sales-order-line-detail.dto';
import { CreateSalesOrderLineDto } from '../dtos/create-sales-order-line.dto';
import { UpdateSalesOrderLineDto } from '../dtos/update-sales-order-line.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';
import { SalesOrderService } from '../../sales-order/sales-order-service';
import { StorageService } from '../../../shared/storage/storage-service';
import { SalesOrderDetailDto } from '../../sales-order/dtos/sales-order-detail.dto';
import { CreateSalesOrderDto } from '../../sales-order/dtos/create-sales-order.dto';
import { AutoRestockDto } from '../../../shared/autorestock.dto';

@Component({
  selector: 'app-sales-order-line-edit',
  imports: [EditView],
  templateUrl: './sales-order-line-edit.html',
  styleUrl: './sales-order-line-edit.css'
})
export class SalesOrderLineEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;
  private salesOrderSaveSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedSalesOrderLine?: SalesOrderLineDetailDto;

  procedureStep2 = false;

  parentId?: number = undefined;

  filledInFields: any = {};

  addModeHideFields = [
    'lineNumber', 'status', 'dispatchedQuantity'
  ]

  @ViewChild(EditView) detailView!: EditView<SalesOrderLineDetailDto>;


  constructor(
    private salesOrderLineService: SalesOrderLineService,
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
    this.getColumnsSub = this.salesOrderLineService.getColumns().subscribe({
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
            'lineNumber', 'status'
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
        this.getByIdSub = this.salesOrderLineService.getById(id).subscribe({
          next: (dto: SalesOrderLineDetailDto) => {
            this.selectedSalesOrderLine = dto;
          },
          error: (err) => {
            console.error('Error retrieving salesOrderLine with id ' + id + ': ', err);
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
    this.salesOrderSaveSub?.unsubscribe();
  }

  handleSaveAndAddAnother(salesOrderLine: CreateSalesOrderLineDto) {
    if (this.parentId) {
      salesOrderLine.salesOrderId = this.parentId;
    }
    this.saveAndAddSub = this.salesOrderLineService.add(salesOrderLine).subscribe({
      next: (dto: AutoRestockDto) => {
        this.selectedSalesOrderLine = undefined;
        this.detailView.clearForm();

        var snackbarText = dto.autoRestocked
          ? `Sales order line saved. Product ${dto.productName} was reduced below minimum available stock. Purchase order ${dto.autoRestockPurchaseOrderNumber} has been created for auto restock.`
          : `Sales order line saved`

        if (dto.productShortage > 0 && !dto.autoRestocked) {
          this.persistentSnackbar.showMessage(`Sales order line saved. Warning: Product ${dto.productName} has a shortage of ${dto.productShortage} considering backorder stock and minimum stock requirements. Please create a purchase order for this amount.`)
        }
        else {
          this.snackBar.open(snackbarText, 'Close', {
            duration: 3000, // 3 seconds
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        }


      },
      error: (err) => {
        console.error('Error saving salesOrderLine ', err);
        this.persistentSnackbar.showError(`Error saving sales order line. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(salesOrderLine: CreateSalesOrderLineDto) {
    if (this.parentId) {
      salesOrderLine.salesOrderId = this.parentId;
    }
    this.saveNewExitSub = this.salesOrderLineService.add(salesOrderLine).subscribe({
      next: (dto: AutoRestockDto) => {
        this.selectedSalesOrderLine = undefined;
        var snackbarText = dto.autoRestocked
          ? `Sales order line saved. Product ${dto.productName} was reduced below minimum available stock. Purchase order ${dto.autoRestockPurchaseOrderNumber} has been created for auto restock.`
          : `Sales order line saved`;

        if (dto.productShortage > 0 && !dto.autoRestocked) {
          this.persistentSnackbar.showMessage(`Sales order line saved. Warning: Product ${dto.productName} has a shortage of ${dto.productShortage} considering backorder stock and minimum stock requirements. Please create a purchase order for this amount.`)
        }
        else {
          this.snackBar.open(snackbarText, 'Close', {
            duration: 3000, // 3 seconds
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        }

        if (this.parentId) {
          this.router.navigate(['app/salesorderline'], {
            queryParams: {
              parentId: this.parentId
            }
          });
        }
        else {
          this.router.navigate(['app/salesorderline']);
        }
      },
      error: (err) => {
        this.persistentSnackbar.showError(`Error saving sales order line. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(salesOrderLine: UpdateSalesOrderLineDto) {
    this.saveExitSub = this.salesOrderLineService.edit(salesOrderLine.id, salesOrderLine).subscribe({
      next: (dto: AutoRestockDto) => {
        var snackbarText = dto.autoRestocked
          ? `Sales order line saved. Product ${dto.productName} was reduced below minimum available stock. Purchase order ${dto.autoRestockPurchaseOrderNumber} has been created for auto restock.`
          : `Sales order line saved`;

        this.snackBar.open(snackbarText, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/salesorderline']);
      },
      error: (err) => {
        console.error('Error updating salesOrderLine: ', err);
        this.persistentSnackbar.showError(`Error saving sales order line. If the problem persists, please contact support.`);
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
    this.router.navigate(['app/salesorderline']);
  }

  handleAddMoreLines(line: CreateSalesOrderLineDto) {
    this.addLineToOrder(line);
    this.selectedSalesOrderLine = undefined;
    this.detailView.clearForm();
  }

  handleSaveAllAndExit(line: CreateSalesOrderLineDto) {
    this.addLineToOrder(line);
    this.saveOrder();

  }

  saveOrder() {
    var so = this.getOrder();

    this.salesOrderSaveSub = this.salesOrderService.add(so).subscribe({
      next: (saved: SalesOrderDetailDto) => {
        debugger;
        this.selectedSalesOrderLine = undefined;

        if (saved.autoRestockDtos && saved.autoRestockDtos.length > 0) {
          var autoRestockedProducts = saved.autoRestockDtos.filter(a => a.autoRestocked);
          var productsWithShortage = saved.autoRestockDtos.filter(a => !a.autoRestocked && a.productShortage > 0);

          var snackbarText = '';

          if (autoRestockedProducts.length > 0) {
            snackbarText += 'The following products have auto restock on and have had purchase orders created for them: ';
            autoRestockedProducts.forEach(p => {
              snackbarText += `${p.productName}: purchase order ${p.autoRestockPurchaseOrderNumber} ;`
            })
          }

          if (productsWithShortage.length > 0) {
            snackbarText += 'The following products have a shortage, being under minimum stock and/or having backordered stock: ';
            productsWithShortage.forEach(p => {
              snackbarText += `${p.productName} has a shortage of ${p.productShortage} ;`
            })
            snackbarText += 'Please create purchase orders for these products compensating for the respective shortages.';
          }

          this.persistentSnackbar.showMessage(snackbarText);
        }
        else {
          this.snackBar.open(`Sales order saved`, 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        }

        this.router.navigate(['app/salesorder']);
      },
      error: (err) => {
        this.persistentSnackbar.showError(`Error saving sales order. If the problem persists, please contact support.`);
        this.removeLastLineAfterError();
      }
    })
  }

  removeLastLineAfterError() {
    var so = this.getOrder();
    if (so.lines.length > 0) {
      so.lines.pop();
    }
    this.storage.store('SalesOrder', so);
  }

  addLineToOrder(line: CreateSalesOrderLineDto) {
    var so = this.getOrder();
    so.lines.push(line);
    this.storage.store('SalesOrder', so);
  }

  getOrder() {
    var so = this.storage.retrieve('SalesOrder') as CreateSalesOrderDto;
    if (!so) {
      this.persistentSnackbar.showError(`Error retrieving sales order. If the problem persist, contact support.`);
    }
    if (so.lines == null) so.lines = [];
    return so;
  }

  handleProcedureCancel() {
    this.confirmDialogService.open({
      title: 'Discard order?',
      message: 'If you exit now, the entire order will not be saved. Discard?',
      confirmText: 'Yes, discard',
      cancelText: 'Keep editing'
    }).subscribe(cancelled => {
      if (cancelled) {
        this.router.navigate(['app/salesorder']);
      }
    });

  }
}
