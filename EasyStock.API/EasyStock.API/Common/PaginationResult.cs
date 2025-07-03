namespace EasyStock.API.Common
{
    public class PaginationResult<T>
    {
        public int TotalCount { get; set; }
        public List<T> Data { get; set; }

        public PaginationResult()
        {
            Data = new List<T>();
        }

        public PaginationResult(int totalCount, List<T> data)
        {
            TotalCount = totalCount;
            Data = data;
        }
    }
}
