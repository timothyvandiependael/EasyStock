import { Component, EventEmitter, Input, Output, ViewChild, ElementRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../features/auth/auth-service';

@Component({
  selector: 'app-left-nav',
  imports: [RouterModule, CommonModule],
  standalone: true,
  templateUrl: './left-nav.html',
  styleUrl: './left-nav.css'
})
export class LeftNav {
  constructor(private authService: AuthService) { }

  @Input() collapsed = false;
  isTransitioning = false;
  contentVisible = true;

  @Output() toggleNav = new EventEmitter<void>();

  @ViewChild('navElement') navElement!: ElementRef<HTMLElement>;

  allNavItems = [
    // Main section
    { name: 'Home', section: 'Main', label: 'Home', route: '/app/startup', icon: '🏠' },
    { name: 'Product', section: 'Main', label: 'Products', route: '/app/product', icon: '📦' },
    { name: 'StockMovement', section: 'Main', label: 'Stock Movements', route: '/app/stockmovement', icon: '🔄' },
    { name: 'PurchaseOrder', section: 'Main', label: 'Purchases', route: '/app/purchaseorder', icon: '🛒' },
    { name: 'PurchaseOrderLine', section: 'Main', label: 'Purchase Lines', route: '/app/purchaseorderline', icon: '🧾' },
    { name: 'SalesOrder', section: 'Main', label: 'Sales', route: '/app/salesorder', icon: '💰' },
    { name: 'SalesOrderLine', section: 'Main', label: 'Sales Lines', route: '/app/salesorderline', icon: '🧾' },
    { name: 'Reception', section: 'Main', label: 'Reception', route: '/app/reception', icon: '🚚	' },
    { name: 'ReceptionLine', section: 'Main', label: 'Reception Lines', route: '/app/receptionline', icon: '🧾' },
    { name: 'Dispatch', section: 'Main', label: 'Dispatch', route: '/app/dispatch', icon: '📤' },
    { name: 'DispatchLine', section: 'Main', label: 'Dispatch Lines', route: '/app/dispatchline', icon: '🧾' },

    // Admin section
    { name: 'User', section: 'Admin', label: 'Users', route: '/app/user', icon: '👤' },
    { name: 'Supplier', section: 'Admin', label: 'Suppliers', route: '/app/supplier', icon: '🏭' },
    { name: 'Client', section: 'Admin', label: 'Clients', route: '/app/client', icon: '👥' },
    { name: 'Category', section: 'Admin', label: 'Categories', route: '/app/category', icon: '📂' },
  ];

  navItems: any[] = [];

  private transitionEndHandler = (event: TransitionEvent) => {
    if (event.propertyName === 'width' && event.target === this.navElement.nativeElement) {
      this.isTransitioning = false;
    }
  }

  onTransitionEnd(event: TransitionEvent) {
    if (event.propertyName === 'width') {
      // When width transition ends, stop transitioning state
      this.isTransitioning = false;
      this.contentVisible = true;
    }
  }

  onToggleClick() {
    this.toggleNav.emit();

    this.contentVisible = false;
    this.isTransitioning = true;  // start hiding content immediately
    this.collapsed = !this.collapsed;

    if (this.collapsed) {
      // When collapsing, content stays hidden (still transitioning)
      // Once transition ends, isTransitioning = false will be set via handler
    }
  }

  ngOnInit() {
    this.navItems = this.allNavItems.filter(item => this.authService.canView(item.name));
  }

  ngAfterViewInit() {
    this.navElement.nativeElement.addEventListener('transitionend', this.transitionEndHandler);
  }

  ngOnDestroy() {
    this.navElement.nativeElement.removeEventListener('transitionend', this.transitionEndHandler);
  }

  getSections(): string[] {
    return Array.from(new Set(this.navItems.map(item => item.section)));
  }

  getItemsBySection(section: string) {
    return this.navItems.filter(item => item.section === section);
  }
}
