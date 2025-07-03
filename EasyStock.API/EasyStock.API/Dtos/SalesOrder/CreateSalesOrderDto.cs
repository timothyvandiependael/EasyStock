namespace EasyStock.API.Dtos
{
    public class CreateSalesOrderDto
    {
        public string? Comments { get; set; }
        public int ClientId { get; set; }
    }
}
