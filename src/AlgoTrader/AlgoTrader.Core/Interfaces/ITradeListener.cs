using System.Threading.Tasks;

namespace AlgoTrader.Core.Interfaces
{
    public interface IExchangeTradesToFile
    {
        int BatchSize { get; }

        Task Start();
        Task Stop();
    }
}
