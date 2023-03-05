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

        PopulateAsync().GetAwaiter().GetResult();
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
        if (totalEntriesRead >= modelEntries.Count)
        {
            // Easy out if we know there are no more events to read.
            return new List<LogModelEntry>();
        }

        List<LogModelEntry> retval = new List<LogModelEntry>();

        for (int i = 0; i < entryCount; i++)
        {
            int index = totalEntriesRead + i;
            if (index > modelEntries.Count)
            {
                totalEntriesRead = index;
                return retval;
            }

            retval.Add(modelEntries[index]);
        }

        totalEntriesRead += entryCount;
        return retval;
    }

    /// <inheritdoc/>
    public override Task PopulateAsync() => Task.Run(() =>
    {
        modelEntries.Clear();
        totalEntriesRead = 0;

        EventLogEntryCollection allEvents = logReader.Entries;
        int count;

        try
        {
            count = allEvents.Count;
        }
        catch
        {
            // Sometimes, this log cannot be loaded. When this happens, the above line will throw.
            return;
        }

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
                if (entry.EntryType == 0)
                {
                    // I have no idea how this could happen, but it causes the severity determination below to crash.
                    continue;
                }

                LogModelEntry modelEntry = new()
                {
                    Message = entry.Message,
                    EventId = entry.InstanceId,
                    Source = entry.Source,
                    TimeStamp = entry.TimeGenerated,

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

            modelEntries.Reverse();
        }
    });
}
