import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-change-password',
  imports: [ FormsModule ],
  templateUrl: './change-password.html',
  styleUrl: './change-password.css'
})
export class ChangePassword {
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';

  onSubmit() {
    // TODO: password logic
  }
}
