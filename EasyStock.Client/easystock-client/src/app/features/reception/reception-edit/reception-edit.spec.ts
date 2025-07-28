import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReceptionEdit } from './reception-edit';

describe('ReceptionEdit', () => {
  let component: ReceptionEdit;
  let fixture: ComponentFixture<ReceptionEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReceptionEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ReceptionEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
