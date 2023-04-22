using System.IO;
using System.Collections.Generic;

namespace AlgoTrader.Feeds.Helpers
{
    /// <summary>
    /// Used for reading backtesting data from .csv files
    /// </summary>
    public class CSVDataReader<T> where T : class
    {
        private readonly string _path;
        private readonly ICSVDataDecoder<T> _decoder;
        private readonly CSVDataReaderType _type;

        public CSVDataReader(string path, ICSVDataDecoder<T> decoder, CSVDataReaderType type = CSVDataReaderType.File)
        {
            _path = path;
            _decoder = decoder;
            _type = type;
        }

        private T _lastRead;
        /// <summary>
        /// Reads a .csv file containing all the historic data and returns the decoded results in an enumerable
        /// </summary>
        /// <returns>Enumerable of all lines in the CSV file</returns>
        public IEnumerable<T> Read()
        {
            if (_type == CSVDataReaderType.File)
            {
                foreach (var data in ReadFile(_path)) // return data from file
                    yield return data;
            }
            else if (_type == CSVDataReaderType.Folder)
            {
                var di = new DirectoryInfo(_path);
                foreach (var fi in di.EnumerateFiles()) // enumerate files in folder
                {
                    foreach (var data in ReadFile(fi.FullName)) // return data from file
                        yield return data;
                }
            }
        }

        private IEnumerable<T> ReadFile(string path)
        {
            using (var reader = new StreamReader(path))
            {
                reader.ReadLine(); // skip first line
                while (reader.EndOfStream == false)
                {
                    var line = reader.ReadLine();
                    _lastRead = _decoder.DecodeLine(line);

                    yield return _lastRead;
                }
            }
        }
    }

    public enum CSVDataReaderType
    {
        File = 0,
        Folder = 1
    }
}
