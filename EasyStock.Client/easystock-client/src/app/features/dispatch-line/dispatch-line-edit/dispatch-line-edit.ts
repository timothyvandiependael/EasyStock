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
import { DispatchService } from '../../dispatch/dispatch-service';
import { StorageService } from '../../../shared/storage/storage-service';
import { DispatchDetailDto } from '../../dispatch/dtos/dispatch-detail.dto';
import { CreateDispatchDto } from '../../dispatch/dtos/create-dispatch.dto';
import { FilterCondition } from '../../../shared/query';

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
  private dispatchSaveSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedDispatchLine?: DispatchLineDetailDto;

  parentId?: number = undefined;

  procedureStep2 = false;

  addModeHideFields = [
    'lineNumber', 'status'
  ]

  additionalFilters: any;

  filledInFields: any = {};

  @ViewChild(EditView) detailView!: EditView<DispatchLineDetailDto>;


  constructor(
    private dispatchLineService: DispatchLineService,
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

      if (mode == 'add') {
        const fromParent = params.get('id');
        if (fromParent) {
          this.procedureStep2 = true;
          this.addModeHideFields.push('dispatchNumber');
          this.addAdditionalFilters();

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
            this.addAdditionalFilters();
          }

          if (parentNumber) {
            this.filledInFields = {
              dispatchNumber: parentNumber
            }
          }
        });
      }

      if (mode === 'edit') {
        const id = Number(params.get('id'));
        this.getByIdSub = this.dispatchLineService.getById(id).subscribe({
          next: (dto: DispatchLineDetailDto) => {
            this.selectedDispatchLine = dto;
            this.addAdditionalFilters();
          },
          error: (err) => {
            this.persistentSnackbar.showError(`Error retrieving record with id ${id}. If the problem persists, please contact support.`);
          }
        })

      }
    });
  }

  addAdditionalFilters() {
    var dispatch = undefined;
    if (this.detailMode == 'add') {
      if (this.procedureStep2) { // new order
        dispatch = this.storage.retrieve("Dispatch");
        this.addFilters(dispatch);
      }
      else { // from parent order
        if (this.parentId) {
          this.dispatchService.getById(this.parentId).subscribe({
            next: (dto: DispatchDetailDto) => {
              this.addFilters(dto);
            },
            error: (err) => {
              this.persistentSnackbar.showError(`Error retrieving parent for additional lookup filtering. If the problem persists, please contact support.`);
            }
          })
        }

      }

    }
    else if (this.detailMode == 'edit') {
      dispatch = this.selectedDispatchLine?.dispatch;
      this.addFilters(dispatch);
    }
  }

  addFilters(dispatch: any) {
    var salesOrderLineFc: FilterCondition[] = [
      {
        field: 'ClientId',
        operator: 'equals',
        value: dispatch.clientId
      },
      { 
        field: 'Status',
        operator: 'notequals',
        value: 'Complete'
      }
    ];

    if (!this.additionalFilters) {
      this.additionalFilters = {};
    }

    this.additionalFilters.salesorderline = salesOrderLineFc;
  }

  ngOnDestroy() {
    this.routeSub?.unsubscribe();
    this.getColumnsSub?.unsubscribe();
    this.saveAndAddSub?.unsubscribe();
    this.saveNewExitSub?.unsubscribe();
    this.saveExitSub?.unsubscribe();
    this.dispatchSaveSub?.unsubscribe();
  }

  handleSaveAndAddAnother(dispatchLine: CreateDispatchLineDto) {
    if (this.parentId) {
      dispatchLine.dispatchId = this.parentId;
    }
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
    if (this.parentId) {
      dispatchLine.dispatchId = this.parentId;
    }
    this.saveNewExitSub = this.dispatchLineService.add(dispatchLine).subscribe({
      next: (saved: DispatchLineDetailDto) => {
        this.selectedDispatchLine = undefined;
        this.snackBar.open(`${saved.lineNumber} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });

        if (this.parentId) {
          this.router.navigate(['app/dispatchline'], {
            queryParams: {
              parentId: this.parentId
            }
          });
        }
        else {
          this.router.navigate(['app/dispatchline']);
        }
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


  handleAddMoreLines(line: CreateDispatchLineDto) {
    this.addLineToOrder(line);
    this.selectedDispatchLine = undefined;
    this.detailView.clearForm();
  }

  handleSaveAllAndExit(line: CreateDispatchLineDto) {
    this.addLineToOrder(line);
    this.saveOrder();

  }

  saveOrder() {
    var di = this.getOrder();

    this.dispatchSaveSub = this.dispatchService.add(di).subscribe({
      next: (saved: DispatchDetailDto) => {
        this.selectedDispatchLine = undefined;
        this.snackBar.open(`Dispatch saved`, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/dispatch']);
      },
      error: (err) => {
        this.persistentSnackbar.showError(`Error saving dispatch. If the problem persists, please contact support.`);
        this.removeLastLineAfterError();
      }
    })
  }

  removeLastLineAfterError() {
    var di = this.getOrder();
    if (di.lines.length > 0) {
      di.lines.pop();
    }
    this.storage.store('Dispatch', di);
  }

  addLineToOrder(line: CreateDispatchLineDto) {
    var di = this.getOrder();
    di.lines.push(line);
    this.storage.store('Dispatch', di);
  }

  getOrder() {
    var di = this.storage.retrieve('PurchaseOrder') as CreateDispatchDto;
    if (!di) {
      this.persistentSnackbar.showError(`Error retrieving dispatch. If the problem persist, contact support.`);
    }
    if (di.lines == null) di.lines = [];
    return di;
  }

  handleProcedureCancel() {
    this.confirmDialogService.open({
      title: 'Discard order?',
      message: 'If you exit now, the entire dispatch will not be saved. Discard?',
      confirmText: 'Yes, discard',
      cancelText: 'Keep editing'
    }).subscribe(cancelled => {
      if (cancelled) {
        this.router.navigate(['app/dispatch']);
      }
    });

  }
}

