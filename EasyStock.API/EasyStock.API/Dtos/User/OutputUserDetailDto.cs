using System.ComponentModel.DataAnnotations;
using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class OutputUserDetailDto : BaseOutputUserDto
    {
        public ICollection<OutputUserPermissionOverviewDto> Permissions { get; set; } 
            = new List<OutputUserPermissionOverviewDto>();
    }
}
