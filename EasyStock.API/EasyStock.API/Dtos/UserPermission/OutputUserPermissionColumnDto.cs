using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputUserPermissionColumnDto
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
                Name = "UserName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "User Name"
            },
            new ColumnMetaData {
                Name = "Resource",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Resource"
            },
            new ColumnMetaData {
                Name = "CanView",
                Type = "boolean",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Can View"
            },
            new ColumnMetaData {
                Name = "CanAdd",
                Type = "boolean",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Can Add"
            },
            new ColumnMetaData {
                Name = "CanEdit",
                Type = "boolean",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Can Edit"
            },
            new ColumnMetaData {
                Name = "CanDelete",
                Type = "boolean",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Can Delete"
            }
        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
