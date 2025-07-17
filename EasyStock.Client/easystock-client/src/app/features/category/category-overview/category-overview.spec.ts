import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CategoryOverview } from './category-overview';

describe('Category', () => {
  let component: CategoryOverview;
  let fixture: ComponentFixture<CategoryOverview>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryOverview]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CategoryOverview);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
