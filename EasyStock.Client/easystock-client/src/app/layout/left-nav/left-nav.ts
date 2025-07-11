import { Component, Input } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-left-nav',
  imports: [ RouterModule ],
  templateUrl: './left-nav.html',
  styleUrl: './left-nav.css'
})
export class LeftNav {
  @Input() collapsed = false;

  navItems = [
    { label: 'Dashboard', route: '/app/startup', icon: '🏠' },
    { label: 'Products', route: '/app/products', icon: '📦' },
    { label: 'Customers', route: '/app/customers', icon: '👥' },
    // add more modules here
  ];
}
