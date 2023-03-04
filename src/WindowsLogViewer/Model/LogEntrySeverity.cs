// Copyright (c) William Kent and contributors. All rights reserved.

namespace WindowsLogViewer.Model;

public enum LogEntrySeverity
{
    /// <summary>
    /// An error.
    /// </summary>
    Error,

    /// <summary>
    /// A warning message.
    /// </summary>
    Warning,

    /// <summary>
    /// An informational message.
    /// </summary>
    Informational,

    /// <summary>
    /// A message that may be useful during debugging.
    /// </summary>
    Trace,

    /// <summary>
    /// An action completed successfully. Used only in <see cref="ClassicLogModel.SecurityLog"/>.
    /// </summary>
    AuditSuccess,

    /// <summary>
    /// An action was denied. Used only in <see cref="ClassicLogModel.SecurityLog"/>.
    /// </summary>
    AuditFailure,

    /// <summary>
    /// The log level was not one of these known values.
    /// </summary>
    /// <remarks>
    /// In ETW logs, the identifiers of the log levels are arbitrary and defined by the logging subsystem.
    /// I must compare the string name of the level to known strings (which, of course, will break on
    /// non-English Windows installations). If the log level was not recognized, this severity will be used.
    /// </remarks>
    Unknown,
}
