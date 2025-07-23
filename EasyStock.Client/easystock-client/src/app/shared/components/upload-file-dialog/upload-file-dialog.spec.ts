import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UploadFileDialog } from './upload-file-dialog';

describe('UploadFileDialog', () => {
  let component: UploadFileDialog;
  let fixture: ComponentFixture<UploadFileDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UploadFileDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UploadFileDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
