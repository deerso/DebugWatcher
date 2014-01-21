using System.Windows;
using System.Windows.Controls;
using DebugWatcher.ViewModels;
using ReactiveUI;

namespace DebugWatcher
{
    /// <summary>
    /// Interaction logic for MainScreenView.xaml
    /// </summary>
    public partial class MainScreenView : IViewFor<IMainScreenViewModel>
    {
        public MainScreenView()
        {
            InitializeComponent();
            //this.OneWayBind(ViewModel, x => x.RequestOutputMessages, x => x.RequestsOutputBox.ItemsSource);
            this.OneWayBind(ViewModel, x => x.DebugOutputMessages, x => x.DebugOutputBox.ItemsSource);
            this.OneWayBind(ViewModel, x => x.ExceptionOutputMessages, x => x.ExceptionsOutputBox.ItemsSource);
            this.BindCommand(ViewModel, x => x.ConnectCommand, x => x.ConnectButton);
            this.BindCommand(ViewModel, x => x.DisconnectCommand, x => x.DisconnectButton);
            this.OneWayBind(ViewModel, x => x.RequestInfoList, x => x.RequestsGrid.ItemsSource);
            this.OneWayBind(ViewModel, x => x.LatestStatusMessage, x => x.LatestStatusText.Text);
            this.OneWayBind(ViewModel, x => x.OrderInfoList, x => x.OrdersGrid.ItemsSource);
            this.Bind(ViewModel, x => x.CrawlersFilter, x => x.ShowCrawlers.IsChecked);
            this.Bind(ViewModel, x => x.RootRequestsFilter, x => x.OnlyRootRequests.IsChecked);
            this.BindCommand(ViewModel, x => x.ClearFiltersCommand, x => x.ClearFiltersButton);
        }
        
        public static DependencyProperty ViewModelProperty =
       DependencyProperty.Register("ViewModel", typeof(IMainScreenViewModel), typeof(MainScreenView),
           new PropertyMetadata(null));

        public IMainScreenViewModel ViewModel
        {
            get { return (IMainScreenViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (MainScreenViewModel)value; }
        }

        private void ChannelsTabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}
