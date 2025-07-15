import { Component, EventEmitter, Input, Output, ViewChild, ElementRef } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-left-nav',
  imports: [RouterModule, CommonModule],
  standalone: true,
  templateUrl: './left-nav.html',
  styleUrl: './left-nav.css'
})
export class LeftNav {
  @Input() collapsed = false;
  isTransitioning = false;
  contentVisible = true;

  @Output() toggleNav = new EventEmitter<void>();

  @ViewChild('navElement') navElement!: ElementRef<HTMLElement>;

  navItems = [
    // Main section
    { section: 'Main', label: 'Home', route: '/app/startup', icon: '🏠' },
    { section: 'Main', label: 'Stock', route: '/app/stock', icon: '📦' },
    { section: 'Main', label: 'Stock Movements', route: '/app/stock-movements', icon: '🔄' },
    { section: 'Main', label: 'Purchases', route: '/app/purchase-orders', icon: '🛒' },
    { section: 'Main', label: 'Sales', route: '/app/sales-orders', icon: '💰' },
    { section: 'Main', label: 'Reception', route: '/app/receptions', icon: '📥' },
    { section: 'Main', label: 'Dispatch', route: '/app/dispatches', icon: '📤' },

    // Admin section
    { section: 'Admin', label: 'Users', route: '/app/users', icon: '👤' },
    { section: 'Admin', label: 'Suppliers', route: '/app/suppliers', icon: '🏭' },
    { section: 'Admin', label: 'Clients', route: '/app/clients', icon: '👥' },
    { section: 'Admin', label: 'Categories', route: '/app/categories', icon: '📂' },
  ];

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
