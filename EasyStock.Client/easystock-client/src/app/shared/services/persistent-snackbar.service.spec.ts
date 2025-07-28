import { TestBed } from '@angular/core/testing';

import { PersistentSnackbarService } from './persistent-snackbar.service';

describe('PersistentSnackbarService', () => {
  let service: PersistentSnackbarService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PersistentSnackbarService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
