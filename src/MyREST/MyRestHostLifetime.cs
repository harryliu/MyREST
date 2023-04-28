using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Abstractions;
using NLog;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace MyREST
{
    /// <summary>
    ///
    /// https://github.com/dotnet/dotnet-docker/blob/main/samples/kubernetes/graceful-shutdown/graceful-shutdown.md
    /// </summary>
    public class MyRestHostLifetime : IHostLifetime, IDisposable
    {
        private IHostApplicationLifetime _applicationLifetime;
        private TimeSpan _delay;
        private IEnumerable<IDisposable>? _disposables;
        private AppState _appState;
        private ILogger<MyRestHostLifetime> _logger;

        public MyRestHostLifetime(IHostApplicationLifetime applicationLifetime, TimeSpan delay,
            AppState appState, ILogger<MyRestHostLifetime> logger)

        {
            _applicationLifetime = applicationLifetime;
            _delay = delay;
            _appState = appState;
            _logger = logger;
            _applicationLifetime.ApplicationStopping.Register(OnShutdown);
            _applicationLifetime.ApplicationStopped.Register(AfterShutdown);
        }

        private void OnShutdown()
        {
            while (_appState.getRunningRequests() >= 1)
            {
                _logger.LogWarning($"SIGTERM signal received, but there are {_appState.getRunningRequests()} running requests. Sleep one moment to ensure them handled. ");
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void AfterShutdown()
        {
            string message = $"There are {_appState.getRunningRequests()} running requests. Application stopped. ";
            if (_appState.getRunningRequests() > 0)
            {
                _logger.LogWarning(message);
            }
            else
            {
                _logger.LogInformation(message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            _disposables = new IDisposable[]
            {
            PosixSignalRegistration.Create(PosixSignal.SIGINT, HandleSignal),
            PosixSignalRegistration.Create(PosixSignal.SIGQUIT, HandleSignal),
            PosixSignalRegistration.Create(PosixSignal.SIGTERM, HandleSignal)
            };
            return Task.CompletedTask;
        }

        protected void HandleSignal(PosixSignalContext ctx)
        {
            ctx.Cancel = true;
            Task.Delay(_delay).ContinueWith(t => _applicationLifetime.StopApplication());
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables ?? Enumerable.Empty<IDisposable>())
            {
                disposable.Dispose();
            }
        }
    }
}