import { Component, ViewChild } from '@angular/core';
import { EditView } from '../../../shared/components/edit-view/edit-view';
import { ActivatedRoute } from '@angular/router';
import { ColumnMetaData } from '../../../shared/column-meta-data';
import { UserService } from '../user-service';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { UserDetailDto } from '../dtos/user-detail.dto';
import { CreateUserDto } from '../dtos/create-user.dto';
import { UpdateUserDto } from '../dtos/update-user.dto';
import { MatSnackBar } from '@angular/material/snack-bar';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ConfirmDialogService } from '../../../shared/components/confirm-dialog/confirm-dialog-service';

@Component({
  selector: 'app-user-edit',
  imports: [EditView],
  templateUrl: './user-edit.html',
  styleUrl: './user-edit.css'
})
export class UserEdit {
  private routeSub?: Subscription;
  private getColumnsSub?: Subscription;
  private saveAndAddSub?: Subscription;
  private saveNewExitSub?: Subscription;
  private saveExitSub?: Subscription;
  private getByIdSub?: Subscription;

  detailMode: 'add' | 'edit' = 'add';
  columnMetaData: ColumnMetaData[] = [];
  selectedUser?: UserDetailDto;

  @ViewChild(EditView) detailView!: EditView<UserDetailDto>;


  constructor(
    private userService: UserService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    private persistentSnackbar: PersistentSnackbarService,
    private confirmDialogService: ConfirmDialogService) { }

  ngOnInit() {
    this.loadColumnMeta();
  }

  loadColumnMeta() {
    this.getColumnsSub = this.userService.getColumns().subscribe({
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
        this.getByIdSub = this.userService.getById(id).subscribe({
          next: (dto: UserDetailDto) => {
            this.selectedUser = dto;
          },
          error: (err) => {
            console.error('Error retrieving user with id ' + id + ': ', err);
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

  handleSaveAndAddAnother(user: CreateUserDto) {
    this.saveAndAddSub = this.userService.add(user).subscribe({
      next: (saved: UserDetailDto) => {
        this.selectedUser = undefined;
        this.detailView.clearForm();
        this.snackBar.open(`${saved.userName} saved`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
      },
      error: (err) => {
        console.error('Error saving user ', err);
        this.persistentSnackbar.showError(`Error saving ${user.userName}. If the problem persists, please contact support.`);
      }
    })
  }

  handleSaveNewAndExit(user: CreateUserDto) {
    if (user.role == "Regular") {
      this.router.navigate(['app/user/permission'], {
        queryParams: {
          userName: user.userName,
          userRole: user.role,
          newMode: true
        }
      })
    }
    else {
      this.saveNewExitSub = this.userService.add(user).subscribe({
        next: (pwData) => {
          this.selectedUser = undefined;
          debugger;
          this.persistentSnackbar.showMessage(`User ${user.userName} added. Provide temporary password to user (warning: this password is only visible once here): ${pwData.password}`);
          this.router.navigate(['app/user']);

        },
        error: (err) => {
          if (err.error?.errors?.userName) {
            this.persistentSnackbar.showError(`Username already exists.`);
          }
          else {
            this.persistentSnackbar.showError(`Error saving ${user.userName}. If the problem persists, please contact support.`);
          }

        }
      })
    }


  }

  handleSaveAndExit(user: UpdateUserDto) {
    this.saveExitSub = this.userService.edit(user.id, user).subscribe({
      next: () => {
        this.snackBar.open(`User updated`, 'Close', {
          duration: 3000, // 3 seconds
          horizontalPosition: 'right',
          verticalPosition: 'top',
        });
        this.router.navigate(['app/user']);
      },
      error: (err) => {
        console.error('Error updating user: ', err);
        this.persistentSnackbar.showError(`Error saving ${user.userName}. If the problem persists, please contact support.`);
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
    this.router.navigate(['app/user']);
  }
}
