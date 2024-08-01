using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

public class DatabaseKeepAliveService : BackgroundService
{
    private readonly ILogger<DatabaseKeepAliveService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _interval;

    public DatabaseKeepAliveService(ILogger<DatabaseKeepAliveService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _interval = TimeSpan.FromMinutes(10); // Set the interval to 5 minutes
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync(stoppingToken);
                    var commandText = "SELECT 1";

                    using (SqlCommand cmd = new SqlCommand(commandText, conn))
                    {
                        var result = await cmd.ExecuteScalarAsync(stoppingToken);
                        _logger.LogInformation($"Keep-alive query executed at: {DateTime.Now}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while executing keep-alive query.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
