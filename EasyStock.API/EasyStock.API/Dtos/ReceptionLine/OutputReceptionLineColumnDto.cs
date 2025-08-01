using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputReceptionLineColumnDto
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
                Name = "receptionNumber",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Reception Number",
                ValidationRules = new ValidationRules
                {
                    Required = true
                },
                IsLookup = true,
                LookupIdField = "receptionId",
                LookupTarget = "Reception"
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
                Name = "purchaseOrderLink",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Purchase Order Link",
                IsLookup = true,
                LookupIdField = "purchaseOrderLineId",
                LookupTarget = "PurchaseOrderLine",
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
                DisplayName = "Product"
            },
            new ColumnMetaData {
                Name = "quantity",
                Type = "number",
                IsEditable = true,
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
