using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputPersonColumnDto
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
            },
            new ColumnMetaData {
                Name = "Address",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Address"
            },
            new ColumnMetaData {
                Name = "City",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "City"
            },
            new ColumnMetaData {
                Name = "PostalCode",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Postal Code"
            },
            new ColumnMetaData {
                Name = "Country",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Country"
            },
            new ColumnMetaData {
                Name = "Phone",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Phone"
            },
            new ColumnMetaData {
                Name = "Email",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "E-Mail"
            },
            new ColumnMetaData {
                Name = "Website",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Website"
            }
        };
    }
}
