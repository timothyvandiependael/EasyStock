using EasyStock.API.Common;
using Microsoft.AspNetCore.Http.Features;
using System.Text;
using ClosedXML.Excel;
using System.Reflection;
using DocumentFormat.OpenXml.Spreadsheet;

namespace EasyStock.API.Services
{
    public class ExportService<T> : IExportService<T> where T : class
    {
        public byte[] GenerateExport(List<T> dtos, string format, List<ColumnMetaData> columns, string title = "")
        {
            var file = Array.Empty<byte>();
            if (format == "csv")
            {
                file = GenerateCsv(dtos, columns);
            }
            else if (format == "excel")
            {
                file = GenerateExcel(dtos, columns, title);
            }
            else
            {
                throw new InvalidOperationException("Format not recognized.");
            }

            return file;
        }

        private byte[] GenerateCsv(List<T> dtos, List<ColumnMetaData> columns)
        {
            if (dtos == null || dtos.Count == 0) return Array.Empty<byte>();

            var sb = new StringBuilder();

            sb.AppendLine(string.Join(",", columns.Select(c => Escape(c.DisplayName == null ? c.Name : c.DisplayName))));

            var propMap = columns.Select(c =>
            {
                string pascalName = c.Name == "sku" ? "SKU" : char.ToUpper(c.Name[0]) + c.Name.Substring(1);
                var prop = typeof(T).GetProperty(pascalName);
                if (prop == null)
                {
                    throw new InvalidOperationException($"Property '{pascalName}' not found on type '{typeof(T).Name}'.");
                }
                return prop;
            }).ToList();

            foreach (var item in dtos)
            {
                var values = propMap.Select(p =>
                {
                    var value = p.GetValue(item);
                    return Escape(value?.ToString() ?? string.Empty);
                });
                sb.AppendLine(string.Join(",", values));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.Contains(",") || s.Contains("\""))
            {
                s = s.Replace("\"", "\"\"");
                return $"\"{s}\"";
            }
            return s;
        }

        private byte[] GenerateExcel(List<T> dtos, List<ColumnMetaData> columns, string title)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(string.IsNullOrEmpty(title) ? "Sheet" : title);

                for (var c = 0; c < columns.Count; c++)
                {
                    worksheet.Cell(1, c + 1).Value = columns[c].DisplayName == null ? columns[c].Name : columns[c].DisplayName;
                    worksheet.Cell(1, c + 1).Style.Font.Bold = true;
                }

                var propMap = columns.Select(c =>
                {
                    
                    string pascalName = c.Name == "sku" ? "SKU" : char.ToUpper(c.Name[0]) + c.Name.Substring(1);
                    
                    var prop = typeof(T).GetProperty(pascalName, BindingFlags.Public | BindingFlags.Instance);
                    if (prop == null)
                        throw new InvalidOperationException($"Property '{pascalName}' not found on {typeof(T).Name}");
                    return prop;
                }).ToList();

                for (int i = 0; i < dtos.Count; i++)
                {
                    var item = dtos[i];
                    for (int c = 0; c < propMap.Count; c++)
                    {
                        var val = propMap[c].GetValue(item);
                        worksheet.Cell(i + 2, c + 1).Value = (val == null ? "" : val.ToString());
                    }
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}
