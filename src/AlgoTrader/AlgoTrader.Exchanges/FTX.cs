using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;

using AlgoTrader.Feeds.Core;
using AlgoTrader.Core.Model;
using AlgoTrader.Core.DTO.FTX;
using AlgoTrader.Exchanges.Core;
using AlgoTrader.Core.Model.Http;
using AlgoTrader.Core.Extensions;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.Model.Attributes;
using AlgoTrader.Core.DTO.FTX.Websocket;

using Newtonsoft.Json;

namespace AlgoTrader.Exchanges
{
    [Exchange("ftx", typeof(CurrencyPairs), typeof(FTX))]
    public class FTX : ExchangeBase, IExchange
    {
        private string GetMethodName => "GET";
        private string PostMethodName => "POST";

        public FTX(ExchangeEnvironment environment = ExchangeEnvironment.Production) : base(environment)
        {
            _httpClient.SetHeader("FTX-KEY", APIKey);

            logger.Trace("Constructor");
        }

        public ICurrencyPair GetCurrencyPair(string value) => CurrencyPairs.FromString(value);

        public Task<IFeed<ICandlestick>> GetCandlestickFeed(ICurrencyPair currencyPair, TimeFrameEnum timeFrame) => Task.FromResult<IFeed<ICandlestick>>(null);

        public async Task<IFeed<Trade>> GetTradesFeed(ICurrencyPair market)
        {
            var ws = await GetWebsocket();
            return new FTXTradeFeed(ws, market);
        }

        public async Task<IFeed<Orderbook>> GetOrderbookFeed(ICurrencyPair market)
        {
            var ws = await GetWebsocket();
            return new FTXOrderbookFeed(ws, market);
        }

        public async Task<IList<ICandlestick>> GetPriceData(ICurrencyPair currencyPair, TimeFrameEnum timeFrame, int length)
        {
            try
            {
                var url = string.Format("{0}/markets/{1}/candles?resolution={2}&limit={3}", APIUrl, currencyPair.ToString(), timeFrame.ToTimeSpan().TotalSeconds, length + 1);
                var res = await _httpClient.GetAsync<HistoricalPricesResponse>(url);

                return res.Result.Select(x => new HistoricalPricesResponseDataWrapper(x, timeFrame)).Where(x => x.CloseTime < DateTime.UtcNow).ToList<ICandlestick>();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error in ${nameof(GetPriceData)}");
                return null;
            }
        }

        public Task<IOrder> MarketOrder(ICurrencyPair currencyPair, double amount, OrderSide side)
        {
            throw new NotImplementedException();
        }

        public Task<IOrder> StopMarketOrder(ICurrencyPair currencyPair, double amount, double stopPrice, OrderSide side)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CancelOrder(string id)
        {
            throw new NotImplementedException();
        }

        public Task CancelAllOrders()
        {
            throw new NotImplementedException();
        }

        //public Task<Orderbook> GetOrderbook(ICurrencyPair currencyPair, CancellationTokenSource cts = null)
        //{
        //    throw new NotImplementedException();
        //}

        #region Utils

        private FTXWebsocket _websocket;
        private async Task<FTXWebsocket> GetWebsocket()
        {
            if (_websocket == null)
            {
                _websocket = new FTXWebsocket(WebsocketUrl, GetWebsocketAuthenticationMessage);
                await _websocket.ConnectAsync();
            }

            return _websocket;
        }

        #region Protected
        
        protected override async Task GetStartingOpenOrders()
        {
            try
            {
                var requestPath = "/orders";
                var url = string.Format("{0}{1}", APIUrl, requestPath);
                var headers = GetRequestHeaders(DateTime.UtcNow, GetMethodName, requestPath);
                var res = await _httpClient.GetAsync<OrderResponse>(url, headers);

                _openOrders = res.Result.ToList<IOrder>();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error in ${nameof(GetStartingOpenOrders)}");
            }
        }

        protected override async Task GetStartingAvailableBalances()
        {
            try
            {
                var requestPath = "/wallet/balances";
                var url = string.Format("{0}{1}", APIUrl, requestPath);
                var headers = GetRequestHeaders(DateTime.UtcNow, GetMethodName, requestPath);
                var res = await _httpClient.GetAsync<BalancesResponse>(url, headers);

                var dict = new Dictionary<string, double>();
                foreach (var balance in res.Result)
                    dict[balance.Coin] = balance.Free;

                _availableBalances = dict;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error in ${nameof(GetStartingAvailableBalances)}");
            }
        }

        protected override async Task SetupUserDataFeed(Action<IWebSocketAccountUpdate> onAccountUpdate, Action<IOrder> onOrderUpdate)
        {
            var ws = await GetWebsocket();
            ws.OrderUpdate += (s, e) => onOrderUpdate.Invoke(e.Data);
        }

        #endregion

        /// <summary>
        /// Gets the required request headers for the API call
        /// </summary>
        /// <param name="requestTimestamp">Timestamp of the request in milliseconds since Unix epoch</param>
        /// <param name="httpMethod">HTTP method in uppercase</param>
        /// <param name="requestPath">Request path, including leading slash and any URL parameters but not including the hostname (e.g. /account)</param>
        /// <param name="requestBody">(POST only) Request body (JSON-encoded)</param>
        /// <returns>Request headers</returns>
        private IDictionary<string, string> GetRequestHeaders(DateTime requestTimestamp, string httpMethod, string requestPath, object requestBody = null)
        {
            var msFromUnixEpoch = requestTimestamp.ToMillisecondsFromEpoch().ToString();
            // get signature payload
            var signaturePayload = string.Format("{0}{1}/api{2}", msFromUnixEpoch, httpMethod, requestPath);
            if (requestBody != null)
                signaturePayload += JsonConvert.SerializeObject(requestBody);

            var signature = Sign(signaturePayload);
            return new Dictionary<string, string>
            {
                ["FTX-TS"] = msFromUnixEpoch,
                ["FTX-SIGN"] = signature
            };
        }

        private string Sign(string message)
        {
            var encoding = new ASCIIEncoding();
            var hash = new HMACSHA256(encoding.GetBytes(APISecret));
            return BitConverter.ToString(hash.ComputeHash(encoding.GetBytes(message))).Replace("-", "").ToLower();
        }

        private WebSocketAuthenticationMessage GetWebsocketAuthenticationMessage()
        {
            var ts = DateTime.UtcNow.ToMillisecondsFromEpoch();
            return new WebSocketAuthenticationMessage(new WebSocketAuthenticationArgs
            {
                Key = APIKey,
                Signature = Sign(string.Format("{0}websocket_login", ts)),
                Time = ts
            });
        }

        #region Feeds & websockets

        private class FTXTradeFeed : FeedBase<Trade>, IFeed<Trade>
        {
            private readonly FTXWebsocket _websocket;
            private readonly ICurrencyPair _currencyPair;

            public FTXTradeFeed(FTXWebsocket websocket, ICurrencyPair currencyPair)
            {
                _websocket = websocket;
                _currencyPair = currencyPair;
            }

            public Task Start()
            {
                _websocket.Trade -= OnWebsocketMessage;
                _websocket.Trade += OnWebsocketMessage;
                _websocket.SendSubscriptionMessage(WebSocketSubscriptionRequest.SubscribeTrades(_currencyPair));
                return Task.CompletedTask;
            }

            public Task Stop()
            {
                _websocket.Trade -= OnWebsocketMessage;
                _websocket.SendSubscriptionMessage(WebSocketSubscriptionRequest.UnsubscribeTrades(_currencyPair));
                return Task.CompletedTask;
            }
            
            private void OnWebsocketMessage(object sender, WebSocketMessage<List<WebSocketTradeMessageData>> e)
            {
                foreach (var t in e.Data)
                    EmitDataEvent(t.ToTrade(_currencyPair));
            }
        }

        private class FTXOrderbookFeed : FeedBase<Orderbook>, IFeed<Orderbook>
        {
            private readonly FTXWebsocket _websocket;
            private readonly ICurrencyPair _currencyPair;

            public FTXOrderbookFeed(FTXWebsocket websocket, ICurrencyPair currencyPair)
            {
                _websocket = websocket;
                _currencyPair = currencyPair;
            }

            public Task Start()
            {
                _websocket.Orderbook -= OnWebsocketMessage;
                _websocket.Orderbook += OnWebsocketMessage;
                _websocket.SendSubscriptionMessage(WebSocketSubscriptionRequest.SubscribeOrderbook(_currencyPair));
                return Task.CompletedTask;
            }

            public Task Stop()
            {
                _websocket.Orderbook -= OnWebsocketMessage;
                _websocket.SendSubscriptionMessage(WebSocketSubscriptionRequest.UnsubscribeOrderbook(_currencyPair));
                return Task.CompletedTask;
            }
            
            private void OnWebsocketMessage(object sender, WebSocketMessage<WebSocketOrderbookData> e) => EmitDataEvent(e.Data.ToOrderbook());
        }

        private class FTXWebsocket : AlgoTraderWebSocket
        {
            private EventHandler<WebSocketMessage<OrderResponseData>> _orderUpdate = delegate { };
            public event EventHandler<WebSocketMessage<OrderResponseData>> OrderUpdate { add => _orderUpdate += value; remove => _orderUpdate -= value; }

            private EventHandler<WebSocketMessage<List<WebSocketTradeMessageData>>> _trade = delegate { };
            public event EventHandler<WebSocketMessage<List<WebSocketTradeMessageData>>> Trade { add => _trade += value; remove => _trade -= value; }

            private EventHandler<WebSocketMessage<WebSocketOrderbookData>> _orderbook = delegate { };
            public event EventHandler<WebSocketMessage<WebSocketOrderbookData>> Orderbook { add => _orderbook += value; remove => _orderbook -= value; }

            private TimeSpan PingInterval => TimeSpan.FromSeconds(15);
            private readonly Timer _pingTimer;

            private readonly IList<(string channel, string market)> _subscriptions = new List<(string channel, string market)>();

            public FTXWebsocket(string url, Func<WebSocketAuthenticationMessage> sign) : base(url)
            {
                _pingTimer = new Timer((s) =>
                {
                    if (IsRunning)
                        Send("{\"op\": \"ping\"}");
                }, null, PingInterval, PingInterval);

                ConnectionHappened += (s, e) =>
                {
                    foreach (var (channel, market) in _subscriptions)
                        SendSubscriptionMessage(WebSocketSubscriptionRequest.GetSubscriptionMessage(channel, market));

                    Authenticate(sign.Invoke());
                };

                MessageReceived.Subscribe(async e =>
                {
                    if (e.MessageType == WebSocketMessageType.Text && string.IsNullOrWhiteSpace(e.Text) == false)
                    {
                        var json = JsonConvert.DeserializeObject<WebSocketMessage>(e.Text);
                        if (json.Type.Equals("info")) // restart websocket
                        {
                            logger.Info(string.Format("Websocket info message ({0}): {1}", json.ErrorCode, json.ErrorMessage));
                            if (json.ErrorCode.Value == 20001)
                            {
                                logger.Info("Reconnecting due to websocket request.");
                                await ReconnectAsync();
                                return;
                            }
                        }
                        else
                        {
                            if (json.ErrorCode.HasValue)
                            {
                                logger.Error(string.Format("Error in {0} ({1}): {2}", nameof(FTXWebsocket), json.ErrorCode.Value, json.ErrorMessage));
                                return;
                            }

                            if (json.Type.Equals("subscribed"))
                            {
                                var found = _subscriptions.ToList().FindIndex(x => x.channel == json.Channel);
                                if (found == -1)
                                    _subscriptions.Add((json.Channel, json.Market.ToString()));
                            }
                            else if (json.Type.Equals("unsubscribed"))
                            {
                                var found = _subscriptions.ToList().FindIndex(x => x.channel == json.Channel);
                                if (found != -1)
                                    _subscriptions.RemoveAt(found);
                            }
                            else
                            {
                                if (WebSocketChannels.Orders.Equals(json.Channel))
                                {
                                    var obj = json.CastData<OrderResponseData>();
                                     _orderUpdate.Invoke(this, obj);
                                }
                                else if (WebSocketChannels.Trades.Equals(json.Channel))
                                {
                                    var obj = json.CastData<List<WebSocketTradeMessageData>>();
                                    _trade.Invoke(this, obj);
                                }
                                else if (WebSocketChannels.Orderbook.Equals(json.Channel))
                                {
                                    var obj = json.CastData<WebSocketOrderbookData>();
                                    _orderbook.Invoke(this, obj);
                                }
                            }
                        }
                    }
                });
            }

            public void SendSubscriptionMessage(WebSocketSubscriptionRequest req) => Send(JsonConvert.SerializeObject(req));

            public void Authenticate(WebSocketAuthenticationMessage req)
            {
                logger.Debug("Authenticating websocket");
                Send(JsonConvert.SerializeObject(req));
            }
        }

        #endregion

        #endregion
    }
}
