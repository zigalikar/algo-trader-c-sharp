using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using AlgoTrader.Core.Model;
using AlgoTrader.Chart.Model;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Core.Model.Backtest;

using FancyCandles;

namespace AlgoTrader.Chart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IList<Candle> _prices = new List<Candle>();

        public MainWindow()
        {
            InitializeComponent();

            Backtest();
        }

        private async void Backtest()
        {
            var res = await BacktesterWrapper.Backtest();

            // charts
            foreach (var e in res.Segments)
            {
                // date
                var x = e.Data.CloseTime;

                // price
                var price = e.Data.ClosePrice;
                var lastPortfolioClose = _prices.Count > 0 ? _prices.Last().Portfolio.C : e.Balance.ClosePrice;
                _prices.Add(new Candle(x, e.Data.OpenPrice, e.Data.HighPrice, e.Data.LowPrice, e.Data.ClosePrice, (long) e.Data.Volume, new CandlePrices(lastPortfolioClose, Math.Max(lastPortfolioClose, e.Balance.ClosePrice), Math.Min(lastPortfolioClose, e.Balance.ClosePrice), e.Balance.ClosePrice, 1)));
                //_prices.Add(new Candle(x, e.Data.OpenPrice, e.Data.HighPrice, e.Data.LowPrice, e.Data.ClosePrice, (long) e.Data.Volume, new CandlePrices(0.2)));
            }

            // buy/sell orders
            foreach (var o in res.OrdersFilled)
            {
                var candle = _prices.First(p => p.t.Equals(o.Timestamp));
                candle.Order = new CandleOrderInfo(o.Side == OrderSide.Buy ? WholeContainerCandleOrderType.Buy : WholeContainerCandleOrderType.Sell, o.Price);
            }

            // plot charts
            Application.Current.Dispatcher.Invoke(() =>
            {
                PriceChart.CandlesSource = new ObservableCollection<ICandle>(_prices.ToList());
                //PriceChart.PortfolioSource = new ObservableCollection<ICandle>(_portfolio.ToList());

                //priceGraph.Plot(_prices.Select(p => p.x.ToUnix()), _prices.Select(p => p.y));
                //ordersGraph.PlotColor(_orders.Select(o => o.x.ToUnix()), _orders.Select(o => o.y), _orders.Select(o => o.c));
                //portfolioGraph.Plot(_portfolio.Select(p => p.x.ToUnix()), _portfolio.Select(p => p.y));
            });

            // plot MAs
            var data = res.GetAdditionalBacktestData();
            var sma20 = data[0] as BacktestData<IList<DateValuePair>>;
            var sma50 = data[1] as BacktestData<IList<DateValuePair>>;
            Application.Current.Dispatcher.Invoke(() =>
            {
                //sma20Graph.Plot(sma20.Data.Select(x => x.Date.ToUnix()), sma20.Data.Select(x => x.Value));
                //sma50Graph.Plot(sma50.Data.Select(x => x.Date.ToUnix()), sma50.Data.Select(x => x.Value));
            });
        }
    }
}
