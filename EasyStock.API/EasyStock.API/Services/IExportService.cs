using EasyStock.API.Common;

namespace EasyStock.API.Services
{
    public interface IExportService<T>
    {
        byte[] GenerateExport(List<T> dtos, string format, List<ColumnMetaData> columns, string title = "");
    }
}
