using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using AlgoTrader.Backtesting;
using AlgoTrader.Optimise.Core;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Algo;
using AlgoTrader.Core.Model.Backtest;
using AlgoTrader.Core.Model.Optimise;

namespace AlgoTrader.Optimise
{
    public class BruteForceOptimiser<A, E> : OptimiserBase<A, E> where A : IBacktestAlgo where E : IExchange
    {
        private readonly Func<object[][]> _parameterGenerator;

        private TaskCompletionSource<OptimisationResult> _optimiseTcs;

        private readonly IList<Thread> _workers = new List<Thread>();
        private readonly ConcurrentQueue<object[]> _queue = new ConcurrentQueue<object[]>();
        private readonly IList<BacktestResultTrainingTestPair> _results = new List<BacktestResultTrainingTestPair>();
        private int _permutationsCount = 0;

        public BruteForceOptimiser(string trainingCsvPath, string testCsvPath, IEvaluator evaluator, Func<object[][]> parameterGenerator, OptimisationOptions options = null, AlgoOptions algoOptions = null) : base(trainingCsvPath, testCsvPath, evaluator, options, algoOptions)
        {
            _parameterGenerator = parameterGenerator ?? throw new ArgumentNullException(nameof(parameterGenerator));
        }

        public override Task<OptimisationResult> Run()
        {
            if (_optimiseTcs != null && _optimiseTcs.Task.IsCompleted == false)
                _optimiseTcs.SetCanceled();
            _optimiseTcs = new TaskCompletionSource<OptimisationResult>();

            // get parameters for optimisation

            var parameters = _parameterGenerator.Invoke();
            if (parameters != null)
            {
                // get all permutations of optimiser parameters
                var permutations = new List<object[]> { null };
                foreach (var list in parameters)
                    permutations = permutations.SelectMany(o => list.Select(s => (o ?? new object[] { }).Concat(new object[] { s }).ToArray())).ToList();

                // delegate parameters to workers
                _permutationsCount = permutations.Count;
                var paramsToStart = new List<object[]>();
                for (var i = 0; i < permutations.Count; i++)
                {
                    if (i < _options.WorkerCount)
                        paramsToStart.Add(permutations[i]); // workers
                    else
                        _queue.Enqueue(permutations[i]); // add to queue
                }

                // start workers
                logger.Info(string.Format("Starting optimisation with {0} workers and {1} permutations of optimisation parameters.", paramsToStart.Count, _permutationsCount));
                for (var i = 0; i < paramsToStart.Count; i++)
                {
                    var p = paramsToStart[i];
                    var worker = new Thread(async () => await RunWorker(p))
                    {
                        Name = string.Format("Optimisation worker {0}", i)
                    };

                    _workers.Add(worker);
                    worker.Start();
                }
            }
            else
                throw new ArgumentException("Arguments provided from the optimising parameters function are null.");

            return _optimiseTcs.Task;
        }

        private async Task RunWorker(object[] parameters)
        {
            // create backtester for each worker
            var opts = _options.BacktestOptions.Clone();
            opts.AlgoParams = parameters;
            var training = new Backtester(_trainingCsvPath, _algoType, _exchangeType, opts, _algoOptions);
            var test = new Backtester(_testCsvPath, _algoType, _exchangeType, opts, _algoOptions);

            // run backtester
            _results.Add(new BacktestResultTrainingTestPair(await training.Run(), await test.Run()));

            DelegateNextTaskToWorker();
        }

        private DateTime? _lastLog;
        private async void DelegateNextTaskToWorker()
        {
            if (_results.Count == _permutationsCount)
            {
                // all workers finished
                logger.Info("Shutting down last worker, evaluating results");
                var optimal = _evaluator.Evaluate(_results);
                _optimiseTcs.SetResult(new OptimisationResult
                {
                    Results = optimal
                });
            }
            else
            {
                // take params from queue
                if (_queue.TryDequeue(out object[] param))
                {
                    if (_queue.Count > 0 && _queue.Count % 100 == 0)
                    {
                        var str = string.Format("Remaining in queue: {0}", _queue.Count);
                        if (_lastLog.HasValue)
                            str += string.Format(", since last log: {0} ms", (DateTime.UtcNow - _lastLog.Value).TotalMilliseconds);

                        logger.Info(str);
                        _lastLog = DateTime.UtcNow;
                    }

                    // take from queue
                    await RunWorker(param);
                }
                else
                {
                    logger.Info("Shutting down worker - empty queue");
                    //Thread.CurrentThread.
                }
            }
        }
    }
}
