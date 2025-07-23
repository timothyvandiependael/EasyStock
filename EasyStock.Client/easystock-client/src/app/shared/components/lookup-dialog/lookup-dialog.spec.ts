import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LookupDialog } from './lookup-dialog';

describe('LookupDialog', () => {
  let component: LookupDialog;
  let fixture: ComponentFixture<LookupDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LookupDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LookupDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
