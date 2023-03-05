// Copyright (c) William Kent and contributors. All rights reserved.

using System.Globalization;
using System.Windows.Data;

namespace WindowsLogViewer.Controls
{
    /// <summary>
    /// A value converter that converts <see cref="DateTime"/> instances to strings.
    /// </summary>
    internal class DateTimeToStringConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the format string to be passed to <see cref="DateTime.ToString(string?)"/>.
        /// </summary>
        public string? Format { get; set; } = null;

        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string)) throw new ArgumentException("Output type not string", nameof(targetType));

            if (value is DateTime date)
            {
                return date.ToString(Format);
            }
            else
            {
                throw new ArgumentException("Input object not DateTime", nameof(value));
            }
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
