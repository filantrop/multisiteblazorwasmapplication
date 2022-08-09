using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Shared;
using Refit;
namespace Admin.Server.Api
{
    public interface IAdminApi
    {
        [Get("/WeatherForecast")]
        Task<WeatherForecast[]> WeatherForecasts();
    }
}
