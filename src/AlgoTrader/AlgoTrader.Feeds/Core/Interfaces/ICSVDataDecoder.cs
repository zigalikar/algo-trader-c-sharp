namespace AlgoTrader.Feeds.Helpers
{
    public interface ICSVDataDecoder<T> where T : class
    {
        T DecodeLine(string line);
    }
}
