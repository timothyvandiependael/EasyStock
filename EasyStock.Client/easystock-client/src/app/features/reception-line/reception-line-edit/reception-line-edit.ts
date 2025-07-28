import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { ReceptionLineService } from '../reception-line-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ReceptionLineDetailDto } from '../dtos/reception-line-detail.dto';
import { CreateReceptionLineDto } from '../dtos/create-reception-line.dto';
import { UpdateReceptionLineDto } from '../dtos/update-reception-line.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';

@Component({
  selector: 'app-reception-line-edit',
  imports: [EditView],
  templateUrl: './reception-line-edit.html',
  styleUrl: './reception-line-edit.css'
})
export class ReceptionLineEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedReceptionLine?: ReceptionLineDetailDto;

  @ViewChild(EditView) detailView!: EditView<ReceptionLineDetailDto>;


  constructor(
    private receptionLineService: ReceptionLineService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService) { }

  ngOnInit() {
    this.loadColumnMeta();
  }

  loadColumnMeta() {
    this.getColumnsSub = this.receptionLineService.getColumns().subscribe({
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
        this.getByIdSub = this.receptionLineService.getById(id).subscribe({
          next: (dto: ReceptionLineDetailDto) => {
            this.selectedReceptionLine = dto;
          },
          error: (err) => {
            console.error('Error retrieving receptionLine with id ' + id + ': ', err);
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

  handleSaveAndAddAnother(receptionLine: CreateReceptionLineDto) {
    this.saveAndAddSub = this.receptionLineService.add(receptionLine).subscribe({
      next: (saved: ReceptionLineDetailDto) => {
        this.selectedReceptionLine = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`Reception line saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving receptionLine ', err);
        this.persistentSnackbar.showError(`Error saving reception line. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(receptionLine: CreateReceptionLineDto) {
    this.saveNewExitSub = this.receptionLineService.add(receptionLine).subscribe({
      next: (saved: ReceptionLineDetailDto) => {
        this.selectedReceptionLine = undefined;
        this.snackBar.open(`Reception line saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/receptionline']);
      },
      error: (err) => {
        console.error('Error saving receptionLine ', err);
        this.persistentSnackbar.showError(`Error saving reception line. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(receptionLine: UpdateReceptionLineDto) {
    this.saveExitSub = this.receptionLineService.edit(receptionLine.id, receptionLine).subscribe({
      next: () => {
        this.snackBar.open(`Reception line updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/receptionline']);
      },
      error: (err) => {
        console.error('Error updating receptionLine: ', err);
        this.persistentSnackbar.showError(`Error saving reception line. If the problem persists, please contact support.`);
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
    this.router.navigate(['app/receptionline']);
  }
}

