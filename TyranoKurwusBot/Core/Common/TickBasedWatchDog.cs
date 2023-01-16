namespace TyranoKurwusBot.Core.Common;

public static class TickBasedWatchDog
{
    public static (Thread watchdog, Action tick) Make(TimeSpan maxTick, CancellationTokenSource supervisor, CancellationToken cancellationToken)
    {
        long watchdogCurrent = 0;
        var watchdog = new Thread(() =>
        {
            long localWatchdogLast = 0;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.WaitHandle.WaitOne(maxTick);
                
                // ReSharper disable once AccessToModifiedClosure
                var localWatchdogCurrent = Interlocked.Read(ref watchdogCurrent);
                if (localWatchdogLast == localWatchdogCurrent)
                {
                    supervisor.Cancel();
                    break;
                }

                localWatchdogLast = localWatchdogCurrent;
            }
        });
        
        watchdog.Start();

        return (watchdog, () => { Interlocked.Increment(ref watchdogCurrent); });
    }
}