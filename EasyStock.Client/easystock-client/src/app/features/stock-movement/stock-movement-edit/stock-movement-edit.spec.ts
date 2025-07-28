import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StockMovementEdit } from './stock-movement-edit';

describe('StockMovementEdit', () => {
  let component: StockMovementEdit;
  let fixture: ComponentFixture<StockMovementEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StockMovementEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StockMovementEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
