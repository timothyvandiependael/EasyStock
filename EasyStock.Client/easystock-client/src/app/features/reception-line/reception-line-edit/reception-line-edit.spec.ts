import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReceptionLineEdit } from './reception-line-edit';

describe('ReceptionLineEdit', () => {
  let component: ReceptionLineEdit;
  let fixture: ComponentFixture<ReceptionLineEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReceptionLineEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ReceptionLineEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
