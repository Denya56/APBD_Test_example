﻿using System.Data.SqlClient;
using Test_Example.DTOs;
using Test_Example.Models;

namespace Test_Example.Services
{
    interface IFireDepartmentServices
    {
        Task<bool> CheckIfFiretruckExistsAsync(int idFireTruck, SqlCommand com);
        Task<FireTruck> GetFireTruckAsync(SqlCommand com);
        Task<List<ActionFiremenDTO>> GetActionsFiremenDTOAsync(SqlCommand com);
        Task<bool> CheckIfActionExistsAsync(Models.Action action, SqlCommand com);
        Task<bool> CheckIfActionUpdatePossibleAsync(Models.Action action, SqlCommand com);
        void UpdateActionEndTimeAsync(Models.Action action, SqlCommand com);
    }
}