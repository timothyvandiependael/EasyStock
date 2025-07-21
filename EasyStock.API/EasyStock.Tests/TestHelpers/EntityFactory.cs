using EasyStock.API.Dtos;
using EasyStock.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyStock.Tests.TestHelpers
{
    public class EntityFactory
    {
        public Category CreateCategory()
        {
            return new Category
            {
                Id = 1,
                Name = "TestCategory",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public Client CreateClient()
        {
            return new Client
            {
                Id = 1,
                Name = "TestClient",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public Dispatch CreateDispatch()
        {
            return new Dispatch
            {
                Id = 1,
                DispatchNumber = "TestNumber",
                ClientId = 1,
                Client = CreateClient(),
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public DispatchLine CreateDispatchLine()
        {
            return new DispatchLine
            {
                Id = 1,
                DispatchId = 1,
                Dispatch = CreateDispatch(),
                ProductId = 1,
                Product = CreateProduct(),
                SalesOrderLineId = 1,
                SalesOrderLine = CreateSalesOrderLine(),
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public Product CreateProduct()
        {
            return new Product
            {
                Id = 1,
                Name = "TestProduct",
                CategoryId = 1,
                Category = CreateCategory(),
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public PurchaseOrder CreatePurchaseOrder()
        {
            return new PurchaseOrder
            {
                Id = 1,
                OrderNumber = "TestOrder",
                SupplierId = 1,
                Supplier = CreateSupplier(),
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public PurchaseOrderLine CreatePurchaseOrderLine()
        {
            return new PurchaseOrderLine
            {
                Id = 1,
                PurchaseOrderId = 1,
                PurchaseOrder = CreatePurchaseOrder(),
                ProductId = 1,
                Product = CreateProduct(),
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public Reception CreateReception()
        {
            return new Reception
            {
                Id = 1,
                ReceptionNumber = "TestReception",
                SupplierId = 1,
                Supplier = CreateSupplier(),
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public ReceptionLine CreateReceptionLine()
        {
            return new ReceptionLine
            {
                Id = 1,
                ReceptionId = 1,
                Reception = CreateReception(),
                ProductId = 1,
                Product = CreateProduct(),
                PurchaseOrderLineId = 1,
                PurchaseOrderLine = CreatePurchaseOrderLine(),
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public SalesOrder CreateSalesOrder()
        {
            return new SalesOrder
            {
                Id = 1,
                OrderNumber = "TestOrder",
                ClientId = 1,
                Client = CreateClient(),
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public SalesOrderLine CreateSalesOrderLine()
        {
            return new SalesOrderLine
            {
                Id = 1,
                SalesOrderId = 1,
                SalesOrder = CreateSalesOrder(),
                ProductId = 1,
                Product = CreateProduct(),
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public StockMovement CreateStockMovement()
        {
            return new StockMovement
            {
                ProductId = 1,
                Product = CreateProduct(),
                Reason = "TestReason",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public Supplier CreateSupplier()
        {
            return new Supplier
            {
                Id = 1,
                Name = "TestSupplier",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public User CreateUser()
        {
            return new User
            {
                Id = 1,
                UserName = "TestUser",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public UserPermission CreateUserPermission()
        {
            return new UserPermission
            {
                Id = 1,
                User = CreateUser(),
                UserId = 1,
                Resource = "TestResource",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }

        public UserAuth CreateUserAuth()
        {
            return new UserAuth
            {
                Id = 1,
                UserName = "TestUser",
                PasswordHash = "",
                CrDate = DateTime.UtcNow,
                LcDate = DateTime.UtcNow,
                CrUserId = "system",
                LcUserId = "system"
            };
        }
    }
}
