using EasyStock.API.Common;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public static class OutputUserColumnDto
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
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "User Name",
                ValidationRules = new ValidationRules
                {
                    MaxLength = 20,
                    Required = true
                }
            },
            new ColumnMetaData {
                Name = "role",
                Type = "string",
                IsEditable = true,
                IsFilterable = false,
                IsSortable = false,
                DisplayName = "Role"
            },
        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
