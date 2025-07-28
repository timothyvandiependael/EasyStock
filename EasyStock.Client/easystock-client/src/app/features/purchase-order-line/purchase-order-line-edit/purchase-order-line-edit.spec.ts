import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PurchaseOrderLineEdit } from './purchase-order-line-edit';

describe('PurchaseOrderLineEdit', () => {
  let component: PurchaseOrderLineEdit;
  let fixture: ComponentFixture<PurchaseOrderLineEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PurchaseOrderLineEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PurchaseOrderLineEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
