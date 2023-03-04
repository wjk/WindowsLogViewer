// Copyright (c) William Kent and contributors. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<BaseLogModelSource> sources = new ObservableCollection<BaseLogModelSource>
            {
                ClassicLogModel.ApplicationLog,
                ClassicLogModel.SecurityLog,
                ClassicLogModel.SetupLog,
                ClassicLogModel.SystemLog,
            };

            DataContext = sources;

            Task.Run(() =>
            {
                foreach (var source in EtwLogModel.AllLogs)
                    Dispatcher.InvokeAsync(() => sources.Add(source));
            });
        }
    }
}
