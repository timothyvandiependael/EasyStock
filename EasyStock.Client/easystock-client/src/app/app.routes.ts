import { Routes } from '@angular/router';

import { Login } from './/features/auth/login/login';
import { ChangePassword } from './features/auth/change-password/change-password';
import { AppLayout } from './layout/app-layout/app-layout';
import { Startup } from './features/startup/startup';

export const routes: Routes = [
      { path: '', redirectTo: 'login', pathMatch: 'full' },
      { path: 'login', component: Login },
      { path: 'change-password', component: ChangePassword },
      { path: 'app', component: AppLayout, children: [
        { path: '', redirectTo: 'startup', pathMatch: 'full' },
        { path: 'startup', component: Startup }
      ]}
    ];
