import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReceptionLineOverview } from './reception-line-overview';

describe('ReceptionLineOverview', () => {
  let component: ReceptionLineOverview;
  let fixture: ComponentFixture<ReceptionLineOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReceptionLineOverview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ReceptionLineOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
