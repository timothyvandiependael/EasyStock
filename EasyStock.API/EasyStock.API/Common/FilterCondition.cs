namespace EasyStock.API.Common
{
    public class FilterCondition
    {
        public required string Field { get; set; }
        public required string Operator { get; set; }
        public required string Value { get; set; }
    }
}
