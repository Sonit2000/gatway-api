using System.Reflection;
using gatway_clone.Utils;
namespace gatway_clone.Utils.CacheBase;

public class CacheFactory
{
    private static readonly ILogger _logger = LogUtil.CreateLogger();

    private static readonly Dictionary<string, ICache> _caches = new();

    static CacheFactory()
    {
        IEnumerable<Type> types = Assembly.GetExecutingAssembly().GetTypes<ICache>("Cache");
        types = types.Concat(Assembly.GetEntryAssembly().GetTypes<ICache>("Cache"));

        foreach (Type type in types)
        {
            try
            {
                _caches.Add(type.Name, (ICache)type.GetProperty("Instance").GetValue(null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, type.Name);
                if (ex is ReflectionTypeLoadException)
                {
                    _logger.LogError((ex as ReflectionTypeLoadException).LoaderExceptions[0], "");
                }
            }
        }
    }

    public static void Init() { }

    public static ICache Get(string cacheName) => _caches.ContainsKey(cacheName) ? _caches[cacheName] : default;

    public static void Dispose()
    {
        foreach (ICache cache in _caches.Values)
        {
            (cache as IDisposable)?.Dispose();
        }
    }
}
