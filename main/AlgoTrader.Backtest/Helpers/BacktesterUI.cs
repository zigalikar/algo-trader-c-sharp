using System.Threading.Tasks;

using AlgoTrader.Backtest.Model;

namespace AlgoTrader.Backtest.Helpers
{
    public abstract class BacktesterUI : HTMLOutputHelper
    {
        protected abstract Task<BacktesterUIChildOutput> GetOutput();

        public async Task Run()
        {
            // get child output
            var output = await GetOutput();

            // append overall statistics
            var document = output.Document;
            var overallStatistics = document.GetElementbyId("overall-statistics-details");
            if (overallStatistics != null)
            {
                foreach (var item in output.OverallStatistics)
                {
                    var key = document.CreateElement("div");
                    key.AddClass("col-3");
                    key.InnerHtml = item.Key;
                    overallStatistics.AppendChild(key);

                    var value = document.CreateElement("div");
                    value.AddClass("col-3");
                    value.InnerHtml = item.Value;
                    overallStatistics.AppendChild(value);
                }
            }
            
            // write to file
            WriteToHTMLFile(document, output.Title);
        }
    }

    public class BacktesterUIStatisticsItem
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public BacktesterUIStatisticsItem(string key, object value)
        {
            Key = key;
            Value = value.ToString();
        }
    }
}
