import { Component } from '@angular/core';
import { LeftNav } from '../left-nav/left-nav';
import { TopBar } from '../top-bar/top-bar';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-app-layout',
  imports: [LeftNav, TopBar, RouterModule],
  standalone: true,
  templateUrl: './app-layout.html',
  styleUrl: './app-layout.css'
})
export class AppLayout {
  isNavCollapsed = false;

  toggleNav() {
    this.isNavCollapsed = !this.isNavCollapsed;
  }
}
