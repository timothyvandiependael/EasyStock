using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputReceptionColumnDto
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
                Name = "ReceptionNumber",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Reception Number",
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
                Name = "SupplierName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Supplier",
                IsLookup = true,
                LookupIdField = "SupplierId"
            }

        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
