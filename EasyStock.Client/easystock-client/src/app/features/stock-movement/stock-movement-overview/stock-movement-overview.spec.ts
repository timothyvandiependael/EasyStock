import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StockMovementOverview } from './stock-movement-overview';

describe('StockMovementOverview', () => {
  let component: StockMovementOverview;
  let fixture: ComponentFixture<StockMovementOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StockMovementOverview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StockMovementOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
