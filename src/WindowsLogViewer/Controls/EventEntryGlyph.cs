// Copyright (c) William Kent and contributors. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using WindowsLogViewer.Model;

namespace WindowsLogViewer.Controls
{
    /// <summary>
    /// Displays a small glyph depending on the a <see cref="LogEntrySeverity"/> value.
    /// </summary>
    internal sealed class EventEntryGlyph : UserControl
    {
        /// <summary>Identifies the <see cref="Symbol"/> dependency property.</summary>
        [SuppressMessage("WpfAnalyzers.DependencyProperty", "WPF0005:Name of PropertyChangedCallback should match registered name.", Justification = "Member name OnSymbolChanged already taken for instance method that is called by this method.")]
        public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(
            nameof(Symbol), typeof(LogEntrySeverity), typeof(EventEntryGlyph),
            new FrameworkPropertyMetadata(LogEntrySeverity.Unknown, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnSymbolChangedStatic));

        private readonly Ellipse childVisual;
        private readonly Style errorStyle;
        private readonly Style warningStyle;
        private readonly Style informationStyle;
        private readonly Style auditSuccessStyle;
        private readonly Style auditFailureStyle;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventEntryGlyph"/> class.
        /// </summary>
        public EventEntryGlyph()
        {
            Width = 24;
            Height = 24;

            childVisual = new Ellipse();
            childVisual.HorizontalAlignment = HorizontalAlignment.Center;
            childVisual.VerticalAlignment = VerticalAlignment.Center;
            AddChild(childVisual);

            LinearGradientBrush brush;

            errorStyle = new Style(typeof(Ellipse));
            errorStyle.Setters.Add(new Setter(Ellipse.WidthProperty, 16.0));
            errorStyle.Setters.Add(new Setter(Ellipse.HeightProperty, 16.0));

            brush = new LinearGradientBrush(Color.FromArgb(255, 0xF9, 0x34, 0x34), Color.FromArgb(255, 0xD0, 0x34, 0x34),
                new Point(0, 0), new Point(0, 1));
            errorStyle.Setters.Add(new Setter(Ellipse.FillProperty, brush));

            warningStyle = new Style(typeof(Ellipse));
            warningStyle.Setters.Add(new Setter(Ellipse.WidthProperty, 16.0));
            warningStyle.Setters.Add(new Setter(Ellipse.HeightProperty, 16.0));

            brush = new LinearGradientBrush(Color.FromArgb(255, 0xF9, 0xF1, 0x34), Color.FromArgb(255, 0xB9, 0xB3, 0x28),
                new Point(0, 0), new Point(0, 1));
            warningStyle.Setters.Add(new Setter(Ellipse.FillProperty, brush));

            informationStyle = new Style(typeof(Ellipse));
            informationStyle.Setters.Add(new Setter(Ellipse.WidthProperty, 10.0));
            informationStyle.Setters.Add(new Setter(Ellipse.HeightProperty, 10.0));

            brush = new LinearGradientBrush(Color.FromArgb(255, 0xB6, 0xFF, 0xF8), Color.FromArgb(255, 0x88, 0xD0, 0xCB),
                new Point(0, 0), new Point(0, 1));
            informationStyle.Setters.Add(new Setter(Ellipse.FillProperty, brush));

            auditSuccessStyle = new Style(typeof(Ellipse));
            auditSuccessStyle.Setters.Add(new Setter(Ellipse.WidthProperty, 12.0));
            auditSuccessStyle.Setters.Add(new Setter(Ellipse.HeightProperty, 12.0));

            brush = new LinearGradientBrush(Color.FromArgb(255, 0x77, 0xFF, 0x5A), Color.FromArgb(255, 0x56, 0xC8, 0x3E),
                new Point(0, 0), new Point(0, 1));
            auditSuccessStyle.Setters.Add(new Setter(Ellipse.FillProperty, brush));

            auditFailureStyle = new Style(typeof(Ellipse));
            auditFailureStyle.Setters.Add(new Setter(Ellipse.WidthProperty, 12.0));
            auditFailureStyle.Setters.Add(new Setter(Ellipse.HeightProperty, 12.0));

            brush = new LinearGradientBrush(Color.FromArgb(255, 0xED, 0x58, 0x58), Color.FromArgb(255, 0xBB, 0x40, 0x40),
                new Point(0, 0), new Point(0, 1));
            auditFailureStyle.Setters.Add(new Setter(Ellipse.FillProperty, brush));
        }

        /// <summary>
        /// Gets or sets the <see cref="LogEntrySeverity"/> to be displayed.
        /// </summary>
        public LogEntrySeverity Symbol
        {
            get => (LogEntrySeverity)GetValue(SymbolProperty);
            set => SetValue(SymbolProperty, value);
        }

        private static void OnSymbolChangedStatic(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            EventEntryGlyph glyph = (EventEntryGlyph)obj;
            glyph.OnSymbolChanged();
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Compound expression.")]
        private void OnSymbolChanged()
        {
            childVisual.SetCurrentValue(StyleProperty, Symbol switch
            {
                LogEntrySeverity.Unknown => null,
                LogEntrySeverity.Error => errorStyle,
                LogEntrySeverity.Warning => warningStyle,
                LogEntrySeverity.Informational => informationStyle,
                LogEntrySeverity.AuditSuccess => auditSuccessStyle,
                LogEntrySeverity.AuditFailure => auditFailureStyle,
                _ => throw new ArgumentException(nameof(Symbol))
            });

            if (childVisual.Style != null) childVisual.SetCurrentValue(VisibilityProperty, Visibility.Visible);
            else childVisual.SetCurrentValue(VisibilityProperty, Visibility.Hidden);
        }
    }
}
