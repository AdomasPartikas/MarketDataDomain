using System;
using System.Threading;
using System.Threading.Tasks;

namespace MarketDataDomain.API.Utils
{
    public static class ProgressReporter
    {
        private static CancellationTokenSource _cancellationTokenSource;
        private static Task _progressTask;

        public static void StartAwaitingNotifier()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            _progressTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    Console.WriteLine("Please wait...");
                    await Task.Delay(5000, token);
                }
            }, token);
        }

        public static async Task StopAwaitingNotifier()
        {
            _cancellationTokenSource.Cancel();    
            try
            {
                await _progressTask;
            }
            catch (TaskCanceledException)
            {
                // Task was canceled, no further action needed
            }
        }
    }
}