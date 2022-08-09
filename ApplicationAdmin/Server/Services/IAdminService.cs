using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Shared;
using Refit;
namespace Admin.Server.Services
{
    public interface IAdminService
    {
        Task<WeatherForecast[]> WeatherForecasts();
    }
}
