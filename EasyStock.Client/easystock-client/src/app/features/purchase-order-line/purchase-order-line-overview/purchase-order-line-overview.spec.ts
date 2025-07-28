import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PurchaseOrderLineOverview } from './purchase-order-line-overview';

describe('PurchaseOrderLineOverview', () => {
  let component: PurchaseOrderLineOverview;
  let fixture: ComponentFixture<PurchaseOrderLineOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PurchaseOrderLineOverview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PurchaseOrderLineOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
