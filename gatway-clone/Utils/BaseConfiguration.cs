using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
namespace gatway_clone.Utils;

public class BaseConfiguration
{
    private static readonly IConfiguration _appSettings = new ConfigurationBuilder()
      .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "baseconfiguration.json"), true)
      .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "baseconfiguration.local.json"), true)
      .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"))
      .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.local.json"), true)
      .Build();
    public static readonly ImmutableDictionary<string, string> DataMaskings = (_appSettings.GetSection("DataMaskings").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>()).ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

    public static readonly string Log4NetConfigurationPath = _appSettings["Log4NetConfigurationPath"];
}
