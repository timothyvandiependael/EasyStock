﻿using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Models
{
    public class Dispatch : ModelBase
    {
        public int Id { get; set; }
        public required string DispatchNumber { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }
        public int ClientId { get; set; }
        public required Client Client { get; set; }
        public ICollection<DispatchLine> Lines { get; set; } = new List<DispatchLine>();
    }
}
