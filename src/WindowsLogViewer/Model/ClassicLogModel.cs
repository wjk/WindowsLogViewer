// Copyright (c) William Kent and contributors. All rights reserved.

using System.Diagnostics;

namespace WindowsLogViewer.Model;

/// <summary>
/// Represents the "classic" Windows XP-era Windows Event Log.
/// </summary>
internal sealed class ClassicLogModel : BaseLogModelSource, IDisposable
{
    /// <summary>
    /// The Application log.
    /// </summary>
    public static readonly ClassicLogModel ApplicationLog = new ClassicLogModel("Application");

    /// <summary>
    /// The Security log.
    /// </summary>
    public static readonly ClassicLogModel SecurityLog = new ClassicLogModel("Security");

    /// <summary>
    /// The Setup log.
    /// </summary>
    public static readonly ClassicLogModel SetupLog = new ClassicLogModel("Setup");

    /// <summary>
    /// The System log.
    /// </summary>
    public static readonly ClassicLogModel SystemLog = new ClassicLogModel("System");

    private readonly string name;
    private readonly EventLog logReader;
    private readonly List<LogModelEntry> modelEntries = new List<LogModelEntry>();
    private int totalEntriesRead = 0;

    private ClassicLogModel(string logName)
    {
        logReader = new EventLog(logName);
        name = logName;
    }

    /// <inheritdoc/>
    public override IReadOnlyList<LogModelEntry> Entries => modelEntries;

    /// <inheritdoc/>
    public override string LogName => name;

    /// <inheritdoc/>
    public void Dispose()
    {
        logReader.Dispose();
    }

    /// <inheritdoc/>
    public override IReadOnlyList<LogModelEntry> Read(int entryCount)
    {
        List<LogModelEntry> entries = new List<LogModelEntry>();

        for (int i = 0; i < entryCount; i++)
        {
            EventLogEntry entry = logReader.Entries[totalEntriesRead + i];

            try
            {
                LogModelEntry modelEntry = new()
                {
                    Message = entry.Message,
                    EventId = entry.InstanceId,
                    Source = entry.Source,

                    Severity = entry.EntryType switch
                    {
                        EventLogEntryType.SuccessAudit => LogEntrySeverity.AuditSuccess,
                        EventLogEntryType.FailureAudit => LogEntrySeverity.AuditFailure,
                        EventLogEntryType.Error => LogEntrySeverity.Error,
                        EventLogEntryType.Warning => LogEntrySeverity.Warning,
                        EventLogEntryType.Information => LogEntrySeverity.Informational,
                        _ => throw new ArgumentException("Unknown EventLogEntryType")
                    },
                };

                entries.Add(modelEntry);
            }
            finally
            {
                entry.Dispose();
            }
        }

        totalEntriesRead += entryCount;
        return entries;
    }

    /// <inheritdoc/>
    public override Task PopulateAsync() => Task.Run(() =>
    {
        EventLogEntryCollection allEvents = logReader.Entries;
        int count = allEvents.Count;

        for (int i = count; i >= 0; i--)
        {
            EventLogEntry entry;

            try
            {
                entry = allEvents[i];
            }
            catch
            {
                // Error occurred while retrieving an event; skip it and move on to the next one
                continue;
            }

            try
            {
                LogModelEntry modelEntry = new()
                {
                    Message = entry.Message,
                    EventId = Convert.ToInt32(entry.InstanceId),
                    Source = entry.Source,

                    Severity = entry.EntryType switch
                    {
                        EventLogEntryType.SuccessAudit => LogEntrySeverity.AuditSuccess,
                        EventLogEntryType.FailureAudit => LogEntrySeverity.AuditFailure,
                        EventLogEntryType.Error => LogEntrySeverity.Error,
                        EventLogEntryType.Warning => LogEntrySeverity.Warning,
                        EventLogEntryType.Information => LogEntrySeverity.Informational,
                        _ => throw new ArgumentException("Unknown EventLogEntryType")
                    },
                };

                modelEntries.Add(modelEntry);
            }
            finally
            {
                entry.Dispose();
            }
        }
    });
}
