import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ColumnMetaData } from '../../shared/column-meta-data';
import { AdvancedQueryParametersDto, PaginationResult } from '../../shared/query';
import { UserPermissionOverviewDto } from './dtos/user-permission-overview.dto';
import { CreateUserPermissionDto } from './dtos/create-user-permission.dto';
import { UpdateUserPermissionDto } from './dtos/update-user-permission.dto';
import { UserPermissionDetailDto } from './dtos/user-permission-detail.dto';

@Injectable({
  providedIn: 'root'
})
export class UserPermissionService {
  private readonly apiUrl = environment.userPermission;
  private permissions: ApplyPermissionDto[] = [];

  constructor(private http: HttpClient) {}

  getForUser(name: string) {
    return this.http.get<ApplyPermissionDto[]>(this.apiUrl + 'getforuser/' + name);
  }

   getColumns(): Observable<ColumnMetaData[]> {
      return this.http.get<ColumnMetaData[]>(this.apiUrl + 'columns');
    }
  
    getAdvanced(params: AdvancedQueryParametersDto): Observable<PaginationResult<UserPermissionOverviewDto>> {
      if (params.filters) {
        params.filters = params.filters.map(f => ({
          field: f.field,
          operator: f.operator,
          value: f.value != null ? f.value.toString() : ''
        }));
      }
      return this.http.post<PaginationResult<UserPermissionOverviewDto>>(this.apiUrl + 'advanced', params);
    }
  
    private formatBeforePost(userPermission: CreateUserPermissionDto) {
      if (!userPermission.canView) userPermission.canView = false;
      if (!userPermission.canAdd) userPermission.canAdd = false;
      if (!userPermission.canEdit) userPermission.canEdit = false;
      if (!userPermission.canDelete) userPermission.canDelete = false;
    }
  
    add(dto: CreateUserPermissionDto): Observable<UserPermissionDetailDto> {
      this.formatBeforePost(dto);
      return this.http.post<UserPermissionDetailDto>(this.apiUrl, dto);
    }
  
    getById(id: number) {
      return this.http.get<UserPermissionDetailDto>(this.apiUrl + 'id/' + id);
    }
  
    edit(id: number, dto: UpdateUserPermissionDto) {
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
