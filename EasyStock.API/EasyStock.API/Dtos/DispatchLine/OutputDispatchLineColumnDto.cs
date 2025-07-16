using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputDispatchLineColumnDto
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
                Name = "DispatchNumber",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Dispatch Number",
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
                DisplayName = "Comments"
            },
            new ColumnMetaData {
                Name = "ProductName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Product",
                IsLookup = true,
                LookupIdField = "ProductId",
            },
            new ColumnMetaData {
                Name = "Quantity",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Quantity",
            },

        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
