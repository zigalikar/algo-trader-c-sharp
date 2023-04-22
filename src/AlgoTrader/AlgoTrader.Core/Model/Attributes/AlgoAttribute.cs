using System;

namespace AlgoTrader.Core.Model.Attributes
{
    public class Algo : Attribute
    {
        public string Type { get; set; }
        public Type ImplementationClass { get; private set; }

        public Algo(string type, Type implementationClass)
        {
            Type = type;
            ImplementationClass = implementationClass;
        }
    }
}
