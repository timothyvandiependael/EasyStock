import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { ClientService } from '../client-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { ClientDetailDto } from '../dtos/client-detail.dto';
import { CreateClientDto } from '../dtos/create-client.dto';
import { UpdateClientDto } from '../dtos/update-client.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';

@Component({
  selector: 'app-client-edit',
  imports: [EditView],
  templateUrl: './client-edit.html',
  styleUrl: './client-edit.css'
})
export class ClientEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedClient?: ClientDetailDto;

  @ViewChild(EditView) detailView!: EditView<ClientDetailDto>;


  constructor(
    private clientService: ClientService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService) { }

  ngOnInit() {
    this.loadColumnMeta();
  }

  loadColumnMeta() {
    this.getColumnsSub = this.clientService.getColumns().subscribe({
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
        this.getByIdSub = this.clientService.getById(id).subscribe({
          next: (dto: ClientDetailDto) => {
            this.selectedClient = dto;
          },
          error: (err) => {
            console.error('Error retrieving client with id ' + id + ': ', err);
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

  handleSaveAndAddAnother(client: CreateClientDto) {
    this.saveAndAddSub = this.clientService.add(client).subscribe({
      next: (saved: ClientDetailDto) => {
        this.selectedClient = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`${saved.name} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving client ', err);
        this.persistentSnackbar.showError(`Error saving ${client.name}. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(client: CreateClientDto) {
    this.saveNewExitSub = this.clientService.add(client).subscribe({
      next: (saved: ClientDetailDto) => {
        this.selectedClient = undefined;
        this.snackBar.open(`${saved.name} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/client']);
      },
      error: (err) => {
        console.error('Error saving client ', err);
        this.persistentSnackbar.showError(`Error saving ${client.name}. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveAndExit(client: UpdateClientDto) {
    this.saveExitSub = this.clientService.edit(client.id, client).subscribe({
      next: () => {
        this.snackBar.open(`${client.name} updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/client']);
      },
      error: (err) => {
        console.error('Error updating client: ', err);
        this.persistentSnackbar.showError(`Error saving ${client.name}. If the problem persists, please contact support.`);
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
    this.router.navigate(['app/client']);
  }
}

