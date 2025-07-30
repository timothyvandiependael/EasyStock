import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { SupplierService } from '../supplier-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { SupplierDetailDto } from '../dtos/supplier-detail.dto';
import { CreateSupplierDto } from '../dtos/create-supplier.dto';
import { UpdateSupplierDto } from '../dtos/update-supplier.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';

@Component({
  selector: 'app-supplier-edit',
  imports: [EditView],
  templateUrl: './supplier-edit.html',
  styleUrl: './supplier-edit.css'
})
export class SupplierEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedSupplier?: SupplierDetailDto;

  @ViewChild(EditView) detailView!: EditView<SupplierDetailDto>;


  constructor(
    private supplierService: SupplierService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService) { }

  ngOnInit() {
    this.loadColumnMeta();
  }

  loadColumnMeta() {
    this.getColumnsSub = this.supplierService.getColumns().subscribe({
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
        this.getByIdSub = this.supplierService.getById(id).subscribe({
          next: (dto: SupplierDetailDto) => {
            this.selectedSupplier = dto;
          },
          error: (err) => {
            console.error('Error retrieving supplier with id ' + id + ': ', err);
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

  handleSaveAndAddAnother(supplier: CreateSupplierDto) {
    this.saveAndAddSub = this.supplierService.add(supplier).subscribe({
      next: (saved: SupplierDetailDto) => {
        this.selectedSupplier = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`${saved.name} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        debugger;
        console.error('Error saving supplier ', err);
        this.persistentSnackbar.showError(`Error saving ${supplier.name}. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(supplier: CreateSupplierDto) {
    this.saveNewExitSub = this.supplierService.add(supplier).subscribe({
      next: (saved: SupplierDetailDto) => {
        this.selectedSupplier = undefined;
        this.snackBar.open(`${saved.name} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/supplier']);
      },
      error: (err) => {
        console.error('Error saving supplier ', err);
        this.persistentSnackbar.showError(`Error saving ${supplier.name}. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(supplier: UpdateSupplierDto) {
    this.saveExitSub = this.supplierService.edit(supplier.id, supplier).subscribe({
      next: () => {
        this.snackBar.open(`${supplier.name} updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/supplier']);
      },
      error: (err) => {
        console.error('Error updating supplier: ', err);
        this.persistentSnackbar.showError(`Error saving ${supplier.name}. If the problem persists, please contact support.`);
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
    this.router.navigate(['app/supplier']);
  }
}

