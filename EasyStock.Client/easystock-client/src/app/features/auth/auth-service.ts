import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginDto } from './login.dto';
import { jwtDecode } from 'jwt-decode';
import { ChangePasswordDto } from './change-password.dto';
import { UserPermissionService } from '../userpermission/user-permission-service';

interface AuthResult {
  token: string;
  mustChangePassword: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly tokenKey = "authToken";
  private readonly apiUrl = environment.auth;
  private permissions: ApplyPermissionDto[] = [];

  constructor(private http: HttpClient, private userPermissionService: UserPermissionService) { }

  login(dto: LoginDto): Observable<AuthResult> {
    return this.http.post<AuthResult>(this.apiUrl + 'login', dto)
      .pipe(
        tap(res => {
          if (res.token) this.setToken(res.token);
        }),
        catchError(err => {
          if (err.status === 401) {
            console.warn('Invalid username or password');
          } else {
            console.error('Login error: ', err);
          }
          return throwError(() => err);
        }))
  }

  changePassword(dto: ChangePasswordDto): Observable<any> {
    return this.http.post(this.apiUrl + 'changepassword', dto);
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const decoded: any = jwtDecode(token);
      const currentTime = Math.floor(Date.now() / 1000);
      return decoded.exp > currentTime;
    } catch (e) {
      return false;
    }
  }

  getUserName(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const decoded: any = jwtDecode(token);

      return decoded.sub || null;
    } catch (e) {
      return null;
    }
  }

  getUserRole(): string | null {
    const token = this.getToken();
    debugger;
    if (!token) return null;

    try {
      const decoded: any = jwtDecode(token);

      return decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
    } catch (e) {
      return null;
    }
  }

  setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  clearToken(): void {
    localStorage.removeItem(this.tokenKey);
  }

  determinePermissionsForUser() {
    var userName = this.getUserName() ?? "";
    var role = (this.getUserRole() ?? "Regular").toLowerCase();

    if (role != "admin") {
      this.userPermissionService.getForUser(userName).subscribe({
        next: (permissions) => {
          this.permissions = permissions;
        },
        error: (err) => {
          console.error(err);
        }
      });
    }
  }

  canAdd(resource: string) {
    var role = (this.getUserRole() ?? "Regular").toLowerCase();
    if (role == "admin") return true;
    var perm = this.permissions.find(p => p.resource == resource);
    if (perm == null) return false;
    else return perm.canAdd;
  }

  canView(resource: string) {
    var role = (this.getUserRole() ?? "Regular").toLowerCase();
    if (role == "admin") return true;
    var perm = this.permissions.find(p => p.resource == resource);
    if (perm == null) return false;
    else return perm.canView;
  }

  canEdit(resource: string) {
    var role = (this.getUserRole() ?? "Regular").toLowerCase();
    if (role == "admin") return true;
    var perm = this.permissions.find(p => p.resource == resource);
    if (perm == null) return false;
    else return perm.canEdit;
  }

  canDelete(resource: string) {
    var role = (this.getUserRole() ?? "Regular").toLowerCase();
    if (role == "admin") return true;
    var perm = this.permissions.find(p => p.resource == resource);
    if (perm == null) return false;
    else return perm.canDelete;
  }
}
