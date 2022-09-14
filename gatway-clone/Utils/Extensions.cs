using System.Reflection;
using System.Runtime;
namespace gatway_clone.Utils;

public static class Extensions
{
    private static readonly ILogger _logger = LogUtil.CreateLogger();
    public static async Task<bool> TimeoutAfter(this Task task, TimeSpan timeout)
    {
        var cts = new CancellationTokenSource();
        if (task == await Task.WhenAny(task, Task.Delay(timeout, cts.Token)).ConfigureAwait(false))
        {
            cts.Cancel();
            await task.ConfigureAwait(false);
            return false;
        }
        return true;
    }
    public static IEnumerable<Type> GetTypes<T>(this Assembly assembly, string suffix = "") where T : class
            => assembly.GetTypes(typeof(T), suffix);
    public static IEnumerable<Type> GetTypes(this Assembly assembly, Type type, string suffix = "")
    {
        return assembly.GetTypes().Where(x => !x.IsAbstract &&
            (IsSubclass(x, type) ||
                type.IsAssignableFrom(x) ||
                x.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == type))
            && x.Name.EndsWith(suffix));
    }
    private static bool IsSubclass(Type type1, Type type2)
    {
        if (type1 == null) return false;
        return type1.IsSubclassOf(type2) || IsSubclass(type1.BaseType, type2);
    }
    public static Task<T> HandleExceptions<T>(this Task<T> task, string lmid)
    {
        task.ContinueWith(c => _logger.LogError(c.Exception, lmid),
            TaskContinuationOptions.OnlyOnFaulted |
            TaskContinuationOptions.ExecuteSynchronously);
        return task;
    }
}
