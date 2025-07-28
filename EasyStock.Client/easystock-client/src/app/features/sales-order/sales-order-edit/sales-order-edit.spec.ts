import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SalesOrderEdit } from './sales-order-edit';

describe('SalesOrderEdit', () => {
  let component: SalesOrderEdit;
  let fixture: ComponentFixture<SalesOrderEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SalesOrderEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SalesOrderEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
