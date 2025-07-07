namespace EasyStock.API.Dtos
{
    public class OutputDtoBase
    {
        public DateTime CrDate { get; set; }
        public DateTime LcDate { get; set; }
        public required string CrUserId { get; set; }
        public required string LcUserId { get; set; }
        public DateTime? BlDate { get; set; }
        public string? BlUserId { get; set; }
    }
}
