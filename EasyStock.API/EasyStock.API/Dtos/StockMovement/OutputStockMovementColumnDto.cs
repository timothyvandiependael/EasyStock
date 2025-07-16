using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputStockMovementColumnDto
    {
        public static readonly List<ColumnMetaData> Columns = new List<ColumnMetaData>()
        {
            new ColumnMetaData {
                Name = "Id",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Id"
            },
            new ColumnMetaData {
                Name = "ProductName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Product"
            },
            new ColumnMetaData {
                Name = "QuantityChange",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Quantity Change"
            },
            new ColumnMetaData {
                Name = "Reason",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Reason"
            },
            new ColumnMetaData {
                Name = "PurchaseOrderId",
                Type = "int",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Purchase Order Id"
            },
            new ColumnMetaData {
                Name = "SalesOrderId",
                Type = "int",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Sales Order Id"
            },

        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
