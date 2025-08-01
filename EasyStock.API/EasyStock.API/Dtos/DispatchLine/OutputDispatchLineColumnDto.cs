using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputDispatchLineColumnDto
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
                Name = "dispatchNumber",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Dispatch Number",
                ValidationRules = new ValidationRules
                {
                    Required = true
                },
                IsLookup = true,
                LookupIdField = "dispatchId",
                LookupTarget = "Dispatch"
            },
            new ColumnMetaData {
                Name = "lineNumber",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Line"
            },
            new ColumnMetaData {
                Name = "comments",
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
                Name = "salesOrderLink",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Sales Order Link",
                IsLookup = true,
                LookupIdField = "salesOrderLineId",
                LookupTarget = "SalesOrderLine",
                ValidationRules = new ValidationRules
                {
                    Required = true,
                }
            },
            new ColumnMetaData {
                Name = "productName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Product",
            },
            new ColumnMetaData {
                Name = "quantity",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Quantity",
                ValidationRules = new ValidationRules
                {
                    Required = true
                }
            }

        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
