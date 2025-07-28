import { TestBed } from '@angular/core/testing';

import { PurchaseOrderLineService } from './purchase-order-line-service';

describe('PurchaseOrderLineService', () => {
  let service: PurchaseOrderLineService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PurchaseOrderLineService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
