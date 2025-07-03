using System.ComponentModel.DataAnnotations;

namespace EasyStock.API.Dtos
{
    public class UpdatePersonDto : CreatePersonDto
    {
        public int Id { get; set; }
       
    }
}
