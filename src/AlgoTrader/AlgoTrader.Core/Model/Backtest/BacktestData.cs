namespace AlgoTrader.Core.Model.Backtest
{
    public class BacktestData<T> : BacktestData where T : class
    {
        /// <summary>
        /// Data of the data series
        /// </summary>
        public T Data { get; private set; }

        public BacktestData(string name, T data) : base(name)
        {
            Data = data;
        }
    }

    public class BacktestData
    {
        /// <summary>
        /// Name of the data series
        /// </summary>
        public string Name { get; private set; }

        public BacktestData(string name)
        {
            Name = name;
        }
    }
}
