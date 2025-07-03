using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class CreatePersonDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [Phone]
        [MaxLength(25)]
        public string? Phone { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Url]
        [MaxLength(200)]
        public string? Website { get; set; }
    }
}
