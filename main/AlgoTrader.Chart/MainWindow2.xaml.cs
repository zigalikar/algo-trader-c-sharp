using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using AlgoTrader.Chart.Model;

using FancyCandles;

namespace AlgoTrader.Chart
{
    public partial class MainWindow2 : Window
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IList<Candle> _prices = new List<Candle>();

        public MainWindow2()
        {
            InitializeComponent();

            Backtest();
        }

        private async void Backtest()
        {
            var res = await BacktesterWrapper.Backtest();
            
            foreach (var e in res.Segments)
            {
                // date
                var x = e.Data.CloseTime;

                // price
                var lastPortfolioClose = _prices.Count > 0 ? _prices.Last().C : e.Balance.ClosePrice;

                // closes
                _prices.Add(new Candle(x, lastPortfolioClose, Math.Max(lastPortfolioClose, e.Balance.ClosePrice), Math.Min(lastPortfolioClose, e.Balance.ClosePrice), e.Balance.ClosePrice, 1));

                //// ticks
                //_prices.Add(new Candle(x, e.Balance.OpenPrice, e.Balance.HighPrice, e.Balance.LowPrice, e.Balance.ClosePrice, 1));
            }

            // statistics
            var statistics = new List<StatisticsItem>
            {
                new StatisticsItem("Total trades", res.TotalTrades),
                new StatisticsItem("Winners", res.Winners),
                new StatisticsItem("Losers", res.Losers),
                new StatisticsItem("Average win", res.AverageWin),
                new StatisticsItem("Average loss", res.AverageLoss),
                new StatisticsItem("Max consecutive wins", res.MaxConsecutiveWins),
                new StatisticsItem("Max consecutive losses", res.MaxConsecutiveLosses),
                new StatisticsItem("Max system drawdown", res.MaxSystemDrawdown),
                new StatisticsItem("Max system drawdown [%]", res.MaxSystemDrawdownPercentage),
                new StatisticsItem("Max system drawdown duration", res.MaxSystemDrawdownDuration),
                new StatisticsItem("Recovery factor", res.RecoveryFactor)
            };

            // run on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                PriceChart.CandlesSource = new ObservableCollection<ICandle>(_prices.ToList());

                var row = 1;
                var col = 0;
                foreach (var i in statistics)
                {
                    // create key label
                    var keyLabel = new Label
                    {
                        Content = i.Key,
                        VerticalAlignment = VerticalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    // create value label
                    var valueLabel = new Label
                    {
                        Content = i.Value,
                        VerticalAlignment = VerticalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center
                    };

                    // set grid positioning
                    Grid.SetRow(keyLabel, row);
                    Grid.SetColumn(keyLabel, col);
                    Grid.SetRow(valueLabel, row);
                    Grid.SetColumn(valueLabel, col + 1);

                    // add row definitions
                    if (row >= StatisticsGrid.RowDefinitions.Count)
                        StatisticsGrid.RowDefinitions.Add(new RowDefinition { Height = StatisticsGrid.RowDefinitions[StatisticsGrid.RowDefinitions.Count - 1].Height });

                    // add to grid
                    StatisticsGrid.Children.Add(keyLabel);
                    StatisticsGrid.Children.Add(valueLabel);

                    // change grid position
                    col += 2;
                    if (col >= StatisticsGrid.ColumnDefinitions.Count)
                    {
                        col = 0;
                        row++;
                    }
                }
            });
        }

        private class StatisticsItem
        {
            public string Key { get; set; }
            public string Value { get; set; }

            public StatisticsItem(string key, object value)
            {
                Key = key;
                Value = value.ToString();
            }
        }
    }
}
