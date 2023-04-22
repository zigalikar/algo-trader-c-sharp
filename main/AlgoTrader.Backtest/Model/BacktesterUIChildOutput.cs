using System.Collections.Generic;

using AlgoTrader.Backtest.Helpers;

using HtmlAgilityPack;

namespace AlgoTrader.Backtest.Model
{
    public class BacktesterUIChildOutput
    {
        public HtmlDocument Document { get; set; }
        public string Title { get; set; }
        public IList<BacktesterUIStatisticsItem> OverallStatistics { get; set; }

        public BacktesterUIChildOutput(HtmlDocument document, string title, IList<BacktesterUIStatisticsItem> overallStatistics)
        {
            Document = document;
            Title = title;
            OverallStatistics = overallStatistics;
        }
    }
}
