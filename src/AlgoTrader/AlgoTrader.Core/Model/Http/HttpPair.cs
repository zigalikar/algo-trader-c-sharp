namespace AlgoTrader.Core.Model.Http
{
    public class HttpPair
    {
        public string Production { get; }
        public string Testnet { get; }

        public HttpPair(string production, string testnet)
        {
            Production = production;
            Testnet = testnet;
        }
    }
}
