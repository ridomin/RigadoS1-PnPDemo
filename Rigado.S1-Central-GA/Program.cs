using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Rigado.S1_Central_GA
{
    class DeviceRunnerService : BackgroundService
    {
        readonly ILogger<DeviceRunnerService> logger;
        readonly IConfiguration configuration;

        public DeviceRunnerService(ILogger<DeviceRunnerService> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionString = ValidateConfigOrDie();
            var device = new S1Device(connectionString, logger, stoppingToken);
            await device.RunDeviceAsync();
        }

        private string ValidateConfigOrDie()
        {
            var connectionString = configuration.GetValue<string>("DeviceConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                logger.LogError("ConnectionString not found using key: DeviceConnectionString");
                throw new ConfigurationErrorsException("Connection String 'DeviceConnectionString' not found in the configured providers.");
            }
            return connectionString;
        }

        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                    services.AddHostedService<DeviceRunnerService>());

            await host.RunConsoleAsync().ConfigureAwait(true);
        }
    }
}
