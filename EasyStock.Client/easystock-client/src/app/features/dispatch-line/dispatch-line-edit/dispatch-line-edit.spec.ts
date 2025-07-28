import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DispatchLineEdit } from './dispatch-line-edit';

describe('DispatchLineEdit', () => {
  let component: DispatchLineEdit;
  let fixture: ComponentFixture<DispatchLineEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DispatchLineEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DispatchLineEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
