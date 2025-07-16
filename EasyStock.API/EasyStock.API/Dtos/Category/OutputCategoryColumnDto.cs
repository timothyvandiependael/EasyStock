using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputCategoryColumnDto
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
                Name = "Name",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Name"
            }
        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
