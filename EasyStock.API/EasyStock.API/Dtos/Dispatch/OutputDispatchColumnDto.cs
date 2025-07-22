using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputDispatchColumnDto
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
                DisplayName = "Dispatch Number"
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
                Name = "ClientName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Client",
                IsLookup = true,
                LookupIdField = "ClientId",
            }

        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
