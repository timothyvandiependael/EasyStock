import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReceptionOverview } from './reception-overview';

describe('ReceptionOverview', () => {
  let component: ReceptionOverview;
  let fixture: ComponentFixture<ReceptionOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReceptionOverview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ReceptionOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
