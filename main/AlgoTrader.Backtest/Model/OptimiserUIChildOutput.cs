using HtmlAgilityPack;

namespace AlgoTrader.Backtest.Model
{
    public class OptimiserUIChildOutput
    {
        public HtmlDocument Document { get; set; }
        public string Title { get; set; }

        public OptimiserUIChildOutput(HtmlDocument document, string title)
        {
            Document = document;
            Title = title;
        }
    }
}
