import { TestBed } from '@angular/core/testing';

import { ReceptionLineService } from './reception-line-service';

describe('ReceptionLineService', () => {
  let service: ReceptionLineService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ReceptionLineService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
