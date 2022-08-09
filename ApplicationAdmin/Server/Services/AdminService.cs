using Application.Shared;
using Admin.Server.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Admin.Server.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminApi _api;
        public AdminService(IAdminApi api)
        {
            _api = api;
        }
        public async Task<WeatherForecast[]> WeatherForecasts()
        {
            return await _api.WeatherForecasts();
        }
    }
}
