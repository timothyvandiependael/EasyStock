import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ColumnMetaData } from '../../shared/column-meta-data';
import { AdvancedQueryParametersDto, PaginationResult } from '../../shared/query';
import { SalesOrderLineOverviewDto } from './dtos/sales-order-line-overview.dto';
import { CreateSalesOrderLineDto } from './dtos/create-sales-order-line.dto';
import { UpdateSalesOrderLineDto } from './dtos/update-sales-order-line.dto';
import { SalesOrderLineDetailDto } from './dtos/sales-order-line-detail.dto';
import { AutoRestockDto } from '../../shared/autorestock.dto';

@Injectable({
  providedIn: 'root'
})
export class SalesOrderLineService {
  private readonly apiUrl = environment.salesOrderLine;
  
    constructor(private http: HttpClient) {}
  
    getColumns(): Observable<ColumnMetaData[]> {
      return this.http.get<ColumnMetaData[]>(this.apiUrl + 'columns');
    }
  
    getAdvanced(params: AdvancedQueryParametersDto): Observable<PaginationResult<SalesOrderLineOverviewDto>> {
      if (params.filters) {
        params.filters = params.filters.map(f => ({
          field: f.field,
          operator: f.operator,
          value: f.value != null ? f.value.toString() : ''
        }));
      }
      return this.http.post<PaginationResult<SalesOrderLineOverviewDto>>(this.apiUrl + 'advanced', params);
    }

    private formatBeforePost(salesOrderLine: CreateSalesOrderLineDto) {
      if (!salesOrderLine.quantity) salesOrderLine.quantity = 0;
      if (!salesOrderLine.unitPrice) salesOrderLine.unitPrice = 0;
    }
  
    add(dto: CreateSalesOrderLineDto): Observable<AutoRestockDto> {
      this.formatBeforePost(dto);
      return this.http.post<AutoRestockDto>(this.apiUrl, dto);
    }
  
    getById(id: number) {
      return this.http.get<SalesOrderLineDetailDto>(this.apiUrl + 'id/' + id);
    }
  
    edit(id: number, dto: UpdateSalesOrderLineDto): Observable<AutoRestockDto> {
      this.formatBeforePost(dto);
      return this.http.put<AutoRestockDto>(this.apiUrl + id, dto);
    }
  
    block(id: number) {
      return this.http.post(this.apiUrl + 'block?id=' + id, null);
    }
  
    unblock(id: number) {
      return this.http.post(this.apiUrl + 'unblock?id=' + id, null);
    }

    export(params: AdvancedQueryParametersDto, format: 'csv' | 'excel') {
    if (params.filters) {
      params.filters = params.filters.map(f => ({
        field: f.field,
        operator: f.operator,
        value: f.value != null ? f.value.toString() : ''
      }));
    }

    const exportRequest = {
      parameters: params,
      format: format
    };

    this.http.post(this.apiUrl + 'export', exportRequest, {
      responseType: 'blob',
      observe: 'response' 
    }).subscribe({
      next: (response) => {
        debugger;
        const blob = response.body as Blob;

        let filename = 'export';
        const contentDisposition = response.headers.get('content-disposition');
        if (contentDisposition) {
          const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(contentDisposition);
          if (matches != null && matches[1]) {
            filename = matches[1].replace(/['"]/g, ''); // strip quotes
          }
        }

        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = filename;
        link.click();

        window.URL.revokeObjectURL(link.href);
      },
      error: (err) => {
        console.error('Export failed', err);
      }
    });
  }
}
