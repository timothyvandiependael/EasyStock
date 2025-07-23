using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputProductColumnDto
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
                Name = "sku",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "SKU",
                ValidationRules = new ValidationRules
                {
                    MaxLength = 50,
                    Required = true
                }
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
                    MaxLength = 200,
                    Required = true
                }
            },
            new ColumnMetaData {
                Name = "description",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Description"
            },
            new ColumnMetaData {
                Name = "costPrice",
                Type = "number",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Cost Price",
                ValidationRules = new ValidationRules
                {
                    Required = true
                }
            },
            new ColumnMetaData {
                Name = "retailPrice",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Retail Price",
                ValidationRules = new ValidationRules
                {
                    Required = true
                }
            },
            new ColumnMetaData {
                Name = "discount",
                Type = "number",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Discount",
                ValidationRules = new ValidationRules
                {
                    Max = 100
                }
            },
            new ColumnMetaData {
                Name = "totalStock",
                Type = "number",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Total"
            },
            new ColumnMetaData {
                Name = "reservedStock",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Reserved"
            },
            new ColumnMetaData {
                Name = "inboundStock",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Inbound"
            },
            new ColumnMetaData {
                Name = "availableStock",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Available"
            },
            new ColumnMetaData {
                Name = "backOrderedStock",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Backordered"
            },
            new ColumnMetaData {
                Name = "minimumStock",
                Type = "number",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Minimum Stock"
            },
            new ColumnMetaData {
                Name = "autoRestock",
                Type = "boolean",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Auto Restock"
            },
            new ColumnMetaData {
                Name = "supplierName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Supplier",
                IsLookup = true,
                LookupIdField = "AutoRestockSupplierId",
                LookupTarget = "Supplier"
            },
            new ColumnMetaData {
                Name = "categoryName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Category",
                IsLookup = true,
                LookupIdField = "CategoryId",
                LookupTarget = "Category",
                ValidationRules = new ValidationRules
                {
                    Required = true
                }
            },

        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
