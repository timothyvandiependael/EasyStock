namespace EasyStock.API.Dtos
{
    public class ExportRequestDto
    {
        public AdvancedQueryParametersDto Parameters { get; set; } = new AdvancedQueryParametersDto();
        public string Format { get; set; } = "";
    }
}
