using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using AlgoTrader.Dashboard.Converters;
using Caliburn.Micro;
using System.Windows.Media;
using AlgoTrader.Core.Model;
using AlgoTrader.Core.Model.Orders;

namespace AlgoTrader.Dashboard.Views
{
    public partial class BacktestingView : ContentControl
    {
        private readonly Date2AxisConverter date2AxisConverter = new Date2AxisConverter();

        public BacktestingView()
        {
            InitializeComponent();
        }

        public void ClearChart() => Execute.OnUIThread(() =>
        {
            LineChart.Points = new PointCollection();
            CircleChart.Children.Clear();
        });

        private IList<(double x, double y, Color col)> _orders = new List<(double, double, Color)>();
        public void AddChartData(BacktestResultCollection data)
        {
            Execute.OnUIThread(() =>
            {
                // visualize orders
                var withOrders = data.Where(x => x.Orders.Any());
                foreach (var withOrder in withOrders)
                {
                    foreach (var order in withOrder.Orders.Where(x => x.Type == OrderTypeEnum.Market || x.Type == OrderTypeEnum.Limit))
                    {
                        var x = (double) date2AxisConverter.Convert(withOrder.Candlestick.CloseTime, typeof(double), null, null);
                        _orders.Add((x, withOrder.Profit, order.Side == OrderSideEnum.Sell ? Color.FromRgb(255, 0, 0) : Color.FromRgb(0, 255, 0)));
                        CircleChart.PlotColorSize(_orders.Select(e => e.x), _orders.Select(e => e.y), _orders.Select(e => e.col), 20);
                    }
                }

                var points = LineChart.Points.ToList();
                points.AddRange(data.Select(x => new Point((double) date2AxisConverter.Convert(x.Candlestick.CloseTime, typeof(double), null, null), x.Profit)));

                LineChart.Points = new PointCollection(points);
            });
        }

        private readonly Regex _regex = new Regex("[^0-9.-]+");
        private void StartingBalancePreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = IsNumber(e.Text) == false;

        private void StartingBalancePasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string) e.DataObject.GetData(typeof(string));
                if (!IsNumber(text))
                    e.CancelCommand();
            }
            else
                e.CancelCommand();
        }

        private bool IsNumber(string text) => !_regex.IsMatch(text);
    }
}
