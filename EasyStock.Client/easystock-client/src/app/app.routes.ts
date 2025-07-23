import { Routes } from '@angular/router';
import { AuthGuard } from './features/auth/auth.guard';
import { Login } from './/features/auth/login/login';
import { ChangePassword } from './features/auth/change-password/change-password';
import { AppLayout } from './layout/app-layout/app-layout';
import { Startup } from './features/startup/startup';
import { CategoryOverview } from './features/category/category-overview/category-overview';
import { CategoryDetail } from './features/category/category-detail/category-detail';
import { ProductOverview } from './features/product/product-overview/product-overview';
import { ProductDetail } from './features/product/product-detail/product-detail';

export const routes: Routes = [
      { path: '', redirectTo: 'login', pathMatch: 'full' },
      { path: 'login', component: Login },
      { path: 'change-password', component: ChangePassword, canActivate: [AuthGuard] },
      { path: 'app', component: AppLayout, canActivate: [AuthGuard], children: [
        { path: '', redirectTo: 'startup', pathMatch: 'full' },
        { path: 'startup', component: Startup },
        { path: 'category', children: [
          { path: '', component: CategoryOverview }, 
          { path: 'detail/:mode', component: CategoryDetail }, 
          { path: 'detail/:mode/:id', component: CategoryDetail } 
        ] },
        { path: 'product', children: [
          { path: '', component: ProductOverview }, 
          { path: 'detail/:mode', component: ProductDetail }, 
          { path: 'detail/:mode/:id', component: ProductDetail } 
        ]}
      ]}
    ];
