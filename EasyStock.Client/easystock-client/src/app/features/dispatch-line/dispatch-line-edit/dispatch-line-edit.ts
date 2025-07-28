import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { DispatchLineService } from '../dispatch-line-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { DispatchLineDetailDto } from '../dtos/dispatch-line-detail.dto';
import { CreateDispatchLineDto } from '../dtos/create-dispatch-line.dto';
import { UpdateDispatchLineDto } from '../dtos/update-dispatch-line.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';

@Component({
  selector: 'app-dispatchLine-edit',
  imports: [EditView],
  templateUrl: './dispatch-line-edit.html',
  styleUrl: './dispatch-line-edit.css'
})
export class DispatchLineEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedDispatchLine?: DispatchLineDetailDto;

  @ViewChild(EditView) detailView!: EditView<DispatchLineDetailDto>;


  constructor(
    private dispatchLineService: DispatchLineService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService) { }

  ngOnInit() {
    this.loadColumnMeta();
  }

  loadColumnMeta() {
    this.getColumnsSub = this.dispatchLineService.getColumns().subscribe({
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
        this.getByIdSub = this.dispatchLineService.getById(id).subscribe({
          next: (dto: DispatchLineDetailDto) => {
            this.selectedDispatchLine = dto;
          },
          error: (err) => {
            console.error('Error retrieving dispatchLine with id ' + id + ': ', err);
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

  handleSaveAndAddAnother(dispatchLine: CreateDispatchLineDto) {
    this.saveAndAddSub = this.dispatchLineService.add(dispatchLine).subscribe({
      next: (saved: DispatchLineDetailDto) => {
        this.selectedDispatchLine = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`${saved.lineNumber} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving dispatchLine ', err);
        this.persistentSnackbar.showError(`Error saving dispatch line. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(dispatchLine: CreateDispatchLineDto) {
    this.saveNewExitSub = this.dispatchLineService.add(dispatchLine).subscribe({
      next: (saved: DispatchLineDetailDto) => {
        this.selectedDispatchLine = undefined;
        this.snackBar.open(`${saved.lineNumber} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/dispatchline']);
      },
      error: (err) => {
        console.error('Error saving dispatchLine ', err);
        this.persistentSnackbar.showError(`Error saving dispatch line. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(dispatchLine: UpdateDispatchLineDto) {
    this.saveExitSub = this.dispatchLineService.edit(dispatchLine.id, dispatchLine).subscribe({
      next: () => {
        this.snackBar.open(`Dispatch line updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/dispatchline']);
      },
      error: (err) => {
        console.error('Error updating dispatchLine: ', err);
        this.persistentSnackbar.showError(`Error saving dispatch line. If the problem persists, please contact support.`);
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
    this.router.navigate(['app/dispatchline']);
  }
}

