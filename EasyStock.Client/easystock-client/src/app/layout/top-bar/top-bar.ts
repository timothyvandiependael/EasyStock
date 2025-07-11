import { Component, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-top-bar',
  imports: [],
  templateUrl: './top-bar.html',
  styleUrl: './top-bar.css'
})
export class TopBar {
  @Output() toggleNav = new EventEmitter<void>();

  userName = 'John Doe'; // TODO user info
}
