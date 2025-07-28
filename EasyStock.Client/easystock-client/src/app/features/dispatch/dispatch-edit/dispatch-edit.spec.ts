import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DispatchEdit } from './dispatch-edit';

describe('DispatchEdit', () => {
  let component: DispatchEdit;
  let fixture: ComponentFixture<DispatchEdit>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DispatchEdit]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DispatchEdit);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
