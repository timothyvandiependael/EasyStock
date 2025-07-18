using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputCategoryColumnDto
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
                Name = "name",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Name",
                ValidationRules = new ValidationRules
                {
                    Required = true,
                    MaxLength = 100
                }
            }
        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
