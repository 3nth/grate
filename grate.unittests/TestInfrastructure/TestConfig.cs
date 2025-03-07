﻿using System;
using System.IO;
using System.Linq;
using grate.Configuration;
using Microsoft.Extensions.Logging;
using static System.StringSplitOptions;

namespace grate.unittests.TestInfrastructure;

public static class TestConfig
{
    private static readonly Random Random = new();

    public static string RandomDatabase() => Random.GetString(15);
        
    public static readonly ILoggerFactory LogFactory = LoggerFactory.Create(builder =>
    {
        builder
            .AddProvider(new NUnitLoggerProvider(GetLogLevel()))
            .SetMinimumLevel(GetLogLevel());
    });
        
    public static DirectoryInfo CreateRandomTempDirectory()
    {
        var dummyFile = Path.GetTempFileName();
        File.Delete(dummyFile);

        var scriptsDir = Directory.CreateDirectory(dummyFile);
        return scriptsDir;
    }
    
    public static string? Username(string connectionString) => connectionString.Split(";", TrimEntries | RemoveEmptyEntries)
        .SingleOrDefault(entry => entry.StartsWith("Uid"))?
        .Split("=", TrimEntries | RemoveEmptyEntries).Last();
        
    public static string? Password(string connectionString) => connectionString.Split(";", TrimEntries | RemoveEmptyEntries)
        .SingleOrDefault(entry => entry.StartsWith("Password") || entry.StartsWith("Pwd"))?
        .Split("=", TrimEntries | RemoveEmptyEntries).Last();
    
    private static LogLevel GetLogLevel()
    {
        if (!Enum.TryParse(Environment.GetEnvironmentVariable("LogLevel"), out LogLevel logLevel))
        {
            logLevel = LogLevel.Trace;
        }
        return logLevel;
    }
        
    public static void WriteContent(DirectoryInfo? path, string filename, string? content)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (!path.Exists)
        {
            path.Create();
        }

        File.WriteAllText(Path.Combine(path.ToString(), filename), content);
    }

    public static DirectoryInfo MakeSurePathExists(DirectoryInfo? path)
    {
        ArgumentNullException.ThrowIfNull(path);
        
        if (!path.Exists)
        {
            path.Create();
        }
       
        return path;
    }

    public static IGrateTestContext GetTestContext(DatabaseType databaseType) => databaseType switch
    {
        DatabaseType.mariadb => new MariaDbGrateTestContext(),
        DatabaseType.oracle => new OracleGrateTestContext(),
        DatabaseType.postgresql => new PostgreSqlGrateTestContext(),
        DatabaseType.sqlite => new SqliteGrateTestContext(),
        DatabaseType.sqlserver => new SqlServerGrateTestContext(),
        _ => throw new ArgumentOutOfRangeException(nameof(databaseType), databaseType.ToString())
    };

}
