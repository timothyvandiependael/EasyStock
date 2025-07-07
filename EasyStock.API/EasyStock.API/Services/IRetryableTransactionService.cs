namespace EasyStock.API.Services
{
    public interface IRetryableTransactionService
    {
        Task ExecuteAsync(Func<Task> action);
    }
}
