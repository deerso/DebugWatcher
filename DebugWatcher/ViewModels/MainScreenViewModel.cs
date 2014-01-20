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
        string LatestStatusMessage { get; set; }

    }
    public class MainScreenViewModel : ReactiveObject, IMainScreenViewModel
    {

        private int currentIndex = 0;
        public MainScreenViewModel(IScreen screen)
        {
            HostScreen = screen;

            var redis = new DeersoRedisClient();

            RequestInfoList = new ReactiveList<RequestInfo>();

            OrderInfoList = new ReactiveList<OrderInfo>();

            RequestOutputMessages = new ReactiveList<string>();
            DebugOutputMessages = new ReactiveList<string>();
            ExceptionOutputMessages = new ReactiveList<string>();

            var requestSubject = new Subject<string>();
            var debugSubject = new Subject<string>();
            var exceptionSubject = new Subject<string>();
            var ordersSubject = new Subject<string>();

            requestSubject.ObserveOnDispatcher().Subscribe(x =>
            {
                try
                {
                    var requestInfo = JsonSerializer.DeserializeFromString<RequestInfo>(x);
                    RequestInfoList.Add(requestInfo);
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
        }

        public string UrlPathSegment
        {
            get { return "MainScreen"; }
        }

        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get { return _selectedTabIndex; }
            set { this.RaiseAndSetIfChanged(ref _selectedTabIndex, value); }
        }
        private Brush _exceptionForeground;
        public Brush ExceptionForeground
        {
            get { return _exceptionForeground; }
            set { this.RaiseAndSetIfChanged(ref _exceptionForeground, value); }
        }
        private Brush _debugForeground;
        public Brush DebugForeground
        {
            get { return _debugForeground; }
            set { this.RaiseAndSetIfChanged(ref _debugForeground, value); }
        }
        private Brush _requestsForeground;
        public Brush RequestsForeground
        {
            get { return _requestsForeground; }
            set { this.RaiseAndSetIfChanged(ref _requestsForeground, value); }
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

        private IReactiveList<string> _exceptionOutputMessages;

        public IReactiveList<string> ExceptionOutputMessages
        {
            get { return _exceptionOutputMessages; }
            set { this.RaiseAndSetIfChanged(ref _exceptionOutputMessages, value); }
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

        public IReactiveCommand AddMessageCommand { get; set; }
        public IReactiveCommand ConnectCommand { get; set; }
        public IReactiveCommand DisconnectCommand { get; set; }

        public IReactiveCommand ProductionRedisAddressCommand { get; set; }
        public IReactiveCommand StagingRedisAddressCommand { get; set; }
        public IScreen HostScreen { get; protected set; }
    }
}