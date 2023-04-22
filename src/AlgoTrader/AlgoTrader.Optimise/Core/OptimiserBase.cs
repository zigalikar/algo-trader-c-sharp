using System;
using System.Threading.Tasks;

using AlgoTrader.Core.Model.Algo;
using AlgoTrader.Core.Interfaces;
using AlgoTrader.Core.Model.Optimise;

namespace AlgoTrader.Optimise.Core
{
    /// <summary>
    /// Used for optimising algos
    /// </summary>
    /// <typeparam name="A">Algo to backtest</typeparam>
    /// <typeparam name="E">Exchange to backtest on</typeparam>
    public abstract class OptimiserBase<A, E> : OptimiserBase where A : IBacktestAlgo where E : IExchange
    {
        /// <summary>
        /// Initiates a new instance of the optimiser
        /// </summary>
        /// <param name="trainingCsvPath">Path to the .csv file with training data</param>
        /// <param name="testCsvPath">Path to the .csv file with test data</param>
        /// <param name="evaluator">Evaluation metric for the optimiser</param>
        /// <param name="options">Optimisation options</param>
        /// <param name="algoOptions">Algo options</param>
        public OptimiserBase(string trainingCsvPath, string testCsvPath, IEvaluator evaluator, OptimisationOptions options = null, AlgoOptions algoOptions = null) : base(trainingCsvPath, testCsvPath, typeof(A), typeof(E), evaluator, options, algoOptions) { }
    }

    public abstract class OptimiserBase
    {
        protected static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected readonly string _trainingCsvPath;
        protected readonly string _testCsvPath;
        protected readonly Type _algoType;
        protected readonly Type _exchangeType;
        protected readonly IEvaluator _evaluator;
        protected readonly OptimisationOptions _options;
        protected readonly AlgoOptions _algoOptions;

        public abstract Task<OptimisationResult> Run();

        public OptimiserBase(string trainingCsvPath, string testCsvPath, Type algoType, Type exchangeType, IEvaluator evaluator, OptimisationOptions options = null, AlgoOptions algoOptions = null)
        {
            _trainingCsvPath = trainingCsvPath;
            _testCsvPath = testCsvPath;
            _algoType = algoType;
            _exchangeType = exchangeType;
            _evaluator = evaluator;
            _options = options ?? new OptimisationOptions();
            _algoOptions = algoOptions ?? new AlgoOptions();

            logger.Trace("Constructor");
        }
    }
}
