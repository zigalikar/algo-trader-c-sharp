using AlgoTrader.Backtest.Extensions;

using HtmlAgilityPack;

namespace AlgoTrader.Backtest.Helpers
{
    public static class HtmlHelper
    {
        public static HtmlDocument GetWrapper(string title, HtmlNodeCollection body)
        {
            var wrapper = new HtmlDocument();
            wrapper.Load("./Assets/Templates/Core/wrapper.html");
            
            // edit title
            var titleNode = wrapper.DocumentNode.SelectSingleNode("//head/title");
            titleNode.InnerHtml = title;
            
            // insert body
            wrapper.GetBody().AppendChildren(body);

            return wrapper;
        }

        public static HtmlDocument GetOptimiseTemplate()
        {
            var template = new HtmlDocument();
            template.Load("./Assets/Templates/optimise.html");

            return template;

            //var wrapperTemplate = GetWrapper(title);
            //return wrapperTemplate;
        }

        public static HtmlDocument GetBacktestTemplate()
        {
            var template = new HtmlDocument();
            template.Load("./Assets/Templates/backtest.html");

            return template;

            //var wrapperTemplate = GetWrapper(title);
            //return wrapperTemplate;
        }
    }
}
