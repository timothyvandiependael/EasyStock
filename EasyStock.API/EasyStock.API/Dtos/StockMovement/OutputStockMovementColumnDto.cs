using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputStockMovementColumnDto
    {
        public static readonly List<ColumnMetaData> Columns = new List<ColumnMetaData>()
        {
            new ColumnMetaData {
                Name = "id",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Id"
            },
            new ColumnMetaData {
                Name = "productName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Product"
            },
            new ColumnMetaData {
                Name = "quantityChange",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Quantity Change",
                ValidationRules = new ValidationRules
                {
                    Required = true
                }
            },
            new ColumnMetaData {
                Name = "reason",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Reason",
                ValidationRules = new ValidationRules
                {
                    MaxLength = 200,
                    Required = true
                }
            },
            new ColumnMetaData {
                Name = "purchaseOrderId",
                Type = "int",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Purchase Order Id"
            },
            new ColumnMetaData {
                Name = "salesOrderId",
                Type = "int",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Sales Order Id"
            },

        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
