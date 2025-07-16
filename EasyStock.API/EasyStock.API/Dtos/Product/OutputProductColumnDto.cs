using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputProductColumnDto
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
                Name = "SKU",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "SKU",
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
                Name = "Description",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Description"
            },
            new ColumnMetaData {
                Name = "CostPrice",
                Type = "number",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Cost Price"
            },
            new ColumnMetaData {
                Name = "RetailPrice",
                Type = "string",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Retail Price"
            },
            new ColumnMetaData {
                Name = "Discount",
                Type = "number",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Discount"
            },
            new ColumnMetaData {
                Name = "TotalStock",
                Type = "number",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Total"
            },
            new ColumnMetaData {
                Name = "ReservedStock",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Reserved"
            },
            new ColumnMetaData {
                Name = "InboundStock",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Inbound"
            },
            new ColumnMetaData {
                Name = "AvailableStock",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Available"
            },
            new ColumnMetaData {
                Name = "BackOrderedStock",
                Type = "number",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Backordered"
            },
            new ColumnMetaData {
                Name = "MinimumStock",
                Type = "number",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Minimum Stock"
            },
            new ColumnMetaData {
                Name = "AutoRestock",
                Type = "boolean",
                IsEditable = true,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Auto Restock"
            },
            new ColumnMetaData {
                Name = "SupplierName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Supplier",
                IsLookup = true,
                LookupIdField = "AutoRestockSupplierId"
            },
            new ColumnMetaData {
                Name = "CategoryName",
                Type = "string",
                IsEditable = false,
                IsFilterable = true,
                IsSortable = true,
                DisplayName = "Category",
                IsLookup = true,
                LookupIdField = "CategoryId"
            },

        }.Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
