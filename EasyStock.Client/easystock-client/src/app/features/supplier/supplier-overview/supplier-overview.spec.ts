import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SupplierOverview } from './supplier-overview';

describe('SupplierOverview', () => {
  let component: SupplierOverview;
  let fixture: ComponentFixture<SupplierOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SupplierOverview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SupplierOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
