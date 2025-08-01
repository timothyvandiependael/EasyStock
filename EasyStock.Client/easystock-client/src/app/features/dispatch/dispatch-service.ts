import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ColumnMetaData } from '../../shared/column-meta-data';
import { AdvancedQueryParametersDto, PaginationResult } from '../../shared/query';
import { DispatchOverviewDto } from './dtos/dispatch-overview.dto';
import { CreateDispatchDto } from './dtos/create-dispatch.dto';
import { UpdateDispatchDto } from './dtos/update-dispatch.dto';
import { DispatchDetailDto } from './dtos/dispatch-detail.dto';
import { CreateDispatchFromSalesOrderDto } from './dtos/create-dispatch-from-salesorder.dto';

@Injectable({
  providedIn: 'root'
})
export class DispatchService {
  private readonly apiUrl = environment.dispatch;
  
    constructor(private http: HttpClient) {}
  
    getColumns(): Observable<ColumnMetaData[]> {
      return this.http.get<ColumnMetaData[]>(this.apiUrl + 'columns');
    }
  
    getAdvanced(params: AdvancedQueryParametersDto): Observable<PaginationResult<DispatchOverviewDto>> {
      if (params.filters) {
        params.filters = params.filters.map(f => ({
          field: f.field,
          operator: f.operator,
          value: f.value != null ? f.value.toString() : ''
        }));
      }
      return this.http.post<PaginationResult<DispatchOverviewDto>>(this.apiUrl + 'advanced', params);
    }

    private formatBeforePost(dispatch: CreateDispatchDto) {

    }
  
    add(dto: CreateDispatchDto): Observable<DispatchDetailDto> {
      this.formatBeforePost(dto);
      return this.http.post<DispatchDetailDto>(this.apiUrl, dto);
    }

    addFromSalesOrder(dto: CreateDispatchFromSalesOrderDto):Observable<DispatchDetailDto> {
      return this.http.post<DispatchDetailDto>(this.apiUrl + 'fromsalesorder', dto);
    }
  
    getById(id: number) {
      return this.http.get<DispatchDetailDto>(this.apiUrl + 'id/' + id);
    }
  
    edit(id: number, dto: UpdateDispatchDto) {
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

