using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using Deerso.Logging.Models;
using Microsoft.Expression.Interactivity.Media;
using ServiceStack;
using ServiceStack.Redis;

namespace DebugWatcher
{
    public class DeersoRedisClient
    {
        private readonly Func<string, bool> _isUnsubscribeAllMessage; 
        protected string ConnectedIpAddress { get; set; }
        public DeersoRedisClient()
        {
            _isUnsubscribeAllMessage = s =>
            {
                var unsubscribeFormat = MessageTypeToStringMap[MessageType.UnsubscribeFromAll];
                return s == unsubscribeFormat.FormatWith(App.ApplicationId);
            };
        }

        public void Disconnect()
        {
            if (ConnectedIpAddress.IsEmpty())
                return;
            var redisClient = new RedisClient(ConnectedIpAddress);
            redisClient.PublishMessage("DeersoWebDebug", MessageTypeToStringMap[MessageType.UnsubscribeFromAll].FormatWith(App.ApplicationId));
        }
        public void Connect(string serverIpAddress,
            ISubject<string> debugSubject,
            ISubject<string> exceptionSubject,
            ISubject<string> requestSubject,
            ISubject<string> ordersSubject, 
            ISubject<string> onConnection)
        {
            //137.135.99.146
            try
            {
                using (var redisConsumer = new RedisClient(serverIpAddress))
                using (var _redisSubscription = redisConsumer.CreateSubscription())
                {
                    ConnectedIpAddress = serverIpAddress;
                    _redisSubscription.OnSubscribe = channel =>
                    {
                        var msg = "Subscribed to {0} on {1}".FormatWith(channel, serverIpAddress);

                        switch (channel)
                        {
                            case "DeersoWebDebug":
                                onConnection.OnNext("DeersoWebDebug");
                                debugSubject.OnNext(msg);
                                break;
                            case "DeersoWebExceptions":
                                exceptionSubject.OnNext(msg);
                                break;
                            case "DeersoWebRequests":
                                requestSubject.OnNext(msg);
                                break;
                            case "DeersoWebOrders":
                                ordersSubject.OnNext(msg);
                                break;
                        }
                    };
                    _redisSubscription.OnUnSubscribe = channel =>
                    {
                        var msg = "Unsubscribed from {0}".FormatWith(channel);
                        switch (channel)
                        {
                            case "DeersoWebDebug":
                                debugSubject.OnNext(msg);
                                break;
                            case "DeersoWebExceptions":
                                exceptionSubject.OnNext(msg);
                                break;
                            case "DeersoWebRequests":
                                requestSubject.OnNext(msg);
                                break;
                            case "DeersoWebOrders":
                                ordersSubject.OnNext(msg);
                                break;
                        }
                    };

                    _redisSubscription.OnMessage = (channel, msg) =>
                    {
                       if (_isUnsubscribeAllMessage(msg))
                            _redisSubscription.UnSubscribeFromAllChannels();

                        if (msg.IndexOf("[UNSUBSCRIBE-ALL]") == 0)
                            return;
                        switch (channel)
                        {
                            case "DeersoWebDebug":
                                debugSubject.OnNext(msg);
                                break;
                            case "DeersoWebExceptions":
                                exceptionSubject.OnNext(msg);
                                break;
                            case "DeersoWebRequests":
                                requestSubject.OnNext(msg);
                                break;
                            case "DeersoWebOrders":
                                ordersSubject.OnNext(msg);
                                break;
                        }
                    };
                    App.SubscriptionThread = new Thread(() =>
                    {
                        try
                        {
                            _redisSubscription.SubscribeToChannels(
                                "DeersoWebDebug",
                                "DeersoWebExceptions",
                                "DeersoWebRequests",
                                "DeersoWebOrders");
                        }
                        catch (RedisException ex)
                        {
                             onConnection.OnError(ex);
                        }
                        
                    });
                    App.SubscriptionThread.Start();
                }
            }
            catch (Exception x)
            {
                debugSubject.OnNext("Error: " + x.Message);
            }

        }

  

        public Dictionary<string, MessageType> StringToMessageTypeMap = new Dictionary<string, MessageType>
        {
            {"[UNSUBSCRIBE-ALL][{0}]", MessageType.UnsubscribeFromAll}
        };

        public Dictionary<MessageType, string> MessageTypeToStringMap = new Dictionary<MessageType, string>
        {
            {MessageType.UnsubscribeFromAll, "[UNSUBSCRIBE-ALL][{0}]"}
        };

        public enum MessageType
        {
            Normal,
            UnsubscribeFromAll
        }
    }
}