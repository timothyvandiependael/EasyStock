import { Routes } from '@angular/router';
import { AuthGuard } from './features/auth/auth.guard';
import { Login } from './/features/auth/login/login';
import { ChangePassword } from './features/auth/change-password/change-password';
import { AppLayout } from './layout/app-layout/app-layout';
import { Startup } from './features/startup/startup';
import { CategoryOverview } from './features/category/category-overview/category-overview';
import { CategoryEdit } from './features/category/category-edit/category-edit';
import { ProductOverview } from './features/product/product-overview/product-overview';
import { ProductEdit } from './features/product/product-edit/product-edit';
import { StockMovementOverview } from './features/stock-movement/stock-movement-overview/stock-movement-overview';
import { StockMovementEdit } from './features/stock-movement/stock-movement-edit/stock-movement-edit';
import { PurchaseOrderOverview } from './features/purchase-order/purchase-order-overview/purchase-order-overview';
import { PurchaseOrderEdit } from './features/purchase-order/purchase-order-edit/purchase-order-edit';
import { PurchaseOrderLineOverview } from './features/purchase-order-line/purchase-order-line-overview/purchase-order-line-overview';
import { PurchaseOrderLineEdit } from './features/purchase-order-line/purchase-order-line-edit/purchase-order-line-edit';
import { SalesOrderOverview } from './features/sales-order/sales-order-overview/sales-order-overview';
import { SalesOrderEdit } from './features/sales-order/sales-order-edit/sales-order-edit';
import { SalesOrderLineOverview } from './features/sales-order-line/sales-order-line-overview/sales-order-line-overview';
import { SalesOrderLineEdit } from './features/sales-order-line/sales-order-line-edit/sales-order-line-edit';
import { ReceptionOverview } from './features/reception/reception-overview/reception-overview';
import { ReceptionEdit } from './features/reception/reception-edit/reception-edit';
import { ReceptionLineOverview } from './features/reception-line/reception-line-overview/reception-line-overview';
import { ReceptionLineEdit } from './features/reception-line/reception-line-edit/reception-line-edit';
import { DispatchOverview } from './features/dispatch/dispatch-overview/dispatch-overview';
import { DispatchEdit } from './features/dispatch/dispatch-edit/dispatch-edit';
import { DispatchLineOverview } from './features/dispatch-line/dispatch-line-overview/dispatch-line-overview';
import { DispatchLineEdit } from './features/dispatch-line/dispatch-line-edit/dispatch-line-edit';
import { UserOverview } from './features/user/user-overview/user-overview';
import { UserEdit } from './features/user/user-edit/user-edit';
import { SupplierOverview } from './features/supplier/supplier-overview/supplier-overview';
import { SupplierEdit } from './features/supplier/supplier-edit/supplier-edit';
import { ClientOverview } from './features/client/client-overview/client-overview';
import { ClientEdit } from './features/client/client-edit/client-edit';
import { UserPermission } from './features/user/user-permission/user-permission';

export const routes: Routes = [
      { path: '', redirectTo: 'login', pathMatch: 'full' },
      { path: 'login', component: Login },
      { path: 'change-password', component: ChangePassword, canActivate: [AuthGuard] },
      { path: 'app', component: AppLayout, canActivate: [AuthGuard], children: [
        { path: '', redirectTo: 'startup', pathMatch: 'full' },
        { path: 'startup', component: Startup },
        { path: 'product', children: [
          { path: '', component: ProductOverview }, 
          { path: 'edit/:mode', component: ProductEdit }, 
          { path: 'edit/:mode/:id', component: ProductEdit } 
        ]},
        { path: 'stockmovement', children: [
          { path: '', component: StockMovementOverview }, 
          { path: 'edit/:mode', component: StockMovementEdit }, 
          { path: 'edit/:mode/:id', component: StockMovementEdit } 
        ]},
        { path: 'purchaseorder', children: [
          { path: '', component: PurchaseOrderOverview }, 
          { path: 'edit/:mode', component: PurchaseOrderEdit }, 
          { path: 'edit/:mode/:id', component: PurchaseOrderEdit },
        ] },
        { path: 'purchaseorderline', children: [
          { path: '', component: PurchaseOrderLineOverview }, 
          { path: 'edit/:mode', component: PurchaseOrderLineEdit }, 
          { path: 'edit/:mode/:id', component: PurchaseOrderLineEdit },
        ] },
        { path: 'salesorder', children: [
          { path: '', component: SalesOrderOverview }, 
          { path: 'edit/:mode', component: SalesOrderEdit }, 
          { path: 'edit/:mode/:id', component: SalesOrderEdit },
        ] },
        { path: 'salesorderline', children: [
          { path: '', component: SalesOrderLineOverview }, 
          { path: 'edit/:mode', component: SalesOrderLineEdit }, 
          { path: 'edit/:mode/:id', component: SalesOrderLineEdit },
        ] },
        { path: 'reception', children: [
          { path: '', component: ReceptionOverview }, 
          { path: 'edit/:mode', component: ReceptionEdit }, 
          { path: 'edit/:mode/:id', component: ReceptionEdit },
        ] },
        { path: 'receptionline', children: [
          { path: '', component: ReceptionLineOverview }, 
          { path: 'edit/:mode', component: ReceptionLineEdit }, 
          { path: 'edit/:mode/:id', component: ReceptionLineEdit },
        ] },
        { path: 'dispatch', children: [
          { path: '', component: DispatchOverview }, 
          { path: 'edit/:mode', component: DispatchEdit }, 
          { path: 'edit/:mode/:id', component: DispatchEdit },
        ] },
        { path: 'dispatchline', children: [
          { path: '', component: DispatchLineOverview }, 
          { path: 'edit/:mode', component: DispatchLineEdit }, 
          { path: 'edit/:mode/:id', component: DispatchLineEdit },
        ] },
        { path: 'user', children: [
          { path: '', component: UserOverview }, 
          { path: 'edit/:mode', component: UserEdit }, 
          { path: 'edit/:mode/:id', component: UserEdit },
          { path: 'permission', component: UserPermission }
        ] },
        { path: 'supplier', children: [
          { path: '', component: SupplierOverview }, 
          { path: 'edit/:mode', component: SupplierEdit }, 
          { path: 'edit/:mode/:id', component: SupplierEdit },
        ] },
        { path: 'client', children: [
          { path: '', component: ClientOverview }, 
          { path: 'edit/:mode', component: ClientEdit }, 
          { path: 'edit/:mode/:id', component: ClientEdit },
        ] },
        { path: 'category', children: [
          { path: '', component: CategoryOverview }, 
          { path: 'edit/:mode', component: CategoryEdit }, 
          { path: 'edit/:mode/:id', component: CategoryEdit },
        ] },
      ]}
    ];
