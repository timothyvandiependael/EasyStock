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

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedSalesOrderLine?: SalesOrderLineDetailDto;

  @ViewChild(EditView) detailView!: EditView<SalesOrderLineDetailDto>;


  constructor(
    private salesOrderLineService: SalesOrderLineService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService) { }

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
  }

  handleSaveAndAddAnother(salesOrderLine: CreateSalesOrderLineDto) {
    this.saveAndAddSub = this.salesOrderLineService.add(salesOrderLine).subscribe({
      next: (saved: SalesOrderLineDetailDto) => {
        this.selectedSalesOrderLine = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`Sales order line saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving salesOrderLine ', err);
        this.persistentSnackbar.showError(`Error saving sales order line. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(salesOrderLine: CreateSalesOrderLineDto) {
    this.saveNewExitSub = this.salesOrderLineService.add(salesOrderLine).subscribe({
      next: (saved: SalesOrderLineDetailDto) => {
        this.selectedSalesOrderLine = undefined;
        this.snackBar.open(`Sales order line saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/salesorderline']);
      },
      error: (err) => {
        console.error('Error saving salesOrderLine ', err);
        this.persistentSnackbar.showError(`Error saving sales order line. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(salesOrderLine: UpdateSalesOrderLineDto) {
    this.saveExitSub = this.salesOrderLineService.edit(salesOrderLine.id, salesOrderLine).subscribe({
      next: () => {
        this.snackBar.open(`Sales order line updated`, 'Close', {
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
}
