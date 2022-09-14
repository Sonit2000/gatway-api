using gatway_clone.Utils;
using System.Threading.Tasks.Dataflow;
using gatway_clone.Caches;

namespace gatway_clone.ThreadPools;

public class SettlementThreadPool
{
    private static readonly ILogger _logger = LogUtil.CreateLogger();
    private static readonly int _maxRetries = Configuration.SettlementMaxRetries == 0 ? int.MaxValue : Configuration.SettlementMaxRetries;
    private static readonly int _poolSize = Configuration.SettlementMaxThread;
    private static readonly ActionBlock<TerminalObject> _actionBlock = null;
    private static int _taskCount;
    public static void Init() { }
}
