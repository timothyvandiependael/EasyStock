import { Component, Input } from '@angular/core';
import { FormArray, FormBuilder, FormGroup } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { UserPermissionOverviewDto } from '../../userpermission/dtos/user-permission-overview.dto';
import { UserPermissionService } from '../../userpermission/user-permission-service';
import { Subscription } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { PersistentSnackbarService } from '../../../shared/services/persistent-snackbar.service';
import { ReactiveFormsModule } from '@angular/forms';
import { FormControl } from '@angular/forms';
import { CreateUserDto } from '../dtos/create-user.dto';
import { UpdateUserDto } from '../dtos/update-user.dto';
import { UserService } from '../user-service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-user-permission',
  imports: [FormsModule, ReactiveFormsModule],
  templateUrl: './user-permission.html',
  styleUrl: './user-permission.css',
  standalone: true
})
export class UserPermission {
  private routeSub?: Subscription;
  private getPermissionsSub?: Subscription;
  private addUserSub?: Subscription;
  private updateUserSub?: Subscription;

  userId?: number = undefined;
  userName?: string = undefined;
  userRole?: string = undefined;
  newMode = false;

  allModules: string[] = [
    'Product', 'StockMovement', 'PurchaseOrder', 'PurchaseOrderLine', 'SalesOrder', 'SalesOrderLine',
    'Reception', 'ReceptionLine', 'Dispatch', 'DispatchLine', 'User', 'Supplier', 'Client', 'Category'
  ];
  existingPermissions: ApplyPermissionDto[] = [];

  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    private userPermissionService: UserPermissionService,
    private userService: UserService,
    private snackbar: MatSnackBar,
    private router: Router,
    private persistentSnackbar: PersistentSnackbarService,
    private route: ActivatedRoute
  ) {
    this.form = this.fb.group({
      permissions: this.fb.array([]),
    });
  }

  ngOnInit() {
    this.loadRouteParams();
  }

  ngOnDestroy() {
    this.routeSub?.unsubscribe();
    this.getPermissionsSub?.unsubscribe();
    this.addUserSub?.unsubscribe();
    this.updateUserSub?.unsubscribe();
  }

  loadRouteParams() {
    this.routeSub = this.route.queryParamMap.subscribe(params => {
      var id = params.get('userId');
      var name = params.get('userName');
      var newMode = params.get('newMode');
      var role = params.get('userRole');

      if (newMode) {
        this.newMode = true;
      }
      else this.newMode = false;

      if (id) {
        this.userId = parseInt(id);
      }
      else this.userId = undefined;

      if (name) {
        this.userName = name;
      }
      else this.userName = undefined;

      if (role) {
        this.userRole = role;
      }
      else this.userRole = undefined;

      this.loadPermissions();
    });
  }

  initializePermissions() {
    this.existingPermissions = [];
    for (var i = 0; i < this.allModules.length; i++) {
      var perm: ApplyPermissionDto = {
        resource: this.allModules[i],
        canAdd: true,
        canView: true,
        canEdit: true,
        canDelete: true
      }
      this.existingPermissions.push(perm);
    }
  }

  loadPermissions() {
    if (this.newMode) {
      this.initializePermissions();
      this.buildForm();
    }
    else {
      if (this.userName) {
        this.getPermissionsSub = this.userPermissionService.getForUser(this.userName).subscribe({
          next: (perms) => {
            this.existingPermissions = perms;
            if (!this.existingPermissions || this.existingPermissions.length == 0) {
              this.initializePermissions();
            }
            this.buildForm();
          },
          error: (err) => {
            if (err.status == 404) {
              this.initializePermissions();
              this.buildForm();
            }
            else {
              this.persistentSnackbar.showError('Error retrieving permissions for user.');
            }
          }
        })
      }
    }
  }

  buildForm() {
    const formArray = this.form.get('permissions') as FormArray;
    formArray.clear();

    this.allModules.forEach((moduleName) => {
      const existing = this.existingPermissions.find(p => p.resource === moduleName);

      const group = new FormGroup({
        resource: new FormControl(moduleName),
        canView: new FormControl({ value: existing?.canView ?? false, disabled: false }),
        canAdd: new FormControl({ value: existing?.canAdd ?? false, disabled: false }),
        canEdit: new FormControl({ value: existing?.canEdit ?? false, disabled: false }),
        canDelete: new FormControl({ value: existing?.canDelete ?? false, disabled: false }),
      })

      formArray.push(group);
    });
  }

  save() {
    const updatedPermissions = this.form.value.permissions;
    if (this.newMode) {
      var dto: CreateUserDto = {
        userName: this.userName ?? '',
        role: this.userRole ?? '',
        permissions: updatedPermissions
      }

      this.addUserSub = this.userService.add(dto).subscribe({
        next: (result) => {
          debugger;
          this.persistentSnackbar.showMessage(`User ${this.userName} and permissions added. Provide temporary password to user (warning: this password is only visible once here): ${result.password}`)
          this.router.navigate(['app/user']);
        },
        error: (err) => {
          this.persistentSnackbar.showError('Error adding user with permissions.');
        }
      })
    }
    else {
      var uDto: UpdateUserDto = {
        id: this.userId ? this.userId : 0,
        userName: this.userName ?? '',
        role: this.userRole ?? '',
        permissions: updatedPermissions
      }

      this.updateUserSub = this.userService.edit(uDto.id, uDto).subscribe({
        next: () => {
          this.snackbar.open(`User ${this.userName} and permissions updated.`, 'Close', {
            duration: 3000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
          this.router.navigate(['app/user']);
        },
        error: (err) => {
          debugger;
          this.persistentSnackbar.showError('Error updating user and permissions.');
        }
      })
    }
  }

  cancel() {
    this.router.navigate(['app/user']);
  }

  get permissionsArray(): FormArray {
    return this.form.get('permissions') as FormArray;
  }
}
