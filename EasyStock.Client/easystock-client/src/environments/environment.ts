var baseApiUrl = 'https://localhost:7270/api';

export const environment = {
    production: false,
    auth: baseApiUrl + '/auth/',
    category: baseApiUrl + '/categories/',
    client: baseApiUrl + '/clients/',
    dispatch: baseApiUrl + '/dispatches/',
    dispatchLine: baseApiUrl + '/dispatchlines/',
    product: baseApiUrl + '/products/',
    purchaseOrder: baseApiUrl + '/purchaseorders/',
    purchaseOrderLine: baseApiUrl + '/purchaseorderlines/',
    reception: baseApiUrl + '/receptions/',
    receptionLine: baseApiUrl + '/receptionlines/',
    salesOrder: baseApiUrl + '/salesorders/',
    salesOrderLine: baseApiUrl + '/salesorderlines/',
    stockMovement: baseApiUrl + '/stockmovements/',
    supplier: baseApiUrl + '/suppliers/',
    user: baseApiUrl + '/users/',
    userPermission: baseApiUrl + '/userpermissions/'
}