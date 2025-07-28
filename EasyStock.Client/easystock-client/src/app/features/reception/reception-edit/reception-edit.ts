import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { ReceptionService } from '../reception-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ReceptionDetailDto } from '../dtos/reception-detail.dto';
import { CreateReceptionDto } from '../dtos/create-reception.dto';
import { UpdateReceptionDto } from '../dtos/update-reception.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';

@Component({
  selector: 'app-reception-edit',
  imports: [EditView],
  templateUrl: './reception-edit.html',
  styleUrl: './reception-edit.css'
})
export class ReceptionEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedReception?: ReceptionDetailDto;

  @ViewChild(EditView) detailView!: EditView<ReceptionDetailDto>;


  constructor(
    private receptionService: ReceptionService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService) { }

  ngOnInit() {
    this.loadColumnMeta();
  }

  loadColumnMeta() {
    this.getColumnsSub = this.receptionService.getColumns().subscribe({
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
        this.getByIdSub = this.receptionService.getById(id).subscribe({
          next: (dto: ReceptionDetailDto) => {
            this.selectedReception = dto;
          },
          error: (err) => {
            console.error('Error retrieving reception with id ' + id + ': ', err);
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

  handleSaveAndAddAnother(reception: CreateReceptionDto) {
    this.saveAndAddSub = this.receptionService.add(reception).subscribe({
      next: (saved: ReceptionDetailDto) => {
        this.selectedReception = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`Reception saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving reception ', err);
        this.persistentSnackbar.showError(`Error saving reception. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(reception: CreateReceptionDto) {
    this.saveNewExitSub = this.receptionService.add(reception).subscribe({
      next: (saved: ReceptionDetailDto) => {
        this.selectedReception = undefined;
        this.snackBar.open(`Reception saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/reception']);
      },
      error: (err) => {
        console.error('Error saving reception ', err);
        this.persistentSnackbar.showError(`Error saving reception. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(reception: UpdateReceptionDto) {
    this.saveExitSub = this.receptionService.edit(reception.id, reception).subscribe({
      next: () => {
        this.snackBar.open(`Reception updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/reception']);
      },
      error: (err) => {
        console.error('Error updating reception: ', err);
        this.persistentSnackbar.showError(`Error saving reception. If the problem persists, please contact support.`);
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
    this.router.navigate(['app/reception']);
  }
}

