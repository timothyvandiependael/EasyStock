import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserPermissionService {
  private readonly apiUrl = environment.userPermission;
  private permissions: ApplyPermissionDto[] = [];

  constructor(private http: HttpClient) {}

  getForUser(name: string) {
    return this.http.get<ApplyPermissionDto[]>(this.apiUrl + 'getforuser?name=' + name);
  }
}
