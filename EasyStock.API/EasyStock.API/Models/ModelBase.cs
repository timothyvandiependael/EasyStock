using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class ModelBase
    {
        [Required]
        public DateTime CrDate { get; set; }
        [Required]
        public DateTime LcDate { get; set; }
        [Required]
        public string CrUserId { get; set; }
        [Required]
        public string LcUserId { get; set; }

        public DateTime? BlDate { get; set; }
        public string? BlUserId { get; set; }
    }
}
