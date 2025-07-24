import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ColumnMetaData } from '../../shared/column-meta-data';
import { AdvancedQueryParametersDto, PaginationResult } from '../../shared/query';
import { CategoryOverviewDto } from './category-overview.dto';
import { CreateCategoryDto } from './create-category.dto';
import { UpdateCategoryDto } from './update-category.dto';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private readonly apiUrl = environment.category;

  constructor(private http: HttpClient) { }

  getColumns(): Observable<ColumnMetaData[]> {
    return this.http.get<ColumnMetaData[]>(this.apiUrl + 'columns');
  }

  getAdvanced(params: AdvancedQueryParametersDto): Observable<PaginationResult<CategoryOverviewDto>> {
    if (params.filters) {
      params.filters = params.filters.map(f => ({
        field: f.field,
        operator: f.operator,
        value: f.value != null ? f.value.toString() : ''
      }));
    }
    return this.http.post<PaginationResult<CategoryOverviewDto>>(this.apiUrl + 'advanced', params);
  }

  add(dto: CreateCategoryDto): Observable<CategoryOverviewDto> {
    return this.http.post<CategoryOverviewDto>(this.apiUrl, dto);
  }

  getById(id: number) {
    return this.http.get<CategoryOverviewDto>(this.apiUrl + 'id/' + id);
  }

  edit(id: number, dto: UpdateCategoryDto) {
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
      observe: 'response' // <-- this lets us see headers
    }).subscribe({
      next: (response) => {
        debugger;
        const blob = response.body as Blob;

        // Try to get filename from headers
        let filename = 'export';
        const contentDisposition = response.headers.get('content-disposition');
        if (contentDisposition) {
          const matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(contentDisposition);
          if (matches != null && matches[1]) {
            filename = matches[1].replace(/['"]/g, ''); // strip quotes
          }
        }

        // Trigger download
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
