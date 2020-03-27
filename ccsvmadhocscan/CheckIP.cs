using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ccsvmadhocscan
{
    public struct ParseIPListsCSV
    {
        public Dictionary<string, List<string>> mappedData;
        public int matchedLines;
        public String[] headers;
    };

    public struct ParseScanEngine
    {
        public Dictionary<string, string> mappedIPScanEngine;
        public List<string> scanEngineList;

    };

    class CheckIP
    {
        static string[] csvIncludedColumns = { "SiteID", "SiteName", "RangeStart", "RangeEnd", "ScanEngine" };
        static string[] csvExcludedColumns = { "SiteID", "SiteName", "RangeStart", "RangeEnd" };
        static string includedFilePath = ConfigurationManager.AppSettings["csvFileinclude"];
        static string excludedFilePath = ConfigurationManager.AppSettings["csvFileExclude"];
        static string logPath = ConfigurationManager.AppSettings["logPath"];
        static string logFileName = ConfigurationManager.AppSettings["logFileName"];
        static string logFilePath = logPath + logFileName;
        static private ILog Llog;//= log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static public String log4netConfigFile; // = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\log4net.config";
        static public String NexposeAdhocScanLog;


        public static bool IsInRange(string startIpAddr, string endIpAddr, string address)
        {
            long ipStart = BitConverter.ToInt32(IPAddress.Parse(startIpAddr).GetAddressBytes().Reverse().ToArray(), 0);

            long ipEnd = BitConverter.ToInt32(IPAddress.Parse(endIpAddr).GetAddressBytes().Reverse().ToArray(), 0);

            long ip = BitConverter.ToInt32(IPAddress.Parse(address).GetAddressBytes().Reverse().ToArray(), 0);

            return ip >= ipStart && ip <= ipEnd; //edited
        }

        public static int ipIsExcluded(string ipAddress, string country, int thread)
        {
            NexposeAdhocScanLog = ConfigurationManager.AppSettings["NexposeAdhocScanErrorLog"];
            Llog = log4net.LogManager.GetLogger(NexposeAdhocScanLog);
            log4netConfigFile = ConfigurationManager.AppSettings["Log4netConfig"];
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4netConfigFile));

            //get excluded file content
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Start processing exclusion checking");
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Start processing csv file: " + excludedFilePath);
            Llog.Info("Thread " + thread + ": Start processing exclusion checking");
            Llog.Info("Thread " + thread + ": Start processing csv file: " + excludedFilePath);
            int csvLines = 0;
            StreamReader csvReader = null;
            FileInfo csvFileInfo = null;
            bool isExcluded = false;
            csvReader = new StreamReader(File.OpenRead(excludedFilePath));
            csvFileInfo = new FileInfo(excludedFilePath);
            int matchedLines = 0;
            var vartable = new Dictionary<string, List<string>>();
            foreach (string columnName in csvExcludedColumns)
            {
                vartable[columnName] = new List<string>();
            }

            try
            {


                while (!csvReader.EndOfStream)
                {
                    var line = csvReader.ReadLine();
                    csvLines++;
                    if (csvLines == 1)
                    {
                        continue;
                    }
                    var values = line.Split(',');
                    if (values.Length == csvExcludedColumns.Length)
                    {
                        if (values[1].Contains(country))
                        {
                            int j = 0;
                            foreach (string columnName in csvExcludedColumns)
                            {
                                vartable[columnName].Add(values[j]);
                                j++;
                            }
                            matchedLines++;
                        }
                    }
                    else
                    {
                        Llog.Warn("Thread " + thread + ": Current csv file doesn't contain all the required columns: " + csvFileInfo.Name);
                        Llog.Warn("Thread " + thread + ": Current csv file line: " + line);
                        //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Current csv file doesn't contain all the required columns: " + csvFileInfo.Name);
                        //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Current csv file line: " + line);
                        ////csvReader.Close();
                    }
                }
                csvReader.Close();
            }
            catch (Exception)
            {
                
                csvReader.Close();
            }
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Finish reading csv file: " + excludedFilePath);
            Llog.Info("Thread " + thread + ": Finish reading csv file: " + excludedFilePath);

            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Total csv lines including header: " + csvLines);

            for (int i = 0; i < matchedLines; i++)
            {
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Start processing matched lines: " + i);
                string siteID = vartable["SiteID"].ToArray()[i];
                string siteName = vartable["SiteName"].ToArray()[i];
                string rangeStart = vartable["RangeStart"].ToArray()[i];
                string rangeEnd = vartable["RangeEnd"].ToArray()[i];
                if (rangeEnd == "\"\"")
                {
                    Llog.Info("Thread " + thread + ": No range end, it's single IP");
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": No range end, it's single IP");

                    rangeEnd = rangeStart;
                }
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Range start with: " + rangeStart);
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Range end with: " + rangeEnd);
                if (IsInRange(rangeStart, rangeEnd, ipAddress))
                {
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Matched range found");
                    Llog.Info("Thread " + thread + ": Matched range found");

                    isExcluded = true;
                    break;
                }
            }

            if (isExcluded)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static int ipIsIncluded(string ipAddress, string country, int thread)
        {

            //get excluded file content
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Start processing csv file: " + includedFilePath);
            Llog.Info("Thread " + thread + ": Start processing csv file: " + includedFilePath);

            int csvLines = 0;
            StreamReader csvReader = null;
            FileInfo csvFileInfo = null;
            bool isIncluded = false;
            csvReader = new StreamReader(File.OpenRead(includedFilePath));
            csvFileInfo = new FileInfo(includedFilePath);
            int matchedLines = 0;
            var vartable = new Dictionary<string, List<string>>();
            foreach (string columnName in csvIncludedColumns)
            {
                vartable[columnName] = new List<string>();
            }

            try
            {
                while (!csvReader.EndOfStream)
                {
                    var line = csvReader.ReadLine();
                    csvLines++;
                    if (csvLines == 1)
                    {
                        continue;
                    }
                    
                    var values = line.Split(',');
                    if (values.Length == csvIncludedColumns.Length)
                    {
                        if (values[1].Contains(country))
                        {
                            int j = 0;
                            foreach (string columnName in csvIncludedColumns)
                            {
                                vartable[columnName].Add(values[j]);
                                j++;
                            }
                            matchedLines++;
                        }
                    }
                    else
                    {
                        //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Current csv file doesn't contain all the required columns: " + csvFileInfo.Name);
                        //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Current csv file line: " + line);
                        //csvReader.Close();
                    }
                }
                csvReader.Close();
            }
            catch (Exception ex)
            {
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Exception occurred during reading excluded file: " + ex.ToString());
                Llog.Error("Thread " + thread + ": Exception occurred during reading excluded file: " + ex.ToString());


            }
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Finish reading csv file: " + includedFilePath);
            Llog.Info("Thread " + thread + ": Finish reading csv file: " + includedFilePath);

            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Total csv lines including header: " + csvLines);

            for (int i = 0; i < matchedLines; i++)
            {
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Start processing matched lines: " + i);
                string siteID = vartable["SiteID"].ToArray()[i];
                string siteName = vartable["SiteName"].ToArray()[i];
                string rangeStart = vartable["RangeStart"].ToArray()[i];
                string rangeEnd = vartable["RangeEnd"].ToArray()[i];
                if (rangeEnd == "\"\"")
                {
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": No range end, it's single IP");
                    Llog.Info("Thread " + thread + ": No range end, it's single IP");
                    rangeEnd = rangeStart;
                }
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Range start with: " + rangeStart);
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Range end with: " + rangeEnd);
                if (IsInRange(rangeStart, rangeEnd, ipAddress))
                {
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Matched range found");
                    Llog.Info("Thread " + thread + ": Matched range found");

                    isIncluded = true;
                    break;
                }
            }

            if (isIncluded)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }


        public ParseScanEngine GetScanEngineDetails(string ipListString, string country, int thread)
        {
            ParseScanEngine parsedValues = new ParseScanEngine();
            Dictionary<string, string> mappedIPScanEngine = new Dictionary<string, string>();
            List<string> scanEngineList = new List<string>();
            String[] ip = ipListString.Split(',');
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Start to get Scan Engine details.");
            Llog.Info("Thread " + thread + ": Start to get Scan Engine details.");

            ParseIPListsCSV parsedData = ParsedIPLists(includedFilePath, country);

            for (int i = 0; i < ip.Length - 1; i++)
            {

                //     Library.WriteErrorLog(logFilePath, "parsedData.matchedLines " + parsedData.matchedLines);

                for (int j = 0; j < parsedData.matchedLines; j++)
                {
                    string siteID = parsedData.mappedData["SiteID"].ToArray()[j];
                    string siteName = parsedData.mappedData["SiteName"].ToArray()[j];
                    string rangeStart = parsedData.mappedData["RangeStart"].ToArray()[j];
                    string rangeEnd = parsedData.mappedData["RangeEnd"].ToArray()[j];
                    string scanEngine = parsedData.mappedData["ScanEngine"].ToArray()[j];

                    if (rangeEnd == "\"\"")
                        rangeEnd = rangeStart;

                    if (IsInRange(rangeStart, rangeEnd, ip[i]))
                    {
                        scanEngineList.Add(scanEngine);

                        if (!mappedIPScanEngine.ContainsKey(ip[i]))
                        {
                            // Library.WriteErrorLog(logFilePath, "IP[i] " + ip[i]);
                            mappedIPScanEngine.Add(ip[i], scanEngine);
                        }

                        break;
                    }
                }
            }

            parsedValues.scanEngineList = scanEngineList;
            parsedValues.mappedIPScanEngine = mappedIPScanEngine;

            return parsedValues;
        }

        public String[] GetHeaders(StreamReader csvReader)
        {
            var line = csvReader.ReadLine();
            var values = line.Split(',');

            return values;
        }


        public ParseIPListsCSV ParsedIPLists(string csvFile, string country)
        {
            ParseIPListsCSV parsedValues = new ParseIPListsCSV();
            var mapRawData = new Dictionary<string, List<string>>();
            int matchedLines = 0;
            int validLines = 0;

            using (StreamReader csvReader = new StreamReader(File.OpenRead(csvFile), true))
            {
                string[] headers = GetHeaders(csvReader);

                foreach (string columnName in headers)
                    mapRawData[columnName] = new List<string>();

                while (!csvReader.EndOfStream)
                {
                    var rawData = csvReader.ReadLine();
                    var dataValue = rawData.Split(',');

                    var newFieldsList = new List<string>(dataValue);

                    if (headers.Length == dataValue.Length)
                    {
                        if (dataValue[1].Contains(country))
                        {
                            // Library.WriteErrorLog(logFilePath,"dataValue[1] " + dataValue[1] + " country " + country);
                            int j = 0;
                            foreach (string columnName in headers)
                            {
                                mapRawData[columnName].Add(dataValue[j]);
                                j++;
                            }

                            validLines++;
                        }
                    }

                    matchedLines++;
                }



                parsedValues.headers = headers;
                parsedValues.mappedData = mapRawData;
                parsedValues.matchedLines = validLines;

            }

            return parsedValues;
        }

    }
}