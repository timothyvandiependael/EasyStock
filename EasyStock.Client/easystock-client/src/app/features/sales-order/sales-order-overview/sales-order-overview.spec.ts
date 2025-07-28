import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesOrderOverview } from './sales-order-overview';

describe('SalesOrderOverview', () => {
  let component: SalesOrderOverview;
  let fixture: ComponentFixture<SalesOrderOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalesOrderOverview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesOrderOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
