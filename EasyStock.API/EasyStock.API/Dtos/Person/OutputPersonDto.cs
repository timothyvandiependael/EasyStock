﻿using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class OutputPersonDto : OutputDtoBase
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
    }
}
