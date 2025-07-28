import { TestBed } from '@angular/core/testing';

import { SalesOrderLineService } from './sales-order-line-service';

describe('SalesOrderLineService', () => {
  let service: SalesOrderLineService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SalesOrderLineService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
