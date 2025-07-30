import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { DispatchService } from '../dispatch-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { DispatchDetailDto } from '../dtos/dispatch-detail.dto';
import { CreateDispatchDto } from '../dtos/create-dispatch.dto';
import { UpdateDispatchDto } from '../dtos/update-dispatch.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';
import { StorageService } from '../../../shared/storage/storage-service';

@Component({
  selector: 'app-dispatch-edit',
  imports: [EditView],
  templateUrl: './dispatch-edit.html',
  styleUrl: './dispatch-edit.css'
})
export class DispatchEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedDispatch?: DispatchDetailDto;

  procedureStep1 = true;
  procedureStep2 = false;

  addModeHideFields = [
    'dispatchNumber', 'status'
  ]

  @ViewChild(EditView) detailView!: EditView<DispatchDetailDto>;


  constructor(
    private dispatchService: DispatchService,
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
    this.getColumnsSub = this.dispatchService.getColumns().subscribe({
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
        this.getByIdSub = this.dispatchService.getById(id).subscribe({
          next: (dto: DispatchDetailDto) => {
            this.selectedDispatch = dto;
          },
          error: (err) => {
            console.error('Error retrieving dispatch with id ' + id + ': ', err);
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

  handleSaveAndAddAnother(dispatch: CreateDispatchDto) {
    this.saveAndAddSub = this.dispatchService.add(dispatch).subscribe({
      next: (saved: DispatchDetailDto) => {
        this.selectedDispatch = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`${saved.dispatchNumber} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving dispatch ', err);
        this.persistentSnackbar.showError(`Error saving dispatch. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(dispatch: CreateDispatchDto) {
    this.saveNewExitSub = this.dispatchService.add(dispatch).subscribe({
      next: (saved: DispatchDetailDto) => {
        this.selectedDispatch = undefined;
        this.snackBar.open(`${saved.dispatchNumber} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/dispatch']);
      },
      error: (err) => {
        console.error('Error saving dispatch ', err);
        this.persistentSnackbar.showError(`Error saving dispatch. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(dispatch: UpdateDispatchDto) {
    this.saveExitSub = this.dispatchService.edit(dispatch.id, dispatch).subscribe({
      next: () => {
        this.snackBar.open(`Dispatch updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/dispatch']);
      },
      error: (err) => {
        console.error('Error updating dispatch: ', err);
        this.persistentSnackbar.showError(`Error saving dispatch. If the problem persists, please contact support.`);
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
    this.router.navigate(['app/dispatch']);
  }

  handleCreateLines(dispatch: CreateDispatchDto) {
      this.storage.store('Dispatch', dispatch);
      this.router.navigate(['app/dispatchline/edit', 'add', 1]);
    }
}

