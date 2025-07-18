namespace EasyStock.API.Common
{
    public class ColumnMetaData
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; 
        public bool IsEditable { get; set; }
        public bool IsFilterable { get; set; }
        public bool IsSortable { get; set; }
        public string? DisplayName { get; set; }
        public bool IsLookup { get; set; } = false;
        public string? LookupIdField { get; set; }
        public ValidationRules ValidationRules { get; set; } = new ValidationRules();
    }
}
