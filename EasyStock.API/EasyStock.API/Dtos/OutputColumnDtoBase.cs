using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputColumnDtoBase
    {
        public static List<ColumnMetaData> Columns = new()
        {
            new ColumnMetaData {
                Name = "CrDate",
                Type = "date",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Created At"
            },
            new ColumnMetaData {
                Name = "CrUserId",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Created By"
            },
            new ColumnMetaData {
                Name = "LcDate",
                Type = "date",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Last Modified"
            },
            new ColumnMetaData {
                Name = "LcUserId",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Modified By"
            },
            new ColumnMetaData {
                Name = "BlDate",
                Type = "date",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Blocked At"
            },
            new ColumnMetaData {
                Name = "BlUserId",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Blocked By"
            },
        };
    }
}
