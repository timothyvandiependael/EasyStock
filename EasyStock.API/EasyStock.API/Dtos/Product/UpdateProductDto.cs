﻿namespace EasyStock.API.Dtos
{
    public class UpdateProductDto : CreateProductDto
    {
        public int Id { get; set; }

        public int TotalStock { get; set; }
    }
}
