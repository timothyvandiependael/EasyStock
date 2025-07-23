using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputUserPermissionColumnDto
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
                Name = "userName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "User Name"
            },
            new ColumnMetaData {
                Name = "resource",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Resource",
                ValidationRules = new ValidationRules
                {
                    Required = true
                }
            },
            new ColumnMetaData {
                Name = "canView",
                Type = "boolean",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Can View"
            },
            new ColumnMetaData {
                Name = "canAdd",
                Type = "boolean",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Can Add"
            },
            new ColumnMetaData {
                Name = "canEdit",
                Type = "boolean",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Can Edit"
            },
            new ColumnMetaData {
                Name = "canDelete",
                Type = "boolean",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Can Delete"
            }
        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
