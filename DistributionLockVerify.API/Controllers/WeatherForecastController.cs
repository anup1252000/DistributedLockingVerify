using Medallion.Threading;
using Microsoft.AspNetCore.Mvc;

namespace DistributionLockVerify.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedLockProvider distributedLockProvider,
        IDistributedReaderWriterLockProvider distributedReaderWriterLockProvider) : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger = logger;
        private readonly IDistributedLockProvider distributedLockProvider = distributedLockProvider;
        private readonly IDistributedReaderWriterLockProvider distributedReaderWriterLockProvider = distributedReaderWriterLockProvider;
        private static List<WeatherForecast> _forecasts;

        static WeatherForecastController()
        {
            _forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToList();
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get(string lockname, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (await distributedLockProvider.AcquireLockAsync(lockname, TimeSpan.FromSeconds(30), cancellationToken))
            {
                _logger.LogInformation("Entered critical phase");
                await Task.Delay(20000);

                _logger.LogInformation("exit critical phase");
                return _forecasts;
            }
        }

        [HttpPost]
        public async Task<IEnumerable<WeatherForecast>> InsertWeather(WeatherForecast weatherForecast, string lockname, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<WeatherForecast> forecast;
            try
            {
                using (await distributedReaderWriterLockProvider.AcquireReadLockAsync(lockname, TimeSpan.FromSeconds(30), cancellationToken: cancellationToken))
                {
                    _logger.LogInformation("Entered Read critical phase");
                    await Task.Delay(100, cancellationToken);
                    forecast = _forecasts;
                    _logger.LogInformation("exit Read critical phase");
                }
                using (await distributedReaderWriterLockProvider.AcquireWriteLockAsync(lockname, TimeSpan.FromMicroseconds(30), cancellationToken))
                {
                    _logger.LogInformation("Entered write critical phase");
                    forecast.Add(weatherForecast);
                    _logger.LogInformation("exit write critical phase");
                }
            }
            catch (TimeoutException)
            {

                throw;
            }
            
            return forecast;
        }
    }
}
