using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using AlgoTrader.Algos;
using AlgoTrader.Evaluate;
using AlgoTrader.Optimise;
using AlgoTrader.Exchanges;
using AlgoTrader.Backtest.Model;
using AlgoTrader.Backtest.Extensions;
using AlgoTrader.Core.Model.Backtest;
using AlgoTrader.Core.Model.Optimise;

using Newtonsoft.Json;

namespace AlgoTrader.Backtest.Helpers.Optimise
{
    public class DBPMOptimiserUI : OptimiserUI
    {
        protected async override Task<OptimiserUIChildOutput> GetOutput()
        {
            var startBalance = 10000;
            var evaluator = new SharpeRatioEvaluator(3.636);

            var optimiser = new BruteForceOptimiser<DBPM, Binance>(
                "C:\\Users\\Žiga\\Desktop\\exchange-csv\\yahoo\\BTC-USD-1D.csv", // training
                "C:\\Users\\Žiga\\Desktop\\exchange-csv\\yahoo\\BTC-USD-1D.csv", // test
                evaluator,
                GetDBPMBruteForceOptimisationParameters,
                new OptimisationOptions
                {
                    WorkerCount = 4,
                    BacktestOptions = new BacktestOptions
                    {
                        LogOrders = false,
                        StartingQuoteBalance = startBalance
                    }
                }
            );

            var columnData = new List<MultiColumnBarData>();
            var res = await optimiser.Run();
            for (var i = 0; i < res.Results.Count; i++)
            {
                var data = res.Results[i].Training; // Training and Test AlgoParams are the same

                var label = JsonConvert.SerializeObject(data.Options.AlgoParams);
                var equity = startBalance + data.Profit;
                var drawdown = data.MaxSystemDrawdown;

                columnData.Add(new MultiColumnBarData
                {
                    Label = label,
                    Value1 = equity,
                    Value2 = drawdown
                });

                //var par = res.Results[i].Training.Options.AlgoParams; 
                //var training = ratios.TrainingSharpeRatios[i];
                //var test = ratios.TestSharpeRatios[i];
            }
            
            // get optimise template & add chart script
            var optimiseTemplate = HtmlHelper.GetOptimiseTemplate();
            var scriptContainer = optimiseTemplate.GetBody();

            optimiseTemplate.AppendScript(scriptContainer, GetEquityDrawdownChartScript(columnData)); // equity & drawdown chart
            
            var title = string.Format("Optimisation {0:dd/MM/yyyy H:mm:ss.fff}", DateTime.UtcNow);
            return new OptimiserUIChildOutput(HtmlHelper.GetWrapper(title, optimiseTemplate.GetBody().ChildNodes), title);
        }

        private object[][] GetDBPMBruteForceOptimisationParameters()
        {
            return new object[][]
            {
                //Enumerable.Range(20, 30).Select(x => x as object).ToArray(),
                //Enumerable.Range(40, 45).Select(x => x as object).ToArray()
                Enumerable.Range(10, 10).Select(x => x as object).ToArray(),
                Enumerable.Range(5, 10).Select(x => x as object).ToArray()
            };
        }

        private string GetEquityDrawdownChartScript(List<MultiColumnBarData> data)
        {
            // bars
            var x = new List<string>();
            var equity = new List<double>();
            var drawdown = new List<double>();
            foreach (var e in data)
            {
                x.Add(e.Label);
                equity.Add(e.Value1);
                drawdown.Add(-e.Value2);
            }

            return $@"
                var data = [
                    {{
                        x: {JsonConvert.SerializeObject(x)},
                        y: {JsonConvert.SerializeObject(equity)},
                        name: 'Equity',
                        type: 'bar',
                        marker: {{
                            color: '{StyleHelper.UpColor}'
                        }}
                    }},
                    {{
                        x: {JsonConvert.SerializeObject(x)},
                        y: {JsonConvert.SerializeObject(drawdown)},
                        name: 'Drawdown',
                        type: 'bar',
                        marker: {{
                            color: '{StyleHelper.DownColor}'
                        }}
                    }}
                ];

                Plotly.newPlot('equity-drawdown-chart', data, {{
                    barmode: 'relative',
                    plot_bgcolor: '#00000000',
                    paper_bgcolor: '#00000000',
                    font: {{
                        color: 'rgba(71, 179, 173, 0.5)'
                    }},
                    yaxis: {{
                        gridcolor: 'rgba(255, 255, 255, 0.2)'
                    }}
                }});
            ";
        }
    }
}
