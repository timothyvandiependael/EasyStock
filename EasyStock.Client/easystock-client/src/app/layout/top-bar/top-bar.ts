import { Component, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../../features/auth/auth-service';

@Component({
  selector: 'app-top-bar',
  imports: [],
  templateUrl: './top-bar.html',
  styleUrl: './top-bar.css'
})
export class TopBar {
  @Output() toggleNav = new EventEmitter<void>();

  userName = '';

  constructor(private authService: AuthService) {
    var usr = authService.getUserName();
    if (usr != null) {
      this.userName = usr;
    }
  }

  
}
