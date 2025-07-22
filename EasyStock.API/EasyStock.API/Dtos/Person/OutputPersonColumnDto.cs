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
                DisplayName = "Name",
                ValidationRules = new ValidationRules
                {
                    Required = true,
                    MaxLength = 100
                }
            },
            new ColumnMetaData {
                Name = "Address",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Address",
                ValidationRules = new ValidationRules
                {
                    MaxLength = 200
                }
            },
            new ColumnMetaData {
                Name = "City",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "City",
                ValidationRules = new ValidationRules
                {
                    MaxLength = 100
                }
            },
            new ColumnMetaData {
                Name = "PostalCode",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Postal Code",
                ValidationRules = new ValidationRules
                {
                    MaxLength = 20
                }
            },
            new ColumnMetaData {
                Name = "Country",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Country",
                ValidationRules = new ValidationRules
                {
                    MaxLength = 100
                }
            },
            new ColumnMetaData {
                Name = "Phone",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Phone",
                ValidationRules = new ValidationRules
                {
                    MaxLength = 25
                }
            },
            new ColumnMetaData {
                Name = "Email",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "E-Mail",
                ValidationRules = new ValidationRules
                {
                    MaxLength = 200,
                    IsEmail = true
                }
            },
            new ColumnMetaData {
                Name = "Website",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Website",
                ValidationRules = new ValidationRules
                {
                    MaxLength = 200,
                    IsUrl = true
                }
            }
        };
    }
}
