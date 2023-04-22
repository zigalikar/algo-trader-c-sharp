using System.Text;
using System.IO.Compression;

namespace CryptoTools.Core.Extensions
{
    public static class GZipExtensions
    {
        public static void WriteLine(this GZipStream stream, string line)
        {
            line += "\n";

            var encoding = new ASCIIEncoding();
            var bytes = encoding.GetBytes(line);
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
