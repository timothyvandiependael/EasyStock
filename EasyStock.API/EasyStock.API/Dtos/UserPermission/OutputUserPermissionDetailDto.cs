using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class OutputUserPermissionDetailDto : BaseOutputUserPermissionDto
    {
        public required OutputUserOverviewDto User { get; set; }
    }
}
