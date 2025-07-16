import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { tap, catchError, Observable } from 'rxjs';
import { ColumnMetaData } from '../../shared/column-meta-data';
import { AdvancedQueryParametersDto, PaginationResult } from '../../shared/query';
import { PurchaseOrderOverviewDto } from './purchase-order-overview.dto';

@Injectable({
  providedIn: 'root'
})
export class PurchaseOrderService {
  private readonly apiUrl = environment.auth;

  constructor(private http: HttpClient) {}

  getColumns(): Observable<ColumnMetaData[]> {
    return this.http.get<ColumnMetaData[]>(this.apiUrl + 'columns');
  }

  getAdvanced(params: AdvancedQueryParametersDto): Observable<PaginationResult<PurchaseOrderOverviewDto>> {
    return this.http.post<PaginationResult<PurchaseOrderOverviewDto>>(this.apiUrl + 'advanced', params);
  }
}
