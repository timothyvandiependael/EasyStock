import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesOrderLineOverview } from './sales-order-line-overview';

describe('SalesOrderLineOverview', () => {
  let component: SalesOrderLineOverview;
  let fixture: ComponentFixture<SalesOrderLineOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalesOrderLineOverview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesOrderLineOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
