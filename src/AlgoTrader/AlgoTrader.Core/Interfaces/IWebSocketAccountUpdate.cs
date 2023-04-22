using System.Collections.Generic;

namespace AlgoTrader.Core.Interfaces
{
    public interface IWebSocketAccountUpdate
    {
        IList<IAccountInformationBalance> Balances { get; set; }
    }

    public interface IAccountInformationBalance
    {
        string Asset { get; }
        double Free { get; }
        double Locked { get; }
    }
}
