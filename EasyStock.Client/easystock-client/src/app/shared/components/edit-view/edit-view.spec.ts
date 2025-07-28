import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DetailView } from './detail-view';

describe('DetailView', () => {
  let component: DetailView<any>;
  let fixture: ComponentFixture<DetailView<any>>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DetailView]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DetailView);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
