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
                Name = "productName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Product",
                IsLookup = true,
                LookupIdField = "ProductId",
                LookupTarget = "Product"
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
