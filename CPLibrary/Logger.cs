using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPLibrary
{
    internal class Logger
    {
        private string _logFilePath;
        private static Logger _instance;

        //This object avoids threading problems by locking this Singleton (INIReader) when it's loaded
        private static readonly Object _locker = new Object();

        private FileStream _fileStream;
        private TextWriter _outputFile;

        protected Logger()
        {
            _fileStream = null;
            _outputFile = null;
            string currentTime = DateTime.Now.Day.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Year.ToString()+"_"+ DateTime.Now.Hour.ToString();
            _logFilePath = AppDomain.CurrentDomain.BaseDirectory + "BaseNETContactProvider_" + currentTime + ".log";
        }

        public static Logger Instance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = new Logger();
                }
            }

            return _instance;
        }

        public void WriteTrace(string trace)
        {
            lock (_locker)
            {
                try
                {
                    DateTime dateTime = DateTime.Now;
                    string dateNow = dateTime.ToShortDateString();
                    string timeNow = dateTime.Hour.ToString("d2") + ":" +
                        dateTime.Minute.ToString("d2") + ":" +
                        dateTime.Second.ToString("d2") + ":" +
                        dateTime.Millisecond.ToString("d3");

                    string traceWithDate = String.Format("[{0} {1}] {2}", dateNow, timeNow, trace);

                    if (File.Exists(_logFilePath))
                    {
                        _fileStream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write,
                        FileShare.Write);

                        _outputFile = new StreamWriter(_fileStream);

                        _outputFile.WriteLine(traceWithDate);
                        _outputFile.Flush();
                        _outputFile.Close();
                        _fileStream.Close();
                    }
                    else
                    {
                        _fileStream = new FileStream(_logFilePath, FileMode.CreateNew, FileAccess.Write,
                            FileShare.Write);

                        _outputFile = new StreamWriter(_fileStream);

                        _outputFile.WriteLine("{0}", traceWithDate);
                        _outputFile.Flush();
                        _outputFile.Close();
                        _fileStream.Close();
                    }
                }
                catch (Exception e)
                {
                    if (_outputFile != null && _fileStream != null)
                    {
                        _outputFile.Close();
                        _fileStream.Close();
                    }
                }
            }
        }



    }
}
