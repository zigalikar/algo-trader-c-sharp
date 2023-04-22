using AlgoTrader.Core.Model.Backtest;

namespace AlgoTrader.Core.Interfaces
{
    public interface IBacktestExchange : IExchange
    {
        BacktestResult GetResult(IBacktestAlgo algo);
    }
}
