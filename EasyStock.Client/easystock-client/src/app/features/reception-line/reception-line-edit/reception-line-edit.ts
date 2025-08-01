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
import { ReceptionService } from '../../reception/reception-service';
import { StorageService } from '../../../shared/storage/storage-service';
import { ReceptionDetailDto } from '../../reception/dtos/reception-detail.dto';
import { CreateReceptionDto } from '../../reception/dtos/create-reception.dto';
import { FilterCondition } from '../../../shared/query';

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
  private receptionSaveSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedReceptionLine?: ReceptionLineDetailDto;

  procedureStep2 = false;

  parentId?: number = undefined;
  parentNumber?: string = undefined;

  filledInFields: any = {};

  additionalFilters: any;

  addModeHideFields = [
    'lineNumber', 'status'
  ]

  @ViewChild(EditView) detailView!: EditView<ReceptionLineDetailDto>;


  constructor(
    private receptionLineService: ReceptionLineService,
    private receptionService: ReceptionService,
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

      if (mode == 'add') {
        const fromParent = params.get('id');
        if (fromParent) {
          this.procedureStep2 = true;
          this.addModeHideFields.push('receptionNumber');
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
              receptionNumber: parentNumber
            }
          }
        });
      }

      if (mode === 'edit') {
        const id = Number(params.get('id'));
        this.getByIdSub = this.receptionLineService.getById(id).subscribe({
          next: (dto: ReceptionLineDetailDto) => {
            this.selectedReceptionLine = dto;
            this.addAdditionalFilters();
          },
          error: (err) => {
            console.error('Error retrieving receptionLine with id ' + id + ': ', err);
            this.persistentSnackbar.showError(`Error retrieving record with id ${id}. If the problem persists, please contact support.`);
          }
        })

      }
    });
  }

  addAdditionalFilters() {
    var reception = undefined;
    if (this.detailMode == 'add') {
      if (this.procedureStep2) { // new order
        reception = this.storage.retrieve("Reception");
        this.addFilters(reception);
      }
      else { // from parent order
        if (this.parentId) {
          this.receptionService.getById(this.parentId).subscribe({
            next: (dto: ReceptionDetailDto) => {
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
      reception = this.selectedReceptionLine?.reception;
      this.addFilters(reception);
    }
  }

  addFilters(reception: any) {
    var purchaseOrderLineFc: FilterCondition[] = [
      {
        field: 'SupplierId',
        operator: 'equals',
        value: reception.supplierId
      }
    ];

    if (!this.additionalFilters) {
      this.additionalFilters = {};
    }

    this.additionalFilters.purchaseorderline = purchaseOrderLineFc;
  }

  ngOnDestroy() {
    this.routeSub?.unsubscribe();
    this.getColumnsSub?.unsubscribe();
    this.saveAndAddSub?.unsubscribe();
    this.saveNewExitSub?.unsubscribe();
    this.saveExitSub?.unsubscribe();
    this.receptionSaveSub?.unsubscribe();
  }

  handleSaveAndAddAnother(receptionLine: CreateReceptionLineDto) {
    if (this.parentId) {
      receptionLine.receptionId = this.parentId;
    }
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
    if (this.parentId) {
      receptionLine.receptionId = this.parentId;
    }
    this.saveNewExitSub = this.receptionLineService.add(receptionLine).subscribe({
      next: (saved: ReceptionLineDetailDto) => {
        this.selectedReceptionLine = undefined;
        this.snackBar.open(`Reception line saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });

        if (this.parentId) {
          this.router.navigate(['app/receptionline'], {
            queryParams: {
              parentId: this.parentId
            }
          });
        }
        else {
          this.router.navigate(['app/receptionline']);
        }
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

  handleAddMoreLines(line: CreateReceptionLineDto) {
    this.addLineToOrder(line);
    this.selectedReceptionLine = undefined;
    this.detailView.clearForm();
  }

  handleSaveAllAndExit(line: CreateReceptionLineDto) {
    this.addLineToOrder(line);
    this.saveOrder();

  }

  saveOrder() {
    var re = this.getOrder();

    this.receptionSaveSub = this.receptionService.add(re).subscribe({
      next: (saved: ReceptionDetailDto) => {
        this.selectedReceptionLine = undefined;
        this.snackBar.open(`Reception saved`, 'Close', {
          duration: 3000,
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/reception']);
      },
      error: (err) => {
        this.persistentSnackbar.showError(`Error saving reception. If the problem persists, please contact support.`);
        this.removeLastLineAfterError();
      }
    })
  }

  removeLastLineAfterError() {
    var re = this.getOrder();
    if (re.lines.length > 0) {
      re.lines.pop();
    }
    this.storage.store('Reception', re);
  }

  addLineToOrder(line: CreateReceptionLineDto) {
    var re = this.getOrder();
    re.lines.push(line);
    this.storage.store('Reception', re);
  }

  getOrder() {
    var re = this.storage.retrieve('Reception') as CreateReceptionDto;
    if (!re) {
      this.persistentSnackbar.showError(`Error retrieving reception. If the problem persist, contact support.`);
    }
    if (re.lines == null) re.lines = [];
    return re;
  }

  handleProcedureCancel() {
    this.confirmDialogService.open({
      title: 'Discard reception?',
      message: 'If you exit now, the entire reception will not be saved. Discard?',
      confirmText: 'Yes, discard',
      cancelText: 'Keep editing'
    }).subscribe(cancelled => {
      if (cancelled) {
        this.router.navigate(['app/reception']);
      }
    });

  }
}

