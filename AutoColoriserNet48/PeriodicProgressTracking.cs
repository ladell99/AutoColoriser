using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoColoriserNet48
{
    public class PeriodicProgressTracking : IDisposable
    {
        private readonly CancellationTokenSource _pcts;
        private readonly Task _pulse;
        
        public PeriodicProgressTracking(Func<Task> onPulse, int pulseMs = 1000)
        {
            _pcts = new CancellationTokenSource();
            var token = _pcts.Token;
            _pulse = Task.Run(async () =>
            {
                // periodic status update
                while (token.IsCancellationRequested == false)
                {
                    await Task.Delay(pulseMs, token);

                    await onPulse();
                }
            });
        }

        public void Dispose()
        {
            _pcts.Cancel();
            try
            {
                _pulse.GetAwaiter().GetResult();
            }
            catch (TaskCanceledException) {}
            catch (OperationCanceledException) {}
        }
    }
}