using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputSalesOrderColumnDto
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
                Name = "orderNumber",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Order Number",
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
                Name = "clientName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Client",
                IsLookup = true,
                LookupIdField = "ClientId",
                LookupTarget = "Client"
            },
            new ColumnMetaData {
                Name = "status",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Status"
            }
        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
