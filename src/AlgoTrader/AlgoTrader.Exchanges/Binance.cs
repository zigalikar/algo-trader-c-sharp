using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;

using AlgoTrader.Core.Model;
using AlgoTrader.Feeds.Core;
using AlgoTrader.Exchanges.Core;
using AlgoTrader.Core.Model.Http;
using AlgoTrader.Core.Extensions;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.DTO.Binance;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.Model.Attributes;
using AlgoTrader.Core.Extensions.Binance;
using AlgoTrader.Core.DTO.Binance.Websocket;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AlgoTrader.Exchanges
{
    [Exchange("binance", typeof(CurrencyPairs), typeof(Binance))]
    public class Binance : ExchangeBase, IExchange
    {
        private Timer listenKeyRenewTimer;
        private TimeSpan ListenKeyRenewTimerPeriod => TimeSpan.FromMinutes(30);

        public Binance(ExchangeEnvironment environment = ExchangeEnvironment.Production) : base(environment)
        {
            _httpClient.SetHeader("X-MBX-APIKEY", APIKey);

            logger.Trace("Constructor");
        }

        public ICurrencyPair GetCurrencyPair(string value) => CurrencyPairs.FromString(value);

        public async Task<IFeed<ICandlestick>> GetCandlestickFeed(ICurrencyPair currencyPair, TimeFrameEnum timeFrame)
        {
            var ws = await GetWebsocket();
            return new BinanceCandlestickFeed(ws, currencyPair, timeFrame);
        }

        //public IFeed<Trade> GetTradesFeed(ICurrencyPair currencyPair) => new BinanceTradeDataWebsocket(string.Format("{0}/{1}@trade", WebsocketUrl, currencyPair.ToString().ToLower()));
        public Task<IFeed<Trade>> GetTradesFeed(ICurrencyPair market) => throw new NotImplementedException();

        public Task<IFeed<Orderbook>> GetOrderbookFeed(ICurrencyPair market) => throw new NotImplementedException();

        public async Task<IList<ICandlestick>> GetPriceData(ICurrencyPair currencyPair, TimeFrameEnum timeFrame, int length)
        {
            try
            {
                var url = string.Format("{0}/klines?symbol={1}&interval={2}&limit={3}", APIUrl, currencyPair.ToString(), timeFrame.ToBinanceTimeFrameInterval(), length + 1);
                var res = await _httpClient.GetAsync<List<List<object>>>(url);

                var list = res.Take(res.Count - 1).Select(x => CandlestickDataResponse.FromArrayResponse(x)).ToList();
                return list.Select(x => x as ICandlestick).ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error in ${nameof(GetPriceData)}");
                return null;
            }
        }

        public async Task<IOrder> MarketOrder(ICurrencyPair currencyPair, double amount, OrderSide side)
        {
            try
            {
                var queryString = string.Format("symbol={0}&side={1}&type={2}&quantity={3}&timestamp={4}", currencyPair.ToString(), side.ToBinanceOrderSide(), OrderType.Market.ToBinanceOrderType(), amount, DateTime.UtcNow.ToMillisecondsFromEpoch());
                var url = string.Format("{0}/order?{1}&signature={2}", APIUrl, queryString, Sign(queryString));
                var res = await _httpClient.PostAsync<OrderResponse>(url, null);
                return res;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error in ${nameof(MarketOrder)}");
                return null;
            }
        }

        public Task<IOrder> StopMarketOrder(ICurrencyPair currencyPair, double amount, double stopPrice, OrderSide side)
        {
            throw new NotImplementedException();
        }

        public async Task<IOrder> LimitOrder(ICurrencyPair currencyPair, double amount, double price, OrderSide side)
        {
            try
            {
                var queryString = string.Format("symbol={0}&side={1}&type={2}&quantity={3}&timestamp={4}&timeInForce=GTC&price={5}", currencyPair.ToString(), side.ToBinanceOrderSide(), OrderType.Limit.ToBinanceOrderType(), amount, DateTime.UtcNow.ToMillisecondsFromEpoch(), price);
                var url = string.Format("{0}/order?{1}&signature={2}", APIUrl, queryString, Sign(queryString));
                var res = await _httpClient.PostAsync<OrderResponse>(url, null);
                return res;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error in ${nameof(LimitOrder)}");
                return null;
            }
        }

        public Task<bool> CancelOrder(string id)
        {
            throw new NotImplementedException();
        }

        public Task CancelAllOrders()
        {
            throw new NotImplementedException();
        }

        //public async Task<Orderbook> GetOrderbook(ICurrencyPair currencyPair, CancellationTokenSource cts = null)
        //{
        //    try
        //    {
        //        var url = string.Format("{0}/depth?symbol={1}", APIUrl, currencyPair.ToString());
        //        var ob = await _httpClient.GetAsync<OrderbookResponse>(url);
        //        return ob.ToOrderbook();
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, string.Format("Error in {0}", nameof(GetOrderbook)));
        //        return null;
        //    }
        //}

        #region Utils

        private BinanceWebsocket _websocket;
        private async Task<BinanceWebsocket> GetWebsocket()
        {
            if (_websocket == null)
            {
                _websocket = new BinanceWebsocket(WebsocketUrl);
                await _websocket.ConnectAsync();
            }

            return _websocket;
        }

        #region Protected

        protected override async Task GetStartingOpenOrders()
        {
            try
            {
                var queryString = string.Format("timestamp={0}", DateTime.UtcNow.ToMillisecondsFromEpoch());
                var url = string.Format("{0}/openOrders?{1}&signature={2}", APIUrl, queryString, Sign(queryString));
                var res = await _httpClient.GetAsync<List<OrderResponse>>(url);

                _openOrders = res.ToList<IOrder>();
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
                var queryString = string.Format("timestamp={0}", DateTime.UtcNow.ToMillisecondsFromEpoch());
                var url = string.Format("{0}/account?{1}&signature={2}", APIUrl, queryString, Sign(queryString));
                var res = await _httpClient.GetAsync<AccountInformationResponse>(url);

                var dict = new Dictionary<string, double>();
                foreach (var balance in res.Balances)
                    dict[balance.Asset] = balance.Free;

                _availableBalances = dict;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error in ${nameof(GetStartingAvailableBalances)}");
            }
        }

        protected override Task SetupUserDataFeed(Action<IWebSocketAccountUpdate> onAccountUpdate, Action<IOrder> onOrderUpdate)
        {
            // TODO
            return Task.CompletedTask;
            //// generate listen key & start renew timer
            //var listenKey = await GenerateListenKey();
            //var listenKeyRenewUrl = string.Format("{0}/userDataStream?listenKey={1}", APIUrl, listenKey);
            //listenKeyRenewTimer = new Timer(async s =>
            //{
            //    try
            //    {
            //        await _httpClient.PutAsync(listenKeyRenewUrl);
            //        logger.Debug(string.Format("Renewed listenKey {0}", listenKey));
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error(ex, string.Format("Failed renewing listenKey {0}", listenKey));
            //    }
            //}, null, ListenKeyRenewTimerPeriod, ListenKeyRenewTimerPeriod);

            //// user data stream websocket - listening to account balance changes, order changes, ...
            //return new BinanceUserDataWebsocket(string.Format("{0}/{1}", WebsocketUrl, listenKey), onAccountUpdate, onOrderUpdate);
        }

        #endregion

        /// <summary>
        /// Sign the message for Binance API
        /// </summary>
        /// <param name="queryString">Query string of the URL</param>
        /// <param name="requestBody">Body of the request</param>
        /// <returns>Signature</returns>
        private string Sign(string queryString, string requestBody = null)
        {
            var totalParams = queryString;
            if (string.IsNullOrWhiteSpace(requestBody) == false)
                totalParams += requestBody;
            
            var encoding = new ASCIIEncoding();
            var hash = new HMACSHA256(encoding.GetBytes(APISecret));
            var signature = BitConverter.ToString(hash.ComputeHash(encoding.GetBytes(totalParams))).Replace("-", "").ToLower();
            return signature;
        }

        private async Task<string> GenerateListenKey()
        {
            try
            {
                var url = string.Format("{0}/userDataStream", APIUrl);
                var res = await _httpClient.PostAsync<object, ListenKeyGenerateResponse>(url, null);
                return res.ListenKey;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Error in {0}", nameof(GenerateListenKey)));
                return null;
            }
        }

        #region Feeds & websockets

        private class BinanceCandlestickFeed : FeedBase<ICandlestick>, IFeed<ICandlestick>
        {
            private readonly BinanceWebsocket _websocket;
            private readonly ICurrencyPair _market;
            private readonly TimeFrameEnum _timeFrame;

            public BinanceCandlestickFeed(BinanceWebsocket websocket, ICurrencyPair market, TimeFrameEnum timeFrame)
            {
                _websocket = websocket;
                _market = market;
                _timeFrame = timeFrame;
            }

            public Task Start()
            {
                _websocket.Candlestick -= OnWebsocketMessage;
                _websocket.Candlestick += OnWebsocketMessage;
                _websocket.SendSubscriptionMessage(WebSocketSubscriptionRequest.SubscribeCandlesticks(_market, _timeFrame));
                return Task.CompletedTask;
            }

            public Task Stop()
            {
                _websocket.Candlestick -= OnWebsocketMessage;
                _websocket.SendSubscriptionMessage(WebSocketSubscriptionRequest.UnsubscribeCandlesticks(_market, _timeFrame));
                return Task.CompletedTask;
            }

            private void OnWebsocketMessage(object sender, WebSocketMessage<WebSocketCandlestickData> e)
            {
                if (e.Data.Closed)
                    EmitDataEvent(e.Data.ToCandlestickDataResponse());
            }
        }

        private class BinanceWebsocket : AlgoTraderWebSocket
        {
            private EventHandler<WebSocketMessage<WebSocketCandlestickData>> _candlestick = delegate { };
            public event EventHandler<WebSocketMessage<WebSocketCandlestickData>> Candlestick { add => _candlestick += value; remove => _candlestick -= value; }
            
            private readonly IList<(string channel, string market)> _subscriptions = new List<(string channel, string market)>();

            public BinanceWebsocket(string url/*, Func<WebSocketAuthenticationMessage> sign*/) : base(url)
            {
                ConnectionHappened += (s, e) =>
                {
                    //foreach (var (channel, market) in _subscriptions)
                    //    SendSubscriptionMessage(WebSocketSubscriptionRequest.GetSubscriptionMessage(channel, market));

                    //Authenticate(sign.Invoke());
                };

                MessageReceived.Subscribe(async e =>
                {
                    if (e.MessageType == WebSocketMessageType.Text && string.IsNullOrWhiteSpace(e.Text) == false)
                    {
                        var jobj = JObject.Parse(e.Text);
                        if (jobj.TryGetValue("e",  out JToken _))
                        {
                            var json = jobj.ToObject<WebSocketMessage>();
                            if (json.EventType.Equals("kline"))
                                _candlestick.Invoke(this, JsonConvert.DeserializeObject<WebSocketMessage<WebSocketCandlestickData>>(e.Text));
                        }
                    }
                });
            }

            public void SendSubscriptionMessage(WebSocketSubscriptionRequest req) => Send(JsonConvert.SerializeObject(req));

            //public void Authenticate(WebSocketAuthenticationMessage req)
            //{
            //    logger.Debug("Authenticating websocket");
            //    Send(JsonConvert.SerializeObject(req));
            //}
        }

        //private class BinancePriceDataWebsocket : DataWebsocket<ICandlestick>
        //{
        //    public BinancePriceDataWebsocket(string url) : base(url)
        //    {
        //        MessageReceived.Subscribe(e =>
        //        {
        //            if (e.MessageType == WebSocketMessageType.Text && string.IsNullOrWhiteSpace(e.Text) == false)
        //            {
        //                var data = JsonConvert.DeserializeObject<WebSocketMessage<WebSocketCandlestickData>>(e.Text);
        //                if (data.Data.Closed)
        //                    EmitDataEvent(data.Data.ToCandlestickDataResponse());
        //            }
        //        });
        //    }
        //}

        //private class BinanceTradeDataWebsocket : DataWebsocket<Trade>
        //{
        //    public BinanceTradeDataWebsocket(string url) : base(url)
        //    {
        //        MessageReceived.Subscribe(e =>
        //        {
        //            if (e.MessageType == WebSocketMessageType.Text && string.IsNullOrWhiteSpace(e.Text) == false)
        //            {
        //                var data = JsonConvert.DeserializeObject<WebSocketTradeData>(e.Text);
        //                EmitDataEvent(data.ToTrade());
        //            }
        //        });
        //    }
        //}

        //private class BinanceUserDataWebsocket : AlgoTraderWebSocket
        //{
        //    private static object messageLock = new object();
        //    private static object debouncingLock = new object();

        //    private readonly Timer _debounceTimer;
        //    private TimeSpan _debounceTimeSpan => TimeSpan.FromMilliseconds(200);
        //    private readonly List<WebSocketUserDataStream> _debounceObjects = new List<WebSocketUserDataStream>();

        //    public BinanceUserDataWebsocket(string url, Action<WebSocketAccountUpdate> onAccountUpdate, Action<WebSocketOrderUpdate> onOrderUpdate) : base(url)
        //    {
        //        _debounceTimer = new Timer(s =>
        //        {
        //            lock (debouncingLock)
        //            {
        //                _debounceObjects.Sort((a, b) => a.EventTime.CompareTo(b.EventTime));
        //                foreach (var e in _debounceObjects)
        //                {
        //                    if (e is WebSocketAccountUpdate a)
        //                        onAccountUpdate.Invoke(a);
        //                    else if (e is WebSocketOrderUpdate o)
        //                        onOrderUpdate.Invoke(o);
        //                    // TODO: other types
        //                }
        //                _debounceObjects.Clear();
        //            }
        //        }, null, -1, -1);

        //        MessageReceived.Subscribe(e =>
        //        {
        //            lock (messageLock)
        //            {
        //                if (e.MessageType == WebSocketMessageType.Text && string.IsNullOrWhiteSpace(e.Text) == false)
        //                {
        //                    var jobj = JObject.Parse(e.Text);
        //                    var type = jobj.Value<string>("e").ToLower();
        //                    WebSocketUserDataStream data = null;
        //                    if (WebSocketUserDataStream.AccountUpdateType.Equals(type))
        //                        data = jobj.ToObject<WebSocketAccountUpdate>();
        //                    else if (WebSocketUserDataStream.OrderUpdateType.Equals(type))
        //                        data = jobj.ToObject<WebSocketOrderUpdate>();
        //                    else if (WebSocketUserDataStream.OrderUpdateOCOType.Equals(type))
        //                        data = jobj.ToObject<WebSocketOrderUpdateOCO>();

        //                    if (data != null)
        //                    {
        //                        _debounceTimer.Change(_debounceTimeSpan, Timeout.InfiniteTimeSpan);
        //                        _debounceObjects.Add(data);
        //                    }
        //                }
        //            }
        //        });
        //    }
        //}

        #endregion

        #endregion
    }
}
