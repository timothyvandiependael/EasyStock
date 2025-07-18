using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputColumnDtoBase
    {
        public static List<ColumnMetaData> Columns = new()
        {
            new ColumnMetaData {
                Name = "crDate",
                Type = "date",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Created At",
                ValidationRules = new ValidationRules
                {
                    Required = true
                }
                
            },
            new ColumnMetaData {
                Name = "crUserId",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Created By",
                ValidationRules = new ValidationRules
                {
                    Required = true
                }
            },
            new ColumnMetaData {
                Name = "lcDate",
                Type = "date",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Last Modified",
                ValidationRules = new ValidationRules
                {
                    Required = true
                }
            },
            new ColumnMetaData {
                Name = "lcUserId",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Modified By",
                ValidationRules = new ValidationRules
                {
                    Required = true
                }
            },
            new ColumnMetaData {
                Name = "blDate",
                Type = "date",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Blocked At"
            },
            new ColumnMetaData {
                Name = "blUserId",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Blocked By"
            },
        };
    }
}
