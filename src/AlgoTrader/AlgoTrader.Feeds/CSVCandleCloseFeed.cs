using AlgoTrader.Feeds.Core;
using AlgoTrader.Feeds.Helpers;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Attributes;

namespace AlgoTrader.Feeds
{
    /// <summary>
    /// Feed that reads candlestick price data from a CSV file
    /// </summary>
    [BacktestFeed]
    public class CSVCandleCloseFeed : CSVFeedBase<ICandlestick>, IFeed<ICandlestick>
    {
        public CSVCandleCloseFeed(string path, CSVDataReaderType type = CSVDataReaderType.File) : base(new CSVDataReader<ICandlestick>(path, new CSVDataCandleDecoder(), type)) { }
    }
}
