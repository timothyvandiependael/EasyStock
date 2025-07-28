import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesOrderLineEdit } from './sales-order-line-edit';

describe('SalesOrderLineEdit', () => {
  let component: SalesOrderLineEdit;
  let fixture: ComponentFixture<SalesOrderLineEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalesOrderLineEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesOrderLineEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
