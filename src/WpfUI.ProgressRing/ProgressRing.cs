﻿// Copyright (c) William Kent and contributors. All rights reserved.

// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

// https://docs.microsoft.com/en-us/fluent-ui/web-components/components/progress-ring

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;

namespace WpfUI;

/// <summary>
/// Rotating loading ring.
/// </summary>
[ToolboxItem(true)]
public class ProgressRing : System.Windows.Controls.Control
{
    /// <summary>
    /// Property for <see cref="Progress"/>.
    /// </summary>
    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
        nameof(Progress),
        typeof(double), typeof(ProgressRing),
        new PropertyMetadata(50d, OnProgressChanged));

    /// <summary>
    /// Property for <see cref="IsIndeterminate"/>.
    /// </summary>
    public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register(
        nameof(IsIndeterminate),
        typeof(bool), typeof(ProgressRing),
        new PropertyMetadata(false));

    /// <summary>
    /// Property for <see cref="EngAngle"/>.
    /// </summary>
    public static readonly DependencyProperty EngAngleProperty = DependencyProperty.Register(
        nameof(EngAngle),
        typeof(double), typeof(ProgressRing),
        new PropertyMetadata(180.0d));

    /// <summary>
    /// Property for <see cref="IndeterminateAngle"/>.
    /// </summary>
    public static readonly DependencyProperty IndeterminateAngleProperty = DependencyProperty.Register(
        nameof(IndeterminateAngle),
        typeof(double), typeof(ProgressRing),
        new PropertyMetadata(180.0d));

    /// <summary>
    /// Property for <see cref="CoverRingStroke"/>.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Line too long otherwise")]
    public static readonly DependencyProperty CoverRingStrokeProperty =
        DependencyProperty.RegisterAttached(
            nameof(CoverRingStroke),
            typeof(Brush),
            typeof(ProgressRing),
            new FrameworkPropertyMetadata(
                Brushes.Black,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender |
                FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>
    /// Property for <see cref="CoverRingVisibility"/>.
    /// </summary>
    public static readonly DependencyProperty CoverRingVisibilityProperty = DependencyProperty.Register(
        nameof(CoverRingVisibility),
        typeof(System.Windows.Visibility), typeof(ProgressRing),
        new PropertyMetadata(System.Windows.Visibility.Visible));

    /// <summary>
    /// Gets or sets the progress.
    /// </summary>
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="ProgressRing"/> shows actual values (<see langword="false"/>)
    /// or generic, continuous progress feedback (<see langword="true"/>).
    /// </summary>
    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>
    /// Gets or sets the <see cref="Arc.EndAngle"/>.
    /// </summary>
    public double EngAngle
    {
        get => (double)GetValue(EngAngleProperty);
        set => SetValue(EngAngleProperty, value);
    }

    /// <summary>
    /// Gets the <see cref="Arc.EndAngle"/> when <see cref="IsIndeterminate"/> is <see langword="true"/>.
    /// </summary>
    public double IndeterminateAngle
    {
        get => (double)GetValue(IndeterminateAngleProperty);
        internal set => SetValue(IndeterminateAngleProperty, value);
    }

    /// <summary>
    /// Gets background ring fill.
    /// </summary>
    public Brush CoverRingStroke
    {
        get => (Brush)GetValue(CoverRingStrokeProperty);
        internal set => SetValue(CoverRingStrokeProperty, value);
    }

    /// <summary>
    /// Gets background ring visibility.
    /// </summary>
    public Visibility CoverRingVisibility
    {
        get => (Visibility)GetValue(CoverRingVisibilityProperty);
        internal set => SetValue(CoverRingVisibilityProperty, value);
    }

    /// <summary>
    /// Re-draws <see cref="Arc.EndAngle"/> depending on <see cref="Progress"/>.
    /// </summary>
    protected void UpdateProgressAngle()
    {
        var percentage = Progress;

        if (percentage > 100)
            percentage = 100;

        if (percentage < 0)
            percentage = 0;

        // (360 / 100) * percentage
        var endAngle = 3.6d * percentage;

        if (endAngle >= 360)
            endAngle = 359;

        SetCurrentValue(EngAngleProperty, endAngle);
    }

    /// <summary>
    /// Validates the entered <see cref="Progress"/> and redraws the <see cref="Arc"/>.
    /// </summary>
    /// <param name="d">
    /// The <see cref="ProgressRing"/> object that was changed.
    /// </param>
    /// <param name="e">
    /// A <see cref="DependencyPropertyChangedEventArgs"/> object describing the change.
    /// </param>
    protected static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ProgressRing control)
            return;

        control.UpdateProgressAngle();
    }
}