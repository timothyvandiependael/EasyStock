import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { StockMovementService } from '../stock-movement-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { StockMovementDetailDto } from '../dtos/stock-movement-detail.dto';
import { CreateStockMovementDto } from '../dtos/create-stock-movement.dto';
import { UpdateStockMovementDto } from '../dtos/update-stock-movement.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';

@Component({
  selector: 'app-stock-movement-edit',
  imports: [EditView],
  templateUrl: './stock-movement-edit.html',
  styleUrl: './stock-movement-edit.css'
})
export class StockMovementEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedStockMovement?: StockMovementDetailDto;

  @ViewChild(EditView) detailView!: EditView<StockMovementDetailDto>;


  constructor(
    private stockMovementService: StockMovementService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService) { }

  ngOnInit() {
    this.loadColumnMeta();
  }

  loadColumnMeta() {
    this.getColumnsSub = this.stockMovementService.getColumns().subscribe({
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
        this.getByIdSub = this.stockMovementService.getById(id).subscribe({
          next: (dto: StockMovementDetailDto) => {
            this.selectedStockMovement = dto;
          },
          error: (err) => {
            console.error('Error retrieving stockMovement with id ' + id + ': ', err);
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

  handleSaveAndAddAnother(stockMovement: CreateStockMovementDto) {
    this.saveAndAddSub = this.stockMovementService.add(stockMovement).subscribe({
      next: (saved: StockMovementDetailDto) => {
        this.selectedStockMovement = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`Stock movement saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving stockMovement ', err);
        this.persistentSnackbar.showError(`Error saving stock movement. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(stockMovement: CreateStockMovementDto) {
    this.saveNewExitSub = this.stockMovementService.add(stockMovement).subscribe({
      next: (saved: StockMovementDetailDto) => {
        this.selectedStockMovement = undefined;
        this.snackBar.open(`Stock movement saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/stockmovement']);
      },
      error: (err) => {
        console.error('Error saving stockMovement ', err);
        this.persistentSnackbar.showError(`Error saving stock movement. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(stockMovement: UpdateStockMovementDto) {
    this.saveExitSub = this.stockMovementService.edit(stockMovement.id, stockMovement).subscribe({
      next: () => {
        this.snackBar.open(`Stock movement updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/stockmovement']);
      },
      error: (err) => {
        console.error('Error updating stockMovement: ', err);
        this.persistentSnackbar.showError(`Error saving stock movement. If the problem persists, please contact support.`);
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
    this.router.navigate(['app/stockmovement']);
  }
}

