using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

using Websocket.Client;

namespace AlgoTrader.Core.Model.Http
{
    public abstract class AlgoTraderWebSocket : WebsocketClient
    {
        protected static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private EventHandler _connectionHappened = delegate { };
        public event EventHandler ConnectionHappened { add => _connectionHappened += value; remove => _connectionHappened -= value; }

        public AlgoTraderWebSocket(string url, bool respondToPings = true) : base(new Uri(url))
        {
            ConnectionHappened += (s, e) => logger.Trace(nameof(ConnectionHappened));

            DisconnectionHappened.Subscribe(async e =>
            {
                if (e.Exception != null)
                    logger.Error(e.Exception, string.Format("{0} {1}", GetType().ToString(), nameof(DisconnectionHappened)));
                
                if (e.CloseStatus == WebSocketCloseStatus.NormalClosure)
                {
                    logger.Trace(string.Format("{0} NORMAL CLOSURE {1}", GetType().ToString(), nameof(DisconnectionHappened)));
                    return;
                }

                if (string.IsNullOrWhiteSpace(e.CloseStatusDescription) == false && e.CloseStatusDescription.Equals("reconnecting") == false)
                {
                    logger.Info(string.Format("[ATTEMPTING RECONNECT] {0} {1}: ({2}) {3}", GetType().ToString(), nameof(DisconnectionHappened), e.CloseStatus, e.CloseStatusDescription));
                    await ReconnectAsync();
                }
            });
        }

        public async Task ConnectAsync()
        {
            try
            {
                await StartOrFail();
                if (IsRunning)
                    _connectionHappened.Invoke(this, null);
            }
            catch (System.Exception ex)
            {
                logger.Error(ex, string.Format("{0} {1}", GetType().ToString(), nameof(ConnectAsync)));
            }
        }

        public async Task ReconnectAsync()
        {
            try
            {
                if (IsStarted || IsRunning)
                    await StopOrFail(WebSocketCloseStatus.NormalClosure, "reconnecting");

                logger.Info(string.Format("{0} {1}", GetType().ToString(), nameof(ReconnectAsync)));
                while (IsRunning == false)
                {
                    try
                    {
                        await ConnectAsync();
                        if (IsRunning == false)
                        {
                            var delay = TimeSpan.FromSeconds(5);
                            logger.Info(string.Format("Failed to reconnect, retrying in {0} seconds.", delay.TotalSeconds));
                            await Task.Delay(delay); // wait delay before retrying
                        }
                    }
                    catch (System.Exception ex)
                    {
                        logger.Error(ex, string.Format("{0} {1} during CONNECT", GetType().ToString(), nameof(ReconnectAsync)));
                    }
                }
            }
            catch (System.Exception ex)
            {
                logger.Error(ex, string.Format("{0} {1}", GetType().ToString(), nameof(ReconnectAsync)));
            }
        }
    }
}
