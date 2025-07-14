import { Component } from '@angular/core';
import { AuthService } from '../auth-service';
import { LoginDto } from '../login.dto';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [ FormsModule ],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  loginDto: LoginDto = {
    userName: '',
    password: ''
  };

  error: string | null = null;

  constructor(private authService: AuthService, private router: Router) {}

  submit() {
    this.authService.login(this.loginDto).subscribe({
      next: res => {
        if (res.mustChangePassword) {
          this.router.navigate(['/change-password']);
        } else {
          this.router.navigate(['/app/startup']);
        }
      },
      error: err => {
        this.error = err.status === 401 ? 'Invalid username or password' : 'Login failed';
      }
    });
  }
}
