using log4net;
namespace gatway_clone.Utils;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

public class WorkerService : IHostedService, IDisposable
{
    protected readonly ILogger _logger = LogUtil.CreateLogger();
    private Task _executingTask;
    private readonly CancellationTokenSource _stoppingCts = new();
    protected virtual Task StartAsync() => Task.CompletedTask;
    protected virtual Task StopAsync() => Task.CompletedTask;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Service is starting");
            await StartAsync();
            _logger.LogInformation("Service is started");
            _executingTask = ExecuteAsync(_stoppingCts.Token);
            if (_executingTask.IsFaulted)
            {
                 throw _executingTask.Exception;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Start service failed");
            _logger.LogInformation("Started failed");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                if (_executingTask != null)
                {
                    _ = await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
                }
            }
            _logger.LogInformation("Service is stoping");
            await StopAsync();
            _logger.LogInformation("Service is stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stop service failed");
        }
        LogManager.Shutdown();
    }

    protected virtual Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;

    public static async Task RunAsync<T>(string[] args, Action<IHostBuilder> configure = null) where T : WorkerService
    {
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
        configure?.Invoke(hostBuilder);
        await hostBuilder
            .UseContentRoot(AppContext.BaseDirectory)
            .ConfigureServices(services => services.AddHostedService<T>())
            .ConfigureLogging(LogUtil.ConfigureLogging)
            .UseWindowsService()
            .Build().RunAsync();
    }

    public void Dispose()
    {
        _stoppingCts.Cancel();
        GC.SuppressFinalize(this);
    }
}
