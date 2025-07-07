namespace EasyStock.API.Dtos
{
    public class OutputCategoryDto : OutputDtoBase
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
