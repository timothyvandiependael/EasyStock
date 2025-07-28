import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PurchaseOrderOverview } from './purchase-order-overview';

describe('PurchaseOrderOverview', () => {
  let component: PurchaseOrderOverview;
  let fixture: ComponentFixture<PurchaseOrderOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PurchaseOrderOverview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PurchaseOrderOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
