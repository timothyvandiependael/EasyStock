import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DispatchLineOverview } from './dispatch-line-overview';

describe('DispatchLineOverview', () => {
  let component: DispatchLineOverview;
  let fixture: ComponentFixture<DispatchLineOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DispatchLineOverview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DispatchLineOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
