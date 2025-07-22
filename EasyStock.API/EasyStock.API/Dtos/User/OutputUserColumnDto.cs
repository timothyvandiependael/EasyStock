using EasyStock.API.Common;
using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public static class OutputUserColumnDto
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
                DisplayName = "User Name",
                ValidationRules = new ValidationRules
                {
                    MaxLength = 20,
                    Required = true
                }
            }
        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
