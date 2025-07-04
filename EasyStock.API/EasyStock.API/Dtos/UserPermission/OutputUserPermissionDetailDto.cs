using EasyStock.API.Models;

namespace EasyStock.API.Dtos
{
    public class OutputUserPermissionDetailDto : BaseOutputUserPermissionDto
    {
        public OutputUserOverviewDto User { get; set; }
    }
}
