import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SupplierEdit } from './supplier-edit';

describe('SupplierEdit', () => {
  let component: SupplierEdit;
  let fixture: ComponentFixture<SupplierEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SupplierEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SupplierEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
