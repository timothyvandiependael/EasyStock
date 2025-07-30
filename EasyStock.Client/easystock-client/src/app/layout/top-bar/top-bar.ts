import { Component, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../../features/auth/auth-service';
import { Router } from '@angular/router';
import { ConfirmDialogService } from '../../shared/components/confirm-dialog/confirm-dialog-service';

@Component({
  selector: 'app-top-bar',
  imports: [],
  templateUrl: './top-bar.html',
  styleUrl: './top-bar.css'
})
export class TopBar {
  @Output() toggleNav = new EventEmitter<void>();

  userName = '';

  constructor(private authService: AuthService, private router: Router, private confirmDialogService: ConfirmDialogService) {
    var usr = authService.getUserName();
    if (usr != null) {
      this.userName = usr;
    }
  }

  logout() {

    this.confirmDialogService.open({
      title: 'Log out?',
      message: 'Do you want to log out?',
      confirmText: 'Yes',
      cancelText: 'No'
    }).subscribe(logout => {
      if (logout) this.authService.logout();
    });
  }


}
