using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputSalesOrderLineColumnDto
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
                Name = "OrderNumber",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Order Number",
            },
            new ColumnMetaData {
                Name = "LineNumber",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Line"
            },
            new ColumnMetaData {
                Name = "Comments",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Comments",
                ValidationRules = new ValidationRules
                {
                    MaxLength = 1000
                }
            },
            new ColumnMetaData {
                Name = "ProductName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Product",
                IsLookup = true,
                LookupIdField = "ProductId"
            },
            new ColumnMetaData {
                Name = "Quantity",
                Type = "number",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Quantity",
                ValidationRules = new ValidationRules
                {
                    Required = true
                }
            },
            new ColumnMetaData {
                Name = "UnitPrice",
                Type = "number",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Unit Price",
                ValidationRules = new ValidationRules
                {
                    Required = true
                }
            },
            new ColumnMetaData {
                Name = "Status",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Status"
            },
            new ColumnMetaData {
                Name = "DispatchedQuantity",
                Type = "number",
                IsEditable = false,
                IsFilterable = false,
                IsSortable = false,
                DisplayName = "Dispatched Quantity"
            }

        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
