using ClickHub.Interfaces;
using ClickHub.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace ClickHub.Services
{
    public class ClickProcessingService : BackgroundService
    {
        private readonly Channel<ClickData> _channel;
        private readonly IDomainDatabase _domainDatabase;
        private readonly ILogger<ClickProcessingService> _logger;

        public ClickProcessingService(Channel<ClickData> channel, IDomainDatabase domainDatabase, ILogger<ClickProcessingService> logger)
        {
            _channel = channel;
            _domainDatabase = domainDatabase;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await foreach (var clickData in _channel.Reader.ReadAllAsync(stoppingToken))
                    {
                        await ProcessClickAsync(clickData);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Normal cancellation, no need to handle
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing clicks");
                    await Task.Delay(5000, stoppingToken); // Wait before retrying
                }
            }
        }

        private async Task ProcessClickAsync(ClickData clickData)
        {
            try
            {
                await _domainDatabase.ProcessClickDataAsync(clickData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing click data: {ClickId}", clickData.Id);
            }
        }
    }
}