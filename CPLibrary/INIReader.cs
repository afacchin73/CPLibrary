using System;
using System.Collections;
using System.Text;
using System.IO;

namespace CPLibrary
{
    internal class INIReader
    {
        private const string SQL = "SQL Server";
        private const string ACCESS = "Access File";

        private static INIReader _instance;

        //This object avoids threading problems by locking this Singleton (INIReader) when it's loaded 
        private static readonly Object _locker = new Object();

        private string _iniPath;
        private FileStream _fileStream;
        private TextReader _inputFile;
        private string _stringStreamed;
        private string _serviceName;
        private bool _isSQL;
        private bool _isAccess;
        private string _connectionString;
        private string _contactTable;
        private string _databaseName;

        protected INIReader()
        {
            _iniPath = "";
            _stringStreamed = "";
            _serviceName = "";
            _isSQL = false;
            _isAccess = false;
            _connectionString = "";
            _contactTable = "";
            _databaseName = "";
        }

        public static INIReader Instance()
        {
            if (_instance == null)
            {
                lock (_locker)
                {
                    _instance = new INIReader();
                }
            }

            return _instance;
        }

        public string IniPath
        {
            get
            {
                lock (_locker)
                {
                    return _iniPath;
                }
            }
            set
            {
                lock (_locker)
                {
                    _iniPath = value;
                }
            }
        }

        public string ServiceName
        {
            get
            {
                lock (_locker)
                {
                    return _serviceName;
                }
            }
            set
            {
                lock (_locker)
                {
                    _serviceName = value;
                }
            }
        }

        public string ConnectionString
        {
            get
            {
                lock (_locker)
                {
                    return _connectionString;
                }
            }
        }

        public string DatabaseName
        {
            get
            {
                lock (_locker)
                {
                    return _databaseName;
                }
            }
        }

        public string ContactTable
        {
            get
            {
                lock (_locker)
                {
                    return _contactTable;
                }
            }
        }

        public bool checkDatabaseType(out bool isSQL, out bool isAccess)
        {
            lock (_locker)
            {
                isSQL = false;
                isAccess = false;
                try
                {
                    if (File.Exists(_iniPath))
                    {
                        _fileStream = new FileStream(_iniPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                        _inputFile = new StreamReader(_fileStream);

                        for (; ; )
                        {
                            _stringStreamed = _inputFile.ReadLine();

                            if (_stringStreamed == null)
                                break;

                            if (_stringStreamed.ToLower().Contains(_serviceName.Trim().ToLower()))
                            {
                                string delimStr = "=";
                                char[] delimiter = delimStr.ToCharArray();

                                string[] split = null;
                                split = _stringStreamed.Split(delimiter, 100);

                                ArrayList stringSplitted = new ArrayList(split);
                                string service = stringSplitted[0].ToString();

                                if (Equals(_serviceName.Trim().ToLower(), service.Trim().ToLower()))
                                {
                                    if (_stringStreamed.ToLower().Contains(SQL.ToLower()))
                                    {
                                        _isSQL = true;
                                        isSQL = true;
                                    }
                                    //else if (_stringStreamed.ToLower().Contains(ACCESS.ToLower()))
                                    //{
                                    //   _isAccess = true;
                                    //   isAccess = true;
                                    //}
                                    else
                                    {
                                        Logger.Instance().WriteTrace(String.Format("Database type in file INI not properly written for service {0}", _serviceName));

                                        _inputFile.Close();
                                        _fileStream.Close();
                                        return false;
                                    }
                                    string databaseType = "";
                                    if (isSQL)
                                        databaseType = SQL;
                                    //if (isAccess)
                                    //   databaseType = ACCESS;

                                    Logger.Instance().WriteTrace(String.Format("Database type from file INI for service {0}: {1}", _serviceName, databaseType));

                                    if (_isSQL)
                                    {
                                        delimStr = ",";
                                        delimiter = delimStr.ToCharArray();

                                        split = null;
                                        split = _stringStreamed.Split(delimiter, 100);

                                        stringSplitted = new ArrayList(split);

                                        _connectionString = "Data Source=" + stringSplitted[1].ToString() + ";Initial Catalog=" + stringSplitted[2].ToString() + ";Persist Security Info=True;User ID=" + stringSplitted[3].ToString() + ";password=" + stringSplitted[4].ToString();
                                        _contactTable = stringSplitted[5].ToString();
                                        _databaseName = stringSplitted[2].ToString();

                                        Logger.Instance().WriteTrace(String.Format("Database connection Service {0} String streamed: {1}", _serviceName, _connectionString));
                                    }

                                    _inputFile.Close();
                                    _fileStream.Close();
                                    return true;
                                }
                            }
                        }
                        Logger.Instance().WriteTrace(String.Format("Configuration for Service {0} not found in INI file, or INI file not existing", _serviceName));
                        return false;
                    }
                    else
                    {
                        Logger.Instance().WriteTrace(String.Format("File INI not available for Service {0}, please check", _serviceName));
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance().WriteTrace(String.Format("Exception {0} on reading file INI", e.Message));
                    return false;
                }
            }
        }
    }
}
