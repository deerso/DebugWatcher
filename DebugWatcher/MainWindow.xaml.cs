using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
using DebugWatcher.ViewModels;
using Microsoft.Expression.Interactivity.Core;
using ServiceStack;
using ServiceStack.Redis;

namespace DebugWatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public AppBootstrapper AppBootstrapper { get; protected set; }
        public MainWindow()
        {
            InitializeComponent();

          AppBootstrapper = new AppBootstrapper();
            DataContext = AppBootstrapper;
            this.Closing += MainWindow_Closing;
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ShowOptionsStateButton(object sender, RoutedEventArgs e)
        {
            ExtendedVisualStateManager.GoToElementState(this.MainContentGrid as FrameworkElement, "ShowOptions", true);
        }

        private void UseStagingServerAddressButton_OnClick(object sender, RoutedEventArgs e)
        {
            DebugWatcher.Properties.Settings.Default.RedisAddress = "137.135.99.146";
        }

        private void UseProductionServerAddressButton_OnClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.RedisAddress = "10.0.1.50";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            base.OnClosing(e);
        }
    }
}
