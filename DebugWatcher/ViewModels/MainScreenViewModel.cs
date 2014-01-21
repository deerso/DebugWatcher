using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Deerso.Logging.Models;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using ServiceStack;
using ServiceStack.Redis;
using ServiceStack.Text;

namespace DebugWatcher.ViewModels
{
    public interface IMainScreenViewModel : IRoutableViewModel
    {
        IReactiveList<string> RequestOutputMessages { get; set; } 
        IReactiveList<string> DebugOutputMessages { get; set; } 
        IReactiveList<string> ExceptionOutputMessages { get; set; }
        IReactiveList<RequestInfo> RequestInfoList { get; set; } 
        IReactiveList<OrderInfo> OrderInfoList { get; set; } 
        IReactiveCommand AddMessageCommand { get; set; }
        IReactiveCommand ConnectCommand { get; set; }
        IReactiveCommand DisconnectCommand { get; set; }
        IReactiveCommand ClearFiltersCommand { get; set; }
        string LatestStatusMessage { get; set; }
        IReactiveList<RequestInfo> AllRequests { get; set; } 
        bool? CrawlersFilter { get; set; }
        bool? RootRequestsFilter { get; set; }

    }
    public class MainScreenViewModel : ReactiveObject, IMainScreenViewModel
    {

        private int currentIndex = 0;
        private Func<RequestInfo, bool> _crawlerPredicate = null;
        private Func<RequestInfo, bool> _rootRequestPredicate = null;

        private void SetRequstInfo()
        {
            IEnumerable<RequestInfo> requests = AllRequests;

            if (_crawlerPredicate != null)
                requests = requests.Where(_crawlerPredicate);

            if (_rootRequestPredicate != null)
                requests = requests.Where(_rootRequestPredicate);

            RequestInfoList = new ReactiveList<RequestInfo>(requests);
        }

        public MainScreenViewModel(IScreen screen)
        {
            HostScreen = screen;

            AllRequests = new ReactiveList<RequestInfo>();
            RequestInfoList = new ReactiveList<RequestInfo>();

            OrderInfoList = new ReactiveList<OrderInfo>();

            RequestOutputMessages = new ReactiveList<string>();
            DebugOutputMessages = new ReactiveList<string>();
            ExceptionOutputMessages = new ReactiveList<string>();

            var requestSubject = new Subject<string>();
            var debugSubject = new Subject<string>();
            var exceptionSubject = new Subject<string>();
            var ordersSubject = new Subject<string>();

 
            this.ObservableForProperty(x => x.CrawlersFilter).ObserveOnDispatcher().Subscribe(x =>
            {
                if (x.Value == null)
                    _crawlerPredicate = null;
                else if (x.Value.Value)
                    _crawlerPredicate = r => r.IsCrawler;
                else
                    _crawlerPredicate = r => !r.IsCrawler;
                SetRequstInfo();
            });
            this.ObservableForProperty(x => x.RootRequestsFilter).ObserveOnDispatcher().Subscribe(x =>
            {
                if (x.Value == null)
                    _rootRequestPredicate = null;
                else if (x.Value.Value)
                    _rootRequestPredicate = r => r.IsLocal;
                else
                    _rootRequestPredicate = r => !r.IsLocal;
                SetRequstInfo();
            });

            requestSubject.ObserveOnDispatcher().Subscribe(x =>
            {
                try
                {
                    var requestInfo = JsonSerializer.DeserializeFromString<RequestInfo>(x);
                    AllRequests.Add(requestInfo);
                    SetRequstInfo();
                }
                catch (Exception)
                {

                }
            });

            ordersSubject.ObserveOnDispatcher().Subscribe(x =>
            {
                try
                {
                    var ordersInfo = JsonSerializer.DeserializeFromString<OrderInfo>(x);
                    OrderInfoList.Add(ordersInfo);
                }
                catch (Exception)
                {
                }
            });

            debugSubject.ObserveOnDispatcher().Subscribe(x => DebugOutputMessages.Add(x));
            exceptionSubject.ObserveOnDispatcher().Subscribe(x => ExceptionOutputMessages.Add(x));


            InitalizeCommands(new DeersoRedisClient(), debugSubject, exceptionSubject, requestSubject, ordersSubject);
        }


        void InitalizeCommands(DeersoRedisClient deersoRedis, 
            ISubject<string> debugsubject, 
            ISubject<string> exceptionSubject, 
            ISubject<string> requestSubject, 
            ISubject<string> ordersSubject)
        {
            DisconnectCommand = new ReactiveCommand();
            DisconnectCommand.ObserveOnDispatcher().Subscribe(x =>
            {
                deersoRedis.Disconnect();
                LatestStatusMessage = "Disconnected from Server";
            });

            ConnectCommand = new ReactiveCommand();
            ConnectCommand.ObserveOnDispatcher().Subscribe(x =>
            {
                var connectionCallback = new Subject<string>();
                connectionCallback.ObserveOnDispatcher().Subscribe(_ =>
                {
                    LatestStatusMessage = "Successfully Connected and Subscribed";
                });
                connectionCallback.Catch<string, RedisException>(ex =>
                {
                    LatestStatusMessage = ex.Message;
                    return Observable.Return("");
                });
                deersoRedis.Connect(Properties.Settings.Default.RedisAddress, 
                    debugsubject, 
                    exceptionSubject,
                    requestSubject, 
                    ordersSubject,
                    connectionCallback);
                LatestStatusMessage = "Connecting to: {0}".FormatWith(Properties.Settings.Default.RedisAddress);
            });

            ClearFiltersCommand = new ReactiveCommand();
            ClearFiltersCommand.Subscribe(x =>
            {
                _rootRequestPredicate = null;
                _crawlerPredicate = null;
                CrawlersFilter = null;
                RootRequestsFilter = null;
            });
        }

        public string UrlPathSegment
        {
            get { return "MainScreen"; }
        }

       

        private string _redisServerAddress;

        public string RedisServerAddress
        {
            get { return _redisServerAddress; }
            set { this.RaiseAndSetIfChanged(ref _redisServerAddress, value); }
        }

        private IReactiveList<string> _debugOutputMessages;
        public IReactiveList<string> DebugOutputMessages
        {
            get { return _debugOutputMessages; }
            set { this.RaiseAndSetIfChanged(ref _debugOutputMessages, value); }
        }

        private IReactiveList<RequestInfo> _allRequests;

        public IReactiveList<RequestInfo> AllRequests
        {
            get { return _allRequests; }
            set { this.RaiseAndSetIfChanged(ref _allRequests, value); }
        }
        private IReactiveList<string> _exceptionOutputMessages;

        public IReactiveList<string> ExceptionOutputMessages
        {
            get { return _exceptionOutputMessages; }
            set { this.RaiseAndSetIfChanged(ref _exceptionOutputMessages, value); }
        }

        private bool? _rootRequestsFilter;
        public bool? RootRequestsFilter
        {
            get { return _rootRequestsFilter; }
            set { this.RaiseAndSetIfChanged(ref _rootRequestsFilter, value); }
        }
        private bool? _crawlersFilter;
        public bool? CrawlersFilter
        {
            get { return _crawlersFilter; }
            set { this.RaiseAndSetIfChanged(ref _crawlersFilter, value); }
        }
        private string _latestStatusMessage;
        public string LatestStatusMessage
        {
            get { return _latestStatusMessage; }
            set { this.RaiseAndSetIfChanged(ref _latestStatusMessage, value); }
        }

        private IReactiveList<string> _requestOutputMessages; 
        public IReactiveList<string> RequestOutputMessages
        {
            get { return _requestOutputMessages; } 
            set { this.RaiseAndSetIfChanged(ref _requestOutputMessages, value); }
        }

        private IReactiveList<RequestInfo> _requestInfoList;

        public IReactiveList<RequestInfo> RequestInfoList
        {
            get { return _requestInfoList; }
            set { this.RaiseAndSetIfChanged(ref _requestInfoList, value); }
        }

        private IReactiveList<OrderInfo> _orderInfoList;
        public IReactiveList<OrderInfo> OrderInfoList
        {
            get { return _orderInfoList; }
            set { this.RaiseAndSetIfChanged(ref _orderInfoList, value); }
        }

        public IReactiveCommand ClearFiltersCommand { get; set; }
        public IReactiveCommand AddMessageCommand { get; set; }
        public IReactiveCommand ConnectCommand { get; set; }
        public IReactiveCommand DisconnectCommand { get; set; }

        public IReactiveCommand ProductionRedisAddressCommand { get; set; }
        public IReactiveCommand StagingRedisAddressCommand { get; set; }
        public IScreen HostScreen { get; protected set; }
    }
}