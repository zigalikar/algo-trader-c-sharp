using System.IO;
using System.Diagnostics;

using HtmlAgilityPack;

namespace AlgoTrader.Backtest.Helpers
{
    public abstract class HTMLOutputHelper
    {
        /// <summary>
        /// Outputs to HTML file and opens in browser
        /// </summary>
        /// <param name="document">Document to write to file</param>
        /// <param name="title">Title of the document (will be formatted to file-friendly title)</param>
        protected void WriteToHTMLFile(HtmlDocument document, string title)
        {
            var fileName = string.Format("{0}.html", title.Replace(' ', '_').Replace('.', '-').Replace(':', '-').Replace('/', '-'));
            var outputHtml = Path.Combine(Directory.GetCurrentDirectory(), "output", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(outputHtml));
            document.Save(outputHtml);
            Process.Start(outputHtml);
        }
    }
}
