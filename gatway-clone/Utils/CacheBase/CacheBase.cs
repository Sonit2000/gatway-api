namespace gatway_clone.Utils.CacheBase;

public abstract class CacheBase<T> : ICache where T : CacheData
{
    protected readonly ILogger _logger = LogUtil.CreateLogger();

    public bool IsLog { get; set; }
    public TimeSpan? Duration { get; set; }

    protected CacheBase(TimeSpan? duration, bool isLog)
    {
        IsLog = isLog;
        Duration = duration;
    }

    public virtual async Task<bool> AddAsync(string lmid, T cacheObject)
    {
        bool result = await AddImplAsync(lmid, cacheObject);
        if (result)
        {
            string primaryKey = cacheObject.GetPrimaryKey();
            foreach (string subKey in cacheObject.GetSupplementKeys())
            {
                await AddSubKeyAsync(lmid, subKey, primaryKey);
            }
        }
        if (IsLog)
        {
            _logger.LogDebug($"{lmid}: {GetType().Name}: Add key {MaskKey(cacheObject.GetPrimaryKey())} with result: {result}");
        }
        return result;
    }

    public virtual async Task<T> GetAsync(string lmid, string key)
    {
        if (string.IsNullOrEmpty(key) || key == "CacheName" || key.Contains(":")) return default;
        T instance = await GetImplAsync(lmid, key);
        if (instance == null)
        {
            instance = await LoadFromDBAsync(lmid, key);
            if (instance != null)
            {
                await AddAsync(lmid, instance);
            }
        }
        return instance;
    }

    public async Task<bool> RemoveAsync(string lmid, string key)
    {
        bool result = false;
        T cacheObject = await GetImplAsync(lmid, key);
        if (cacheObject != null)
        {
            result = await RemoveImplAsync(lmid, key);
            foreach (string subKey in cacheObject.GetSupplementKeys())
            {
                await RemoveImplAsync(lmid, subKey);
            }
        }
        if (IsLog)
        {
            _logger.LogDebug($"{lmid}: {GetType().Name}: Remove key {MaskKey(key)} with result: {result}");
        }
        return true;
    }

    protected virtual async Task<T> LoadFromDBAsync(string lmid, string key) => await Task.FromResult<T>(default);

    protected virtual List<T> LoadFromDB() => new();

    protected virtual string MaskKey(string key) => key;

    public abstract Task ClearAsync(string lmid);

    public async Task ReloadAsync(string lmid)
    {
        if (IsLog)
        {
            _logger.LogDebug($"{lmid}: {GetType().Name}: Reload");
        }
        await ClearAsync(lmid);
        foreach (T value in LoadFromDB())
        {
            await AddAsync(lmid, value);
        }
    }

    public async Task ReloadAsync(string lmid, string key)
    {
        await RemoveAsync(lmid, key);
        T value = await LoadFromDBAsync(lmid, key);
        if (value != null)
        {
            await AddAsync(lmid, value);
        }
    }

    public abstract Task<int> CountAsync { get; }

    public abstract Task<IDictionary<string, T>> GetAllAsync(string lmid);

    protected abstract Task<bool> AddImplAsync(string lmid, T cacheObject);

    protected abstract Task<T> GetImplAsync(string lmid, string key);

    protected abstract Task<bool> RemoveImplAsync(string lmid, string key);

    protected async Task<T> GetBySubKeyAsync(string lmid, string subFieldName, string subKey)
        => string.IsNullOrEmpty(subKey) ? default :
            await GetImplAsync(lmid, await GetPrimaryKeyFromSubKeyAsync(lmid, $"{subFieldName}:{subKey}"));

    protected abstract Task<string> GetPrimaryKeyFromSubKeyAsync(string lmid, string subKey);

    protected abstract Task<bool> AddSubKeyAsync(string lmid, string subKey, string primaryKey);
}

public abstract class CacheData
{
    public abstract string GetPrimaryKey();

    public virtual List<string> GetSupplementKeys() => new();

    public static string FormatSubKey(string subKeyName, string subKeyValue) => $"{subKeyName}:{subKeyValue}";
}

public interface ICache
{
    public Task<bool> RemoveAsync(string lmid, string key);
    public Task ReloadAsync(string lmid, string key);
    public Task ReloadAsync(string lmid);
}
