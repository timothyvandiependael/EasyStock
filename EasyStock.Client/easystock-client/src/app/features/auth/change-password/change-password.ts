import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../auth-service';
import { ChangePasswordDto } from '../change-password.dto';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-change-password',
  imports: [FormsModule],
  templateUrl: './change-password.html',
  styleUrl: './change-password.css'
})
export class ChangePassword {
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  mustChangePassword = false;

  passwordComplexityError = false;
  submissionErrorMessages: string[] = [];
  successMessage = '';

  private queryParamSub?: Subscription;
  private changePasswordSub?: Subscription;

  constructor(private route: ActivatedRoute, private authService: AuthService, private router: Router, private snackBar: MatSnackBar) {
    this.queryParamSub = this.route.queryParams.subscribe(params => {
      this.mustChangePassword = params['mustChangePassword'] === 'true';
    });
  }

  ngOnDestroy() {
    this.queryParamSub?.unsubscribe();
    this.changePasswordSub?.unsubscribe();
  }

  onNewPasswordChange(value: string) {
    this.passwordComplexityError = !this.validatePasswordComplexity(value);
  }

  validatePasswordComplexity(password: string): boolean {
    const pattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$/;
    return pattern.test(password);
  }

  onSubmit() {
    this.submissionErrorMessages = [];
    this.successMessage = '';
    this.passwordComplexityError = false;

    if (!this.mustChangePassword && !this.currentPassword) {
      return;
    }

    if (!this.newPassword || this.newPassword.length < 8) {
      return;
    }

    if (this.newPassword.length > 40) {
    }

    if (this.newPassword !== this.confirmPassword) {
      return;
    }

    if (!this.validatePasswordComplexity(this.newPassword)) {
      this.passwordComplexityError = true;
      return;
    }

    var userName = this.authService.getUserName();
    if (userName == null) return;

    const dto: ChangePasswordDto = {
      userName: userName,
      newPassword: this.newPassword,
      oldPassword: this.mustChangePassword ? undefined : this.currentPassword
    };

    this.changePasswordSub = this.authService.changePassword(dto).subscribe({
      next: () => {
        this.successMessage = 'Password successfully changed.';

        this.snackBar.open('Password successfully changed.', 'OK', { duration: 2000, horizontalPosition: 'center', verticalPosition: 'top' });
        this.router.navigate(['/app/startup']);
      },
      error: (err) => {
        if (err.status === 400 && err.error.errors) {
          const errors = err.error.errors;
          for (const key in errors) {
            if (errors.hasOwnProperty(key)) {
              this.submissionErrorMessages.push(...errors[key]);
            }
          }
        } else {
          this.submissionErrorMessages.push('An unexpected error occured.');
        }
      }
    })
  }




}
