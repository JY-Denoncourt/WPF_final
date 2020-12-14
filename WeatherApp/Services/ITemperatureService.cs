using System.Threading.Tasks;
using WeatherApp.Models;

namespace WeatherApp.ViewModels
{
    public interface ITemperatureService
    {
        Task<TemperatureModel> GetTempAsync();
        void SetLocation(string city);
    }
}