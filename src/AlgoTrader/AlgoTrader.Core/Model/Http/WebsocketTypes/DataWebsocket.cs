using System;

namespace AlgoTrader.Core.Model.Http.WebsocketTypes
{
    /// <summary>
    /// Websocket with subscribable data event
    /// </summary>
    /// <typeparam name="T">Data type that is emitted</typeparam>
    public class DataWebsocket<T> : AlgoTraderWebSocket where T : class
    {
        private EventHandler<T> _data = delegate { };
        public event EventHandler<T> Data { add => _data += value; remove => _data -= value; }

        public DataWebsocket(string url) : base(url) { }

        protected void EmitDataEvent(T data) => _data.Invoke(this, data);
    }
}
