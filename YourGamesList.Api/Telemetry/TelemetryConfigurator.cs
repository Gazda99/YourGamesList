using System;
using System.Data;
using System.Diagnostics;
using System.Net.Http;
using Refit;

namespace YourGamesList.Api.Telemetry;

public static class TelemetryConfigurator
{
    /// <summary>
    /// Better DB Name tag if using SQLite. Display name contains SQL verb
    /// </summary>
    public static Action<Activity, IDbCommand> EnrichWithIDbCommand => (activity, command) =>
    {
        var conn = command.Connection;
        if (conn == null)
        {
            return;
        }

        var dbIdentifier = conn.Database;

        // Check if DB is an SQLite. If so, then extract datasource name from connection string, as SQLite connection name is always "main".
        var isSqlite = conn.GetType().FullName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) ?? false;

        if (isSqlite && dbIdentifier == "main")
        {
            var dbConnectionStringBuilder = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = conn.ConnectionString };

            if (dbConnectionStringBuilder.TryGetValue("Data Source", out var ds) || dbConnectionStringBuilder.TryGetValue("DataSource", out ds))
            {
                dbIdentifier = System.IO.Path.GetFileName(ds.ToString());
            }
        }

        activity.SetTag("db.system", isSqlite ? "SQLite" : "other");
        activity.SetTag("db.name", dbIdentifier);

        // Change display name to include verb, e.g. "DB: SELECT"
        var sql = command.CommandText;
        var parts = sql.Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            var verb = parts[0].ToUpper();
            activity.DisplayName = $"DB: {verb}";
        }
    };

    /// <summary>
    /// Replaces the default span name with a formatted HTTP Method and Request URI.
    /// </summary>
    public static readonly Action<Activity, HttpRequestMessage> HttpClientInstrumentationTracingEnrichWithBetterDisplayName = (activity, httpRequestMessage) =>
    {
        activity.DisplayName = $"{httpRequestMessage.Method}:{httpRequestMessage.RequestUri}";
    };

    /// <summary>
    /// Extracts the Refit interface method name and adds it as a "destination_name" tag.
    /// </summary>
    public static readonly Action<Activity, HttpRequestMessage> HttpClientInstrumentationTracingEnrichWithRefitMethodInfo = (activity, httpRequestMessage) =>
    {
        const string tagName = "destination.name";

        if (!httpRequestMessage.Options.TryGetValue(new HttpRequestOptionsKey<RestMethodInfo>("Refit.RestMethodInfo"), out var refitMethodInfo))
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(refitMethodInfo.Name))
        {
            activity.SetTag(tagName, refitMethodInfo.Name);
        }
    };
}