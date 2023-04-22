using AlgoTrader.Dashboard.Converters;
using System.Windows.Controls;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TwitterAnalyser.Core.DTO;
using TwitterAnalyser.Core.Model;
using Caliburn.Micro;
using InteractiveDataDisplay.WPF;

namespace AlgoTrader.Dashboard.Views
{
    public partial class TwitterView : ContentControl
    {
        private readonly Date2AxisConverter date2AxisConverter = new Date2AxisConverter();

        public TwitterView()
        {
            InitializeComponent();
        }

        public void ClearChart() => Execute.OnUIThread(() => ChartGrid.Children.Clear());

        public void AddChartData(TickerMentionData data)
        {
            Execute.OnUIThread(() =>
            {
                var graph = new BarGraph
                {
                    Color = Brushes.Blue,
                    BarsWidth = 1
                };

                graph.PlotBars((double) date2AxisConverter.Convert(data.Date, typeof(double), null, null), data.Mentions);
                ChartGrid.Children.Insert(0, graph);
            });
        }
    }
}
