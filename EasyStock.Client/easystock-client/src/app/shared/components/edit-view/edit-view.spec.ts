import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditView } from './edit-view';

describe('DetailView', () => {
  let component: EditView<any>;
  let fixture: ComponentFixture<EditView<any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
