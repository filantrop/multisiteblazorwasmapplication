using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Shared;
using Refit;
namespace Admin.Client.Services
{
    public interface IAdminService
    {
        Task<ApiResponse<WeatherForecast[]>> WeatherForecasts();
    }
}
