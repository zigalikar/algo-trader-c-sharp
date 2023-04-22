using AlgoTrader.Core.Model.Http;

namespace AlgoTrader.Core.DTO
{
    /// <summary>
    /// Holds configuration data of an exchange
    /// </summary>
    public class ExchangeConfig
    {
        public string Name { get; set; }
        public HttpPair ApiKeys { get; set; }
        public HttpPair ApiSecrets { get; set; }
        public HttpPair Api { get; set; }
        public HttpPair Websocket { get; set; }
        public double MakerFee { get; set; }
        public double TakerFee { get; set; }
    }
}
