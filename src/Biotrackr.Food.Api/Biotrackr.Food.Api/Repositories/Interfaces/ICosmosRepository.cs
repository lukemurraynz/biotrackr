using Biotrackr.Food.Api.Models;

namespace Biotrackr.Food.Api.Repositories.Interfaces;

public interface ICosmosRepository
{
    Task<List<FoodDocument>> GetAllFoodLogsAsync(int pageNumber, int pageSize);
    Task<FoodDocument?> GetFoodLogByDateAsync(string date);
    Task<List<FoodDocument>> GetFoodLogsByDateRangeAsync(string startDate, string endDate, int pageNumber, int pageSize);
    Task<int> GetTotalFoodLogsCountAsync();
    Task<int> GetFoodLogsCountByDateRangeAsync(string startDate, string endDate);
}
