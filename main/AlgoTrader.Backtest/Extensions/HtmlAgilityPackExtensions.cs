using HtmlAgilityPack;

namespace AlgoTrader.Backtest.Extensions
{
    public static class HtmlAgilityPackExtensions
    {
        public static HtmlNode GetBody(this HtmlDocument document) => document.DocumentNode.SelectSingleNode("//body");

        public static void AppendScript(this HtmlDocument document, HtmlNode node, string script)
        {
            var el = document.CreateElement("script");
            el.InnerHtml = script;
            node.AppendChild(el);
        }
    }
}
