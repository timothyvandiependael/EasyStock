namespace EasyStock.API.Dtos
{
    public class OutputDtoBase
    {
        public DateTime CrDate { get; set; }
        public DateTime LcDate { get; set; }
        public string CrUserId { get; set; }
        public string LcUserId { get; set; }
        public DateTime? BlDate { get; set; }
        public string? BlUserId { get; set; }
    }
}
