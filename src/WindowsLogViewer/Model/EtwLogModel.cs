// Copyright (c) William Kent and contributors. All rights reserved.

using System.Diagnostics.Eventing.Reader;

namespace WindowsLogViewer.Model;

/// <summary>
/// Represents the "new" Windows Vista-era event tracing mechanism.
/// </summary>
internal sealed class EtwLogModel : BaseLogModelSource, IDisposable
{
    /// <summary>
    /// Gets a list of <see cref="EtwLogModel"/> for every log in the system.
    /// </summary>
    /// <remarks>
    /// This is calculated lazily and cached.
    /// </remarks>
    public static IReadOnlyList<EtwLogModel> AllLogs => new Lazy<List<EtwLogModel>>(() =>
    {
        List<EtwLogModel> logList = new List<EtwLogModel>();

        EventLogSession session = EventLogSession.GlobalSession;
        foreach (string logName in session.GetLogNames())
        {
            logList.Add(new EtwLogModel(logName));
        }

        return logList;
    }).Value;

    private string name;
    private readonly List<LogModelEntry> entries = new List<LogModelEntry>();
    private readonly EventLogReader reader;

    /// <summary>
    /// Initializes a new instance of the <see cref="EtwLogModel"/> class.
    /// </summary>
    /// <param name="logName">
    /// The name of the ETW log stream to read.
    /// </param>
    public EtwLogModel(string logName)
    {
        EventLogQuery query = new EventLogQuery(logName, PathType.LogName);
        reader = new EventLogReader(query);
        name = logName;
    }

    /// <inheritdoc/>
    public override IReadOnlyList<LogModelEntry> Entries => entries;

    /// <summary>
    /// Gets the name of the log.
    /// </summary>
    public override string LogName => name;

    /// <inheritdoc/>
    public void Dispose()
    {
        reader.Dispose();
    }

    /// <inheritdoc/>
    public override Task PopulateAsync() => Task.Run(() =>
    {
        reader.BatchSize = 512;

        while (true)
        {
            EventRecord? logEntry;

            try
            {
                logEntry = reader.ReadEvent();
            }
            catch
            {
                // ignore error and try again
                continue;
            }

            if (logEntry == null) break;

            LogModelEntry modelEntry = new()
            {
                Source = logEntry.ProviderName,
                EventId = logEntry.Id,
                Message = logEntry.FormatDescription(),

                Severity = logEntry.LevelDisplayName switch
                {
                    "Information" => LogEntrySeverity.Informational,
                    "Warning" => LogEntrySeverity.Warning,
                    "Error" => LogEntrySeverity.Error,
                    _ => LogEntrySeverity.Unknown
                },
            };

            entries.Append(modelEntry);
        }
    });
}
