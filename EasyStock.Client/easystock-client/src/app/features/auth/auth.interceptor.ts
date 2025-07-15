import { Injectable } from "@angular/core";
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Observable } from "rxjs";
import { AuthService } from "./auth-service";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private authService: AuthService) {}

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        debugger;
        const token = this.authService.getToken();
        if (token) {
            const cloned = req.clone({
                setHeaders: {
                     Authorization: `Bearer ${token}`
                }
            });
            debugger;
            return next.handle(cloned);
        }

        return next.handle(req);
    }
}