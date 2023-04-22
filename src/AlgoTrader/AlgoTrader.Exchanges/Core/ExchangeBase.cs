using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using AlgoTrader.Core.Model;
using AlgoTrader.Core.Model.Http;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.Model.Attributes;

namespace AlgoTrader.Exchanges.Core
{
    /// <summary>
    /// Base class for any exchange
    /// </summary>
    public abstract class ExchangeBase
    {
        protected readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected string APIKey { get; private set; }
        protected string APISecret { get; private set; }
        protected string APIUrl { get; private set; }
        protected string WebsocketUrl { get; private set; }

        protected AlgoTraderHttpClient _httpClient;

        protected IList<IOrder> _openOrders = new List<IOrder>();
        public IList<IOrder> OpenOrders => _openOrders.ToList();

        protected IDictionary<string, double> _availableBalances = new Dictionary<string, double>();
        public IDictionary<string, double> AvailableBalances => new Dictionary<string, double>(_availableBalances);

        private EventHandler<IOrder> _orderPlaced = delegate { };
        public event EventHandler<IOrder> OrderPlaced { add => _orderPlaced += value; remove => _orderPlaced -= value; }

        private EventHandler<IOrder> _orderFilled = delegate { };
        public event EventHandler<IOrder> OrderFilled { add => _orderFilled += value; remove => _orderFilled -= value; }

        private EventHandler<IOrder> _orderCancelled = delegate { };
        public event EventHandler<IOrder> OrderCancelled { add => _orderCancelled += value; remove => _orderCancelled -= value; }

        protected abstract Task GetStartingOpenOrders();
        protected abstract Task GetStartingAvailableBalances();
        protected abstract Task SetupUserDataFeed(Action<IWebSocketAccountUpdate> onAccountUpdate, Action<IOrder> onOrderUpdate);

        /// <summary>
        /// Initiates a new instance of an exchange
        /// </summary>
        public ExchangeBase(ExchangeEnvironment environment = ExchangeEnvironment.Production)
        {
            var attr = GetType().GetCustomAttribute<Exchange>();
            if (attr != null)
            {
                _httpClient = new AlgoTraderHttpClient();

                var config = attr.GetConfig();
                var prod = environment == ExchangeEnvironment.Production;
                APIKey = prod ? config.ApiKeys.Production : config.ApiKeys.Testnet;
                APISecret = prod ? config.ApiSecrets.Production : config.ApiSecrets.Testnet;
                APIUrl = prod ? config.Api.Production : config.Api.Testnet;
                WebsocketUrl = prod ? config.Websocket.Production : config.Websocket.Testnet;
            }

            logger.Trace("Constructor");
        }

        protected bool _initialized;
        public async Task Initialize()
        {
            if (_initialized)
                return;
            _initialized = true;

            // initialize open orders
            await GetStartingOpenOrders();

            // initialize wallet balances
            await GetStartingAvailableBalances();

            // user data stream feed - listening to account balance changes, order changes, ...
            await SetupUserDataFeed(a =>
            {
                foreach (var asset in a.Balances)
                    _availableBalances[asset.Asset] = asset.Free;
            }, o =>
            {
                if (o.Status == OrderStatus.PartiallyFilled)
                {
                    // TODO: update fills
                }

                if (o.Status == OrderStatus.Filled || o.Status == OrderStatus.Expired)
                {
                    // remove from open orders
                    var found = _openOrders.FirstOrDefault(e => e.Id.Equals(o.Id));
                    if (found != null)
                        _openOrders.Remove(found);

                    if (o.Status == OrderStatus.Filled)
                        EmitOrderFilled(o);
                }
                else if (o.Status == OrderStatus.New || o.Status == OrderStatus.Cancelled)
                {
                    // add to open orders
                    var found = _openOrders.FirstOrDefault(e => e.Id.Equals(o.Id));
                    if (found == null)
                        _openOrders.Add(o);

                    if (o.Status == OrderStatus.Cancelled)
                        EmitOrderCancelled(o);
                    else if (o.Status == OrderStatus.New)
                        EmitOrderPlaced(o);
                }
            });
        }

        protected void EmitOrderPlaced(IOrder order)
        {
            logger.Debug(string.Format("Placed order with ID {0}", order.Id));
            _orderPlaced.Invoke(this, order);
        }

        protected void EmitOrderFilled(IOrder order)
        {
            logger.Debug(string.Format("Filled order with ID {0}", order.Id));
            _orderFilled.Invoke(this, order);
        }

        protected void EmitOrderCancelled(IOrder order)
        {
            logger.Debug(string.Format("Cancelled order with ID {0}", order.Id));
            _orderCancelled.Invoke(this, order);
        }
    }
}
