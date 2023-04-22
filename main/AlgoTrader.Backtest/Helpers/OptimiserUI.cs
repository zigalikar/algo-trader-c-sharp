using System.Threading.Tasks;

using AlgoTrader.Backtest.Model;

namespace AlgoTrader.Backtest.Helpers
{
    public abstract class OptimiserUI : HTMLOutputHelper
    {
        protected abstract Task<OptimiserUIChildOutput> GetOutput();

        public async Task Run()
        {
            // get child output
            var output = await GetOutput();

            // write to file
            WriteToHTMLFile(output.Document, output.Title);
        }
    }
}
