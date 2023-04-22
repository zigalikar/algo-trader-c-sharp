using System;
using System.Linq;
using System.Collections.Generic;

using AlgoTrader.Core.Model.Order;

using Newtonsoft.Json;

namespace AlgoTrader.Core.JsonConverters.Binance
{
    public class OrderStatusConverter : JsonConverter
    {
        private IDictionary<string, OrderStatus> _orderStatusDictionary => new Dictionary<string, OrderStatus>
        {
            { "filled", OrderStatus.Filled },
            { "partially_filled", OrderStatus.PartiallyFilled },
            { "expired", OrderStatus.Expired },
            { "new", OrderStatus.New },
            { "canceled", OrderStatus.Cancelled }
        };

        public override bool CanConvert(Type objectType) => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteRawValue(_orderStatusDictionary.Keys.ElementAt(_orderStatusDictionary.ToList().FindIndex(x =>  x.Value == (OrderStatus) value)));
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var val = (reader.Value as string).ToLower();
            if (_orderStatusDictionary.ContainsKey(val))
                return _orderStatusDictionary[val];

            throw new NotImplementedException();
        }
    }

    public class OrderTypeConverter : JsonConverter
    {
        private IDictionary<string, OrderType> _orderTypeDictionary => new Dictionary<string, OrderType>
        {
            { "limit", OrderType.Limit },
            { "market", OrderType.Market },
            { "stop_loss", OrderType.StopMarket },
            { "stop_loss_limit", OrderType.StopLimit }
        };

        public override bool CanConvert(Type objectType) => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteRawValue(_orderTypeDictionary.Keys.ElementAt(_orderTypeDictionary.ToList().FindIndex(x => x.Value == (OrderType) value)));
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var val = (reader.Value as string).ToLower();
            if (_orderTypeDictionary.ContainsKey(val))
                return _orderTypeDictionary[val];

            throw new NotImplementedException();
        }
    }

    public class OrderSideConverter : JsonConverter
    {
        private IDictionary<string, OrderSide> _orderSideDictionary => new Dictionary<string, OrderSide>
        {
            { "buy", OrderSide.Buy },
            { "sell", OrderSide.Sell }
        };

        public override bool CanConvert(Type objectType) => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteRawValue(_orderSideDictionary.Keys.ElementAt(_orderSideDictionary.ToList().FindIndex(x => x.Value == (OrderSide) value)));
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var val = (reader.Value as string).ToLower();
            if (_orderSideDictionary.ContainsKey(val))
                return _orderSideDictionary[val];

            throw new NotImplementedException();
        }
    }
}
