using System.Data.SqlClient;
using Test_Example.DTOs;
using Test_Example.Models;

namespace Test_Example.Services
{
    public interface IFireDepartmentServices
    {
        Task<bool> CheckIfFiretruckExistsAsync(int idFireTruck, SqlCommand com);
        Task<FireTruck> GetFireTruckAsync(SqlCommand com);
        Task<IEnumerable<ActionFiremenDTO>> GetActionsFiremenDTOAsync(SqlCommand com, int idFireTruck);
        Task<FireTruckActionDTO> GetFireTruckActionDTOAsync(int idFireTruck);
        Task<bool> CheckIfActionExistsAsync(Models.Action action, SqlCommand com);
        Task<bool> CheckIfActionUpdatePossibleAsync(Models.Action action, SqlCommand com);
        Task UpdateActionEndTimeAsync(Models.Action action, SqlCommand com);
        Task PutActionEndTime(Models.Action action);
    }
}