using EasyStock.API.Common;

namespace EasyStock.API.Dtos
{
    public static class OutputClientColumnDto
    {
        public static readonly List<ColumnMetaData> Columns = new List<ColumnMetaData>()
            .Concat(OutputPersonColumnDto.Columns).Concat(OutputColumnDtoBase.Columns).ToList();
    }
}
