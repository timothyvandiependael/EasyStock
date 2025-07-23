using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputPersonColumnDto
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
            },
            new ColumnMetaData {
                Name = "address",
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
                Name = "city",
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
                Name = "postalCode",
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
                Name = "country",
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
                Name = "phone",
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
                Name = "email",
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
                Name = "website",
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
