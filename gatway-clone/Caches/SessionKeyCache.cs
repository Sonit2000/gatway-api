using gatway_clone.Utils.CacheBase;
using System;

namespace gatway_clone.Caches;

public class SessionKeyCache
{
}
public class SessionObject : CacheData
{
    public byte[] SessionKey { get; set; }
    public string Salt { get; set; }

    public string Key { get; set; }
    private static readonly Lazy<SessionKeyCache> _lazy = new(() => new());
    public override string GetPrimaryKey() => Key;
    public static SessionKeyCache Instance => _lazy.Value;
}
