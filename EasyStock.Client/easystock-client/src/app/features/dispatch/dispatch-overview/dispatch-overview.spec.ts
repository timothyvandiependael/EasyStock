import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DispatchOverview } from './dispatch-overview';

describe('DispatchOverview', () => {
  let component: DispatchOverview;
  let fixture: ComponentFixture<DispatchOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DispatchOverview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DispatchOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
