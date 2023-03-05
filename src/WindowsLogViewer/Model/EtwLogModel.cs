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
        var names = session.GetLogNames().ToList();
        names.Sort();

        foreach (string logName in names)
        {
            try
            {
                EventLogConfiguration config = new EventLogConfiguration(logName);

                // Skip analytical and debug logs, just as the built-in Event Viewer does.
                if (config.LogType == EventLogType.Analytical || config.LogType == EventLogType.Debug)
                {
                    continue;
                }

                logList.Add(new EtwLogModel(logName));
            }
            catch (UnauthorizedAccessException)
            {
                // Some logs require elevation to read. Skip those.
            }
        }

        return logList;
    }).Value;

    private string name;
    private readonly List<LogModelEntry> entries = new List<LogModelEntry>();
    private readonly EventLogReader reader;
    private int totalEntriesRead = 0;

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
    public override IReadOnlyList<LogModelEntry> Read(int entryCount)
    {
        if (totalEntriesRead >= entries.Count)
        {
            // Easy out if we know there are no more events to read.
            return new List<LogModelEntry>();
        }

        List<LogModelEntry> retval = new List<LogModelEntry>();

        for (int i = 0; i < entryCount; i++)
        {
            int index = totalEntriesRead + i;
            if (index > entries.Count)
            {
                totalEntriesRead = index;
                return retval;
            }

            retval.Add(entries[index]);
        }

        totalEntriesRead += entryCount;
        return retval;
    }

    /// <inheritdoc/>
    public override Task PopulateAsync() => Task.Run(() =>
    {
        entries.Clear();
        totalEntriesRead = 0;

        reader.BatchSize = 512;

        while (true)
        {
            EventRecord? logEntry = reader.ReadEvent();

            if (logEntry == null) break;

            LogModelEntry modelEntry = new()
            {
                Source = logEntry.ProviderName,
                EventId = logEntry.Id,
                Message = logEntry.FormatDescription(),
                TimeStamp = logEntry.TimeCreated,

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

        entries.Reverse();
    });
}
