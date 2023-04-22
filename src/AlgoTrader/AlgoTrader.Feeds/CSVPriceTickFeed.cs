using AlgoTrader.Feeds.Core;
using AlgoTrader.Feeds.Helpers;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Attributes;

namespace AlgoTrader.Feeds
{
    /// <summary>
    /// Feed that reads tick price data from a CSV file
    /// </summary>
    [BacktestFeed]
    public class CSVPriceTickFeed : CSVFeedBase<IPriceTick>, IFeed<IPriceTick>
    {
        public CSVPriceTickFeed(string path, CSVDataReaderType type = CSVDataReaderType.Folder) : base(new CSVDataReader<IPriceTick>(path, new CSVDataPriceTickDecoder(), type)) { }
    }
}
