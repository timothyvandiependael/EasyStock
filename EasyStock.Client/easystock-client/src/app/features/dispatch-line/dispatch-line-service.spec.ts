import { TestBed } from '@angular/core/testing';

import { DispatchLineService } from './dispatch-line-service';

describe('DispatchLineService', () => {
  let service: DispatchLineService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DispatchLineService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
