using System.Runtime.CompilerServices;

namespace gatway_clone.Utils;

public class TimerUtil
{
    private static readonly ILogger _logger = LogUtil.CreateLogger();
    private static readonly Dictionary<string, Timer> _timers = new();

    public static Timer RegisterTimer(Action callback, TimeSpan period, TimeSpan? dueTime = null, [CallerFilePath] string name = null)
    {
        name = Path.GetFileNameWithoutExtension(name);
        Timer timer = new(state =>
        {
            try
            {
                callback();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, state as string);
            }
        }, name, dueTime ?? TimeSpan.Zero, period);
        _timers.Add(name, timer);
        return timer;
    }

    public static bool UnregisterTimer([CallerFilePath] string name = null)
    {
        name = Path.GetFileNameWithoutExtension(name);
        if (!_timers.ContainsKey(name))
        {
            return false;
        }
        _timers[name].Dispose();
        return true;
    }
}
