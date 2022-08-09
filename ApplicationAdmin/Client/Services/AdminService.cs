using Application.Shared;
using Admin.Client.Api;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.Client.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminApi _api;
        public AdminService(IAdminApi api)
        {
            _api = api;
        }
        public async Task<ApiResponse<WeatherForecast[]>> WeatherForecasts()
        {
            return await _api.WeatherForecasts();
        }
    }
}
