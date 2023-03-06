// Copyright (c) William Kent and contributors. All rights reserved.

using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using WindowsLogViewer.Model;

namespace LogViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LogChooser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
            viewModel.ActiveSource = (LogSource)LogChooser.SelectedItem;
            viewModel.DisplayedEvents.Clear();
            viewModel.ReadEvents();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel viewModel = new MainWindowViewModel(Dispatcher);
            DataContext = viewModel;

            Task task = Task.Run(viewModel.PopulateSources);
            task.GetAwaiter().OnCompleted(() => LoadingProgressSpinner.SetCurrentValue(VisibilityProperty, Visibility.Collapsed));
        }

        private void MainScroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            MainWindowViewModel viewModel = (MainWindowViewModel)DataContext;
            if (viewModel == null) return;

            if (e.VerticalChange > 0)
            {
                if (e.VerticalOffset + e.ViewportHeight == e.ExtentHeight)
                {
                    viewModel.ReadEvents();
                }
            }
        }

        private bool IsUserVisible(FrameworkElement element, FrameworkElement container)
        {
            // This function is directly derived from the following Stack Overflow post:
            // https://stackoverflow.com/questions/1517743/in-wpf-how-can-i-determine-whether-a-control-is-visible-to-the-user

            if (!element.IsVisible)
                return false;

            Rect bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.Contains(bounds);
        }
    }
}
