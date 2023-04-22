using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using AlgoTrader.Algos;
using AlgoTrader.Exchanges;
using AlgoTrader.Core.Model;
using AlgoTrader.Backtesting;
using AlgoTrader.Backtest.Model;
using AlgoTrader.Core.Extensions;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Order;
using AlgoTrader.Backtest.Extensions;
using AlgoTrader.Core.Model.Backtest;
using AlgoTrader.Core.Model.Order.Backtest;

using Newtonsoft.Json;

namespace AlgoTrader.Backtest.Helpers.Backtest
{
    public class DBPMBacktesterUI : BacktesterUI
    {
        private static TimeFrameEnum _timeFrame = TimeFrameEnum.Minute1;

        protected async override Task<BacktesterUIChildOutput> GetOutput()
        {
            //var backtester = new Backtester<DBPM, Binance>("C:\\Users\\Žiga\\Desktop\\exchange-csv\\quantconnect\\EUR-USD-1D.csv", new BacktestOptions
            //var backtester = new Backtester<DBPM, Binance>("C:\\Users\\Žiga\\Desktop\\exchange-csv\\quantconnect\\printed-from-console\\eur-usd.csv", new BacktestOptions
            //var backtester = new Backtester<SMAAlgo, Binance>("C:\\Users\\Žiga\\Desktop\\exchange-csv\\yahoo\\BTC-USD-1D.csv", new BacktestOptions
            //var backtester = new Backtester<DBPM, Binance>("C:\\Users\\Žiga\\Desktop\\exchange-csv\\ftx script\\output.csv", new BacktestOptions
            var backtester = new Backtester<DBPM, Binance>("C:\\Users\\Žiga\\Desktop\\exchange-csv\\custom\\ftx\\btcusd", _timeFrame, new BacktestOptions
            {
                AlgoParams = new object[] { 11, 6 }
            });

            var res = await backtester.Run();

            // get optimise template & add chart scripts
            var backtestTemplate = HtmlHelper.GetBacktestTemplate();
            var scriptContainer = backtestTemplate.GetBody();

            backtestTemplate.AppendScript(scriptContainer, GetPortfolioChartScript(res)); // portfolio chart
            backtestTemplate.AppendScript(scriptContainer, GetPriceChartScript(res)); // price chart

            var title = string.Format("Backtesting {0:dd/MM/yyyy H:mm:ss.fff}", DateTime.UtcNow);
            return new BacktesterUIChildOutput(HtmlHelper.GetWrapper(title, backtestTemplate.GetBody().ChildNodes), title, new List<BacktesterUIStatisticsItem>
            {
                new BacktesterUIStatisticsItem("Total profit", res.Profit),
                new BacktesterUIStatisticsItem("Total profit [%]", res.ProfitPercentage * 100),
                new BacktesterUIStatisticsItem("Total trades", res.TotalTrades),
                new BacktesterUIStatisticsItem("Winners", res.Winners),
                new BacktesterUIStatisticsItem("Losers", res.Losers),
                new BacktesterUIStatisticsItem("Average win", res.AverageWin),
                new BacktesterUIStatisticsItem("Average loss", res.AverageLoss),
                new BacktesterUIStatisticsItem("Max consecutive wins", res.MaxConsecutiveWins),
                new BacktesterUIStatisticsItem("Max consecutive losses", res.MaxConsecutiveLosses),
                new BacktesterUIStatisticsItem("Max system drawdown", res.MaxSystemDrawdown),
                new BacktesterUIStatisticsItem("Max system drawdown [%]", res.MaxSystemDrawdownPercentage * 100),
                new BacktesterUIStatisticsItem("Max system drawdown duration", res.MaxSystemDrawdownDuration),
                new BacktesterUIStatisticsItem("Recovery factor", res.RecoveryFactor)
            });
        }

        private string GetPortfolioChartScript(BacktestResult result)
        {
            // candles
            var x = new List<string>();
            var open = new List<double>();
            var high = new List<double>();
            var low = new List<double>();
            var close = new List<double>();
            foreach (var e in result.Segments)
            {
                x.Add(e.Data.OpenTime.ToISOString());
                open.Add(e.Balance.OpenPrice);
                high.Add(e.Balance.HighPrice);
                low.Add(e.Balance.LowPrice);
                close.Add(e.Balance.ClosePrice);
            }

            var orders = new List<IOrder>();
            foreach (var o in result.OrdersFilled)
            {
                var ts = o.Timestamp.RoundDown(_timeFrame.ToTimeSpan()).ToISOString();
                var i = x.FindIndex(l => l.Equals(ts));
                orders.Add(new BacktestOrderFill(o.CurrencyPair, o.Type, o.Side, o.Status, open[i], o.Amount, o.Timestamp, o.Id));
            }

            var a = GetBuySellAnnotations(orders);
            return GetCandlesticksChartScript("portfolio-chart", x, a, open, high, low, close);
        }

        private string GetPriceChartScript(BacktestResult result)
        {
            // candles
            var x = new List<string>();
            var open = new List<double>();
            var high = new List<double>();
            var low = new List<double>();
            var close = new List<double>();
            foreach (var e in result.Segments)
            {
                x.Add(e.Data.OpenTime.ToISOString());
                open.Add(e.Data.OpenPrice);
                high.Add(e.Data.HighPrice);
                low.Add(e.Data.LowPrice);
                close.Add(e.Data.ClosePrice);
            }

            return GetCandlesticksChartScript("price-chart", x, GetBuySellAnnotations(result.OrdersFilled), open, high, low, close);
        }

        private int dataCounter = 0;
        private string GetCandlesticksChartScript(string elementId, IList<string> x, IList<ChartAnnotation> annotations, IList<double> open, IList<double> high, IList<double> low, IList<double> close) => $@"
            const data{dataCounter} = [{{
                x: {JsonConvert.SerializeObject(x)},
                open: {JsonConvert.SerializeObject(open)},
                high: {JsonConvert.SerializeObject(high)},
                low: {JsonConvert.SerializeObject(low)},
                close: {JsonConvert.SerializeObject(close)},
                decreasing: {{ line: {{ color: '{StyleHelper.DownColor}' }} }},
                increasing: {{ line: {{ color: '{StyleHelper.UpColor}' }} }},
                line: {{ color: 'rgba(31, 119, 180, 1)' }},
                type: 'candlestick',
                xaxis: 'x',
                yaxis: 'y'
            }}];

            Plotly.newPlot('{elementId}', data{dataCounter++}, {{
                dragmode: 'zoom',
                showlegend: false,
                plot_bgcolor: '#00000000',
                paper_bgcolor: '#00000000',
                font: {{
                    color: 'rgba(71, 179, 173, 0.5)'
                }},
                yaxis: {{
                    gridcolor: 'rgba(255, 255, 255, 0.2)'
                }},
                xaxis: {{
                    rangeslider: {{ visible: false }},
                    gridcolor: 'rgba(255, 255, 255, 0.2)'
                }},
                annotations: {JsonConvert.SerializeObject(annotations)}
            }});
        ";

        private static IList<ChartAnnotation> GetBuySellAnnotations(IList<IOrder> orders)
        {
            var annotations = new List<ChartAnnotation>();
            // buy orders
            foreach (var b in orders.Where(x => x.Side == OrderSide.Buy))
                annotations.Add(ChartAnnotation.Buy(b));

            // sell orders
            foreach (var s in orders.Where(x => x.Side == OrderSide.Sell))
                annotations.Add(ChartAnnotation.Sell(s));

            return annotations;
        }

        private class ChartAnnotation
        {
            private static int ArrowOffset => 30;

            [JsonProperty("x")]
            public string X { get; set; }

            [JsonProperty("y")]
            public double Y { get; set; }

            [JsonProperty("xref")]
            public string XRef { get; set; }

            [JsonProperty("yref")]
            public string YRef { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("font")]
            public ChartAnnotationFont Font { get; set; }

            [JsonProperty("showarrow")]
            public bool ShowArrow { get; set; } = true;

            [JsonProperty("arrowcolor")]
            public string ArrowColor { get; set; }

            [JsonProperty("xanchor")]
            public string XAnchor { get; set; }

            [JsonProperty("ax")]
            public int AX { get; set; }

            [JsonProperty("ay")]
            public int AY { get; set; }

            public static ChartAnnotation Buy(IOrder order) => new ChartAnnotation
            {
                X = order.Timestamp.RoundDown(_timeFrame.ToTimeSpan()).ToISOString(),
                Y = order.Price,
                Text = "Buy",
                ArrowColor = StyleHelper.UpColor,
                Font = new ChartAnnotationFont { Color = StyleHelper.UpColor },
                AY = -ArrowOffset
            };

            public static ChartAnnotation Sell(IOrder order) => new ChartAnnotation
            {
                X = order.Timestamp.RoundDown(_timeFrame.ToTimeSpan()).ToISOString(),
                Y = order.Price,
                Text = "Sell",
                ArrowColor = StyleHelper.DownColor,
                Font = new ChartAnnotationFont { Color = StyleHelper.DownColor },
                AY = ArrowOffset
            };

            public class ChartAnnotationFont
            {
                [JsonProperty("color")]
                public string Color { get; set; }
            }
        }
    }
}
