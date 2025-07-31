import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ColumnMetaData } from '../../shared/column-meta-data';
import { AdvancedQueryParametersDto, PaginationResult } from '../../shared/query';
import { SupplierOverviewDto } from './dtos/supplier-overview.dto';
import { CreateSupplierDto } from './dtos/create-supplier.dto';
import { UpdateSupplierDto } from './dtos/update-supplier.dto';
import { SupplierDetailDto } from './dtos/supplier-detail.dto';

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
  private readonly apiUrl = environment.supplier;
  
    constructor(private http: HttpClient) {}
  
    getColumns(): Observable<ColumnMetaData[]> {
      return this.http.get<ColumnMetaData[]>(this.apiUrl + 'columns');
    }
  
    getAdvanced(params: AdvancedQueryParametersDto): Observable<PaginationResult<SupplierOverviewDto>> {
      if (params.filters) {
        params.filters = params.filters.map(f => ({
          field: f.field,
          operator: f.operator,
          value: f.value != null ? f.value.toString() : ''
        }));
      }
      return this.http.post<PaginationResult<SupplierOverviewDto>>(this.apiUrl + 'advanced', params);
    }

    private formatBeforePost(supplier: CreateSupplierDto) {
      if (!supplier.email) supplier.email = undefined;
      if (!supplier.phone) supplier.phone = undefined;
      if (!supplier.website) supplier.website = undefined;
    }
  
    add(dto: CreateSupplierDto): Observable<SupplierDetailDto> {
      this.formatBeforePost(dto);
      return this.http.post<SupplierDetailDto>(this.apiUrl, dto);
    }
  
    getById(id: number) {
      return this.http.get<SupplierDetailDto>(this.apiUrl + 'id/' + id);
    }
  
    edit(id: number, dto: UpdateSupplierDto) {
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

  addProduct(id: number, productId: number) {
    return this.http.post(this.apiUrl + 'addproduct?id=' + id + '&productId=' + productId, null);
  }

  removeProduct(id: number, productId: number) {
    return this.http.post(this.apiUrl + 'removeproduct?id=' + id + '&productId=' + productId, null);
  }
}

