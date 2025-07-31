import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ColumnMetaData } from '../../shared/column-meta-data';
import { AdvancedQueryParametersDto, PaginationResult } from '../../shared/query';
import { StockMovementOverviewDto } from './dtos/stock-movement-overview.dto';
import { CreateStockMovementDto } from './dtos/create-stock-movement.dto';
import { UpdateStockMovementDto } from './dtos/update-stock-movement.dto';
import { StockMovementDetailDto } from './dtos/stock-movement-detail.dto';
import { PageTitleService } from '../../shared/services/page-title-service';

@Injectable({
  providedIn: 'root'
})
export class StockMovementService {
  private readonly apiUrl = environment.stockMovement;
  
    constructor(private http: HttpClient) {}
  
    getColumns(): Observable<ColumnMetaData[]> {
      return this.http.get<ColumnMetaData[]>(this.apiUrl + 'columns');
    }
  
    getAdvanced(params: AdvancedQueryParametersDto): Observable<PaginationResult<StockMovementOverviewDto>> {
      if (params.filters) {
        params.filters = params.filters.map(f => ({
          field: f.field,
          operator: f.operator,
          value: f.value != null ? f.value.toString() : ''
        }));
      }
      return this.http.post<PaginationResult<StockMovementOverviewDto>>(this.apiUrl + 'advanced', params);
    }

    private formatBeforePost(stockMovement: CreateStockMovementDto) {
      if (!stockMovement.quantityChange) stockMovement.quantityChange = 0;
    }
  
    add(dto: CreateStockMovementDto): Observable<StockMovementDetailDto> {
      this.formatBeforePost(dto);
      return this.http.post<StockMovementDetailDto>(this.apiUrl, dto);
    }
  
    getById(id: number) {
      return this.http.get<StockMovementDetailDto>(this.apiUrl + 'id/' + id);
    }
  
    edit(id: number, dto: UpdateStockMovementDto) {
      this.formatBeforePost(dto);
      return this.http.put<void>(this.apiUrl + id, dto);
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
