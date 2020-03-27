using CNAVCryptoLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ccsvmadhocscan
{
    public struct parseCSVFile
    {
        public Dictionary<string, List<string>> mappedData;
        public int matchedLines;
        public String[] headers;
    };
    public struct parseSPInfo
    {
        public string _spSite;
        public string _scanEngine;
    };

    //Comment out the prvious method from line 638. Use new methods for program enchancement. 
    //Will NOT update the part of upload report to SharePoint.
    //QW 25-May-2017
    class AdhocScan
    {
        static string programPath = ConfigurationManager.AppSettings["programPath"];
        static string logPath = ConfigurationManager.AppSettings["logPath"];
        static string logFileSize = ConfigurationManager.AppSettings["logFileSize"];
        static string emailServer = ConfigurationManager.AppSettings["smtpServer"];
        static string emailPort = ConfigurationManager.AppSettings["smtpServerPort"];
        static string logFileName = ConfigurationManager.AppSettings["logFileName"];
        static string logFilePath = logPath + logFileName;
        static string rubyPath = ConfigurationManager.AppSettings["rubyPath"];
        static string reportPath = ConfigurationManager.AppSettings["reportPath"];
        static string emailAddressFrom = ConfigurationManager.AppSettings["emailAddress"];
        static string adminEmailAddress = ConfigurationManager.AppSettings["adminEmailAddress"];
        static string adhocScanReportScript = rubyPath + @"scripts\run_adhoc_scan_reports.rb";
        static string includedFilePath = ConfigurationManager.AppSettings["csvFileinclude"];
        static string excludedFilePath = ConfigurationManager.AppSettings["csvFileExclude"];
        static string ccsVMServerIP = ConfigurationManager.AppSettings["ccsVMServerIP"];
        static string ccsVMServerPort = ConfigurationManager.AppSettings["ccsVMServerPort"];
        static string ccsVMServerUser = ConfigurationManager.AppSettings["ccsVMServerUser"];
        static string passworFile = ConfigurationManager.AppSettings["ccsVMServerPassword"];
        static string csvFilePath = ConfigurationManager.AppSettings["csvFilePath"];
        static string csvFilePathFailed = ConfigurationManager.AppSettings["csvFilePathFailed"];
        static string csvFilePathProcessed = ConfigurationManager.AppSettings["csvFilePathProcessed"];
        static string getSiteIncluded = rubyPath + @"scripts\get_site_included_range.rb";
        static string getSiteExcluded = rubyPath + @"scripts\get_site_excluded_range.rb";
        static string deleteSite = rubyPath + @"scripts\run_delete_site.rb";
        static string tempLogFilePath = Directory.GetCurrentDirectory().ToString() + @"\" + logFileName;
        static string uploadServer = ConfigurationManager.AppSettings["uploadServer"];
        static string sharePointMappingFile = ConfigurationManager.AppSettings["sharePointMappingFile"];
        //static string[] csvColumns = { "SharepointRecordID", "AssetIP", "Requester", "RequesterEmailID", "RequesterComments", "Country", "ScanTemplate" };
        //static string[] csvColumns = { "SharepointRecordID", "AssetIP", "Requester", "RequesterEmailID", "Country", "ScanTemplate" };
        static string csvFailedProcess = ConfigurationManager.AppSettings["csvfailedprocess"];
        static string csvProcessed = ConfigurationManager.AppSettings["csvprocessed"];
        static string csvFailedProcessLogPath = logPath + csvFailedProcess;
        static string csvProcessedLogPath = logPath + csvProcessed;
        static string scanFailureNotice = ConfigurationManager.AppSettings["scanFailureNotice"];
        bool csvProcessedSuccess = false;
        bool csvProcessedFail = false;

        public string dquote = "\"";
        public string subject = null;
        public string body = null;
        public string listIPs = null;
        public string listIPs1 = null;




        //---------Log4net -------//
        static private ILog Llog;//= log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static public String log4netConfigFile; // = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\log4net.config";
        static public String NexposeAdhocScanLog;

        static private ILog failedLog;
        static private ILog processLog;
        static public String FailedScanLog;
        static public String ProcessedScanLog;


        public void runScan(string csvFile, int thread)
        {
            NexposeAdhocScanLog = ConfigurationManager.AppSettings["NexposeAdhocScanErrorLog"];
            FailedScanLog = ConfigurationManager.AppSettings["NexposeAdhocScanFailProcessedLog"];
            ProcessedScanLog = ConfigurationManager.AppSettings["NexposeAdhocScanProcessedLog"];

            Llog = log4net.LogManager.GetLogger(NexposeAdhocScanLog);
            failedLog = log4net.LogManager.GetLogger(FailedScanLog);
            processLog = log4net.LogManager.GetLogger(ProcessedScanLog);


            log4netConfigFile = ConfigurationManager.AppSettings["Log4netConfig"];
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4netConfigFile));

            string id = null;
            string requester = null;
            string emailId = null;
            string country = null;
            string scanTemplate = null;
            string allIP = null;
            string excludedIP = null;
            string includedIP = dquote;
            string outofrangeIP = null;

            int ipIsExcluded;
            int ipIsIncluded;
            int countExclude = 0;
            int countInclude = 0;
            int countOutRange = 0;
            int validateOutofRange = 0;
            bool excludeError = false;
            bool includeError = false;

            FileInfo csvFileInfo = new FileInfo(csvFile);
          //  String csvFileName = csvFileInfo.Name;

            Mutex mutex = null;
            bool ownsMutex = false;
            string CcsAdhocScanMutex = "CcsAdhocScanMutex";


             String processedFileFullPath = csvFilePathProcessed + csvFileInfo.Name;

            if (File.Exists(processedFileFullPath))
             {
                 File.Delete(processedFileFullPath);
             }
             String failedFileFullPath = csvFilePathFailed + csvFileInfo.Name;
             if (File.Exists(failedFileFullPath))
             {
                 File.Delete(failedFileFullPath); 
             }

            while (!ownsMutex)
            {
                using (mutex = new Mutex(true, CcsAdhocScanMutex, out ownsMutex))
                {
                    if (!ownsMutex)
                    {
                        ownsMutex = mutex.WaitOne(5000);
                    }
                    if (ownsMutex)
                    {
                        //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Obtained mutex for ruby script");
                        Llog.Info("Thread " + thread + ": Obtained mutex for ruby script");
                        try
                        {
                            parseCSVFile parsedValue = parsedCSV(csvFile);

                            for (int i = 0; i < parsedValue.matchedLines; i++)
                            {
                                id = parsedValue.mappedData[parsedValue.headers[0]].ToArray()[i];
                                requester = Regex.Replace(parsedValue.mappedData[parsedValue.headers[2]].ToArray()[i], @"\s", "");
                                emailId = parsedValue.mappedData[parsedValue.headers[3]].ToArray()[i];
                                country = parsedValue.mappedData[parsedValue.headers[4]].ToArray()[i];
                                scanTemplate = parsedValue.mappedData[parsedValue.headers[5]].ToArray()[i];
                                allIP = parsedValue.mappedData[parsedValue.headers[1]].ToArray()[i];


                                ipIsExcluded = CheckIP.ipIsExcluded(parsedValue.mappedData[parsedValue.headers[1]].ToArray()[i], country, thread);

                                switch (ipIsExcluded)
                                {
                                    case 1:
                                        excludedIP += parsedValue.mappedData[parsedValue.headers[1]].ToArray()[i] + ",";
                                        countExclude++;
                                        break;
                                    case -1:
                                        excludeError = true;
                                        break;
                                    case 0:
                                        ipIsIncluded = CheckIP.ipIsIncluded(parsedValue.mappedData[parsedValue.headers[1]].ToArray()[i], country, thread);
                                        switch (ipIsIncluded)
                                        {
                                            case 1:
                                                includedIP += parsedValue.mappedData[parsedValue.headers[1]].ToArray()[i] + ",";
                                                countInclude++;
                                                break;
                                            case 0:
                                                outofrangeIP += parsedValue.mappedData[parsedValue.headers[1]].ToArray()[i] + ",";
                                                countOutRange++;
                                                break;
                                            case -1:
                                                includeError = true;
                                                break;
                                            default:
                                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": invalid ipIsIncluded input");
                                                Llog.Error("Thread " + thread + ": invalid ipIsIncluded input");
                                                Library.WriteErrorLog(logFilePath, "Thread " + thread + ": invalid ipIsIncluded input", csvFileInfo.Name);
                                                break;
                                        }
                                        break;
                                    default:
                                        //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": invalid ipIsIncluded input");
                                        Llog.Error("Thread " + thread + ": invalid ipIsIncluded input");
                                        Library.WriteErrorLog(logFilePath, "Thread " + thread + ": invalid ipIsIncluded input", csvFileInfo.Name);

                                        break;

                                }
                            }

                            if (excludeError == true)
                            {
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Error when checking the excluded list");
                                Llog.Error("Thread " + thread + ": Error when checking the excluded list");
                                Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Error when checking the excluded list", csvFileInfo.Name);

                                
                                subject = "CCSVM Adhoc Scan: [Error] when checking the excluded list";
                                body = "Hit error when checking excluded list file.<br>Please verify the data in exclude list file.";
                                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                            }
                            else
                            {
                                //all excluded, move to failed location,send email
                                //terminate this file process, proceed next file
                                if (countExclude == parsedValue.matchedLines)
                                {
                                    Llog.Warn( "Thread " + thread + ": [Warning] ALL IPs are Not Applicable for CCSVM Adhoc Scan");
                                    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": [Warning] ALL IPs are Not Applicable for CCSVM Adhoc Scan", csvFileInfo.Name);

                                    File.Move(csvFile, failedFileFullPath);
                                    Llog.Info("Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);
                                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The csv file " + csvFile + " has been moved to " + processedFileFullPath);
                                    csvProcessedFail = true;

                                    subject = "CCSVM Adhoc Scan: [Warning] ALL IPs are found in Excluded List which are Not Applicable for CCSVM Adhoc Scan";
                                    listIPs = excludedIP.Replace(",", "<br>");
                                    body = "<p>All IPs are not applicable for CCSVM Adhoc Scan, please refer to below list: <br>"
                                        + listIPs + "</p><p>The request file is: <br>" + csvFileInfo.Name + "</p>";

                                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                                    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The IPs are: " + excludedIP, csvFileInfo.Name);

                                }
                                else if (excludedIP != null && countExclude < parsedValue.matchedLines)
                                {
                                    subject = "CCSVM Adhoc Scan: [Warning] Found IP in Excluded List which is Not Applicable for CCSVM Adhoc Scan";

                                    //   body = excludedIP;
                                    listIPs = excludedIP.Replace(",", "<br>");
                                    body = "<p>The Asset IP(s) is excluded from Adhoc Scanning as following:<br>"
                                        + listIPs + "</p><p> The rest of Asset IP(s) will be continue processed.</p>";
                                    csvProcessedFail = true;

                                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);

                                    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": [Warning] Found IP: " + excludedIP + " in Excluded List, skip the scannning for those IPs. Continue to check other IPs.", csvFileInfo.Name);
                                    Llog.Warn("Thread " + thread + ": [Warning] Found IP in Excluded List, skip the scannning for those IPs. Continue to check other IPs.");

                                    validateOutofRange = parsedValue.matchedLines - countExclude;
                                    validateToPrceedAdhocScan(includeError, countOutRange, validateOutofRange, outofrangeIP, countInclude, includedIP, requester, emailId, scanTemplate, country, csvFileInfo, processedFileFullPath, failedFileFullPath, thread);
                                }
                                else if (excludedIP == null)
                                {
                                    validateOutofRange = parsedValue.matchedLines;

                                    validateToPrceedAdhocScan(includeError, countOutRange, validateOutofRange, outofrangeIP, countInclude, includedIP, requester, emailId, scanTemplate, country, csvFileInfo, processedFileFullPath, failedFileFullPath, thread);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Llog.Error("Thread " + thread + ": Failed to get csv file content: " + csvFileInfo.Name);
                            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Failed to get csv file content: " + csvFileInfo.Name);
                            Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Failed to get csv file content: " + csvFileInfo.Name, csvFileInfo.Name);

                            //Library.WriteErrorLog(logFilePath, ex.Message);
                            Llog.Error(ex.Message);


                            File.Move(csvFile, failedFileFullPath);
                            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);
                            Llog.Error("Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);
                            csvProcessedFail = true;

                            //Library.WriteErrorLog(csvFailedProcessLogPath, csvFile); //-------To be done
                            failedLog.Info(csvFile);

                            string subject = "CCSVM AdhocScan: [Failure] Failed to get csv file content";
                            string body = "Current csv file doesn't contain all the required columns: " + csvFileInfo.Name;
                            Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Continue next file");
                            Llog.Info("Thread " + thread + ": Continue next file");

                            //mutex.ReleaseMutex();
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": release mutex for ruby script");
                            Llog.Info("Thread " + thread + ": release mutex for ruby script");

                        }
                    }
                    else
                    {
                        int randTime = new Random().Next(5, 10);
                        Thread.Sleep(randTime * 1000);
                    }
                }
            }
        }

        public String[] getHeaders(StreamReader csvReader)
        {
            var line = csvReader.ReadLine();
            var values = line.Split(',');

            return values;
        }

        public parseCSVFile parsedCSV(string csvFile)
        {
            parseCSVFile parseValues = new parseCSVFile();
            StreamReader csvReader = null;
            csvReader = new StreamReader(File.OpenRead(csvFile));

            var mapRawData = new Dictionary<string, List<string>>();
            int matchedLines = 0;

            int count = 0;
            string newFieldName = "";
            List<int> findIndex = new List<int>();

            string[] headers = getHeaders(csvReader);

            foreach (string columnName in headers)
                mapRawData[columnName] = new List<string>();

            while (!csvReader.EndOfStream)
            {
                var rawData = csvReader.ReadLine();
                var dataValue = rawData.Split(',');

                var newFieldsList = new List<string>(dataValue);

                if (headers.Length != dataValue.Length && headers.Length < dataValue.Length)
                {
                    foreach (string findField in dataValue)
                    {
                        if (findField.Contains("\""))
                        {
                            findIndex.Add(count);
                        }
                        count++;
                    }

                    for (int i = findIndex[0]; i <= findIndex.Last(); i++)
                    {
                        if (i != findIndex.Last())
                        {
                            newFieldName += dataValue[i] + ",";
                            newFieldsList.Remove(dataValue[i]);
                        }
                        else { newFieldName += dataValue[i]; }
                    }

                    newFieldsList[findIndex[0]] = newFieldName;
                }

                int j = 0;
                foreach (string columnName in headers)
                {
                    mapRawData[columnName].Add(newFieldsList[j]);
                    j++;
                }

                matchedLines++;
            }

            csvReader.Close();

            parseValues.headers = headers;
            parseValues.mappedData = mapRawData;
            parseValues.matchedLines = matchedLines;


            return parseValues;
        }

        public parseSPInfo parseSPSiteFile(string country, int thread)
        {
            parseSPInfo parseSPInfo = new parseSPInfo();
            parseCSVFile parsedValue = parsedCSV(sharePointMappingFile);

            string spSite = "";
            string scanEngine = "";

            for (int i = 0; i < parsedValue.matchedLines; i++)
            {
                if (parsedValue.mappedData[parsedValue.headers[1]].ToArray()[i] == country)
                {
                    spSite = parsedValue.mappedData[parsedValue.headers[0]].ToArray()[i];
                    scanEngine = parsedValue.mappedData[parsedValue.headers[4]].ToArray()[i];

                    parseSPInfo._spSite = spSite;
                    parseSPInfo._scanEngine = scanEngine;
                }
            }

            return parseSPInfo;
        }

        protected String passwordDecryption(string requesterEmailID, int thread)
        {
            string decryptedPassword = "";
            string passPhrase = "xyz123!@#";
            string emailList = adminEmailAddress;
            byte[] bytesAlreadyEncrypted = null;
            byte[] bytesDecrypted = null;
            byte[] passwordBytes = Encoding.UTF8.GetBytes(passPhrase);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);

            if (Int32.Parse(scanFailureNotice) == 1)
                emailList = emailList + "," + requesterEmailID;

            try
            {
                bytesAlreadyEncrypted = File.ReadAllBytes(passworFile);
                bytesDecrypted = new CryptoLibrary().AES_Decrypt(bytesAlreadyEncrypted, passwordBytes);
                decryptedPassword = Encoding.UTF8.GetString(bytesDecrypted);
            }
            catch (Exception ex)
            {
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": failed to decrypt password.");
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": " + ex.ToString());
                Llog.Error("Thread " + thread + ": failed to decrypt password.");
                Llog.Error("Thread " + thread + ": " + ex.ToString());

                subject = "CCSVM AdhocScan: [Failure] Failed to read the password file";
                body = ex.ToString();
                Library.sendEmail(emailAddressFrom, emailList, subject, body);
                Environment.Exit(1);
            }

            return decryptedPassword;
        }

        public void validateToPrceedAdhocScan(bool includeError, int countOutRange, int validateOutofRange, string outofrangeIP, 
            int countInclude, string includedIP, string requester, string requesterEmailID, string scanTemplate, string country, 
            FileInfo csvFileInfo, string processedFileFullPath, string failedFileFullPath, int thread)
        {
            string csvFile = csvFileInfo.ToString();
            string timeStamp = csvFileInfo.Name.Split('-')[1];
            string siteName = "AdhocRequest-" + requester + "-" + timeStamp;
            string auditReportName = requester + "-CCSVM-Audit-" + timeStamp;
            string adminReportName = requester + "-CCSVM-Admin-" + timeStamp;
            string emailList = adminEmailAddress + "," + requesterEmailID;

            if (includeError == true)
            {
                Llog.Error("Thread " + thread + ": Error when checking the excluded list");
                Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Error when checking the excluded list",csvFileInfo.Name);

                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Error when checking the excluded list");
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Error when checking the excluded list", csvFileInfo.Name);

                subject = "CCSVM Adhoc Scan: [Error] Failed when checking the included list";
                body = "Have error when checking included list file. Please verify the data in included list file.";
                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                File.Move(csvFile, failedFileFullPath);
            }
            else
            {

                if (countOutRange == validateOutofRange)
                {
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": ALL IPs except Excluded IPs are out of your country range.");
                    Llog.Warn("Thread " + thread + ": ALL IPs: " + outofrangeIP + "  except Excluded IPs are out of your country range.");

                    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": ALL IPs: " + outofrangeIP +" except Excluded IPs are out of your country range.", csvFileInfo.Name);

                    listIPs = outofrangeIP.Replace(",", "<br>");
                    subject = "CCSVM Adhoc Scan: [Failed] ALL IP(s) are found out of range of your country/region for the request";
                    body = "All IP(s) are out of range of your country/region for the request as listed below: <br>" + listIPs 
                        + "<p>The request file is: <br>" + csvFileInfo.Name + "</p>";
                    Library.sendEmail(emailAddressFrom, emailList, subject, body);
                    File.Move(csvFile, failedFileFullPath);
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);
                    Llog.Info("Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);

                    failedLog.Info(csvFile); //----To be Completed
                    csvProcessedFail = true;

                }
                else if (countOutRange < validateOutofRange)
                {
                    if (countOutRange > 0)
                    {
                        //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Found out of range IPs: " + outofrangeIP);
                        Llog.Warn("Thread " + thread + ": Found out of range IPs: " + outofrangeIP);

                        Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Found out of range IPs: " + outofrangeIP, csvFileInfo.Name);


                        subject = "CCSVM Adhoc Scan: [Failed] Found IP(s) are out of range of your country/region for the request";
                        listIPs = outofrangeIP.Replace(",", "<br>");
                        body = "Found IP(s) are out of range of your country/region for the request as listed below: <br>" + listIPs
                             + "<p> The rest of IPs will be processed for Adhoc Scan.</p>" + "<p>The request file is: <br>" + csvFileInfo.Name + "</p>";

                        Library.sendEmail(emailAddressFrom, emailList, subject, body);
                    }

                    ParseScanEngine parsedValue = new ParseScanEngine();

                    if (includedIP.StartsWith("\""))
                    {
                        includedIP = includedIP.Remove(0, 1);
                    }

                    CheckIP checkIP = new CheckIP();
                    parsedValue = checkIP.GetScanEngineDetails(includedIP, country, thread);
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Successfully get the Scan Engine detail.");
                    Llog.Info("Thread " + thread + ": Successfully get the Scan Engine detail.");


                    List<string> distinctScanEngineList = parsedValue.scanEngineList.Distinct().ToList();

                    foreach (string eachScanEngine in distinctScanEngineList)
                    {
                        string newIPScanString = "";
                        foreach (KeyValuePair<string, string> kvp in parsedValue.mappedIPScanEngine)
                        {
                            if (eachScanEngine == kvp.Value)
                            {
                                newIPScanString += kvp.Key + ",";
                            }
                        }

                        newIPScanString = newIPScanString.Remove(newIPScanString.Length - 1);
                        newIPScanString += dquote;

                                     /*}
                      else if (countInclude > 0)
                      {*/
                    /*listIPs = includedIP;
                    includedIP = includedIP.Remove(includedIP.Length - 1);
                    includedIP += dquote;*/

                        parseSPInfo parseSP = parseSPSiteFile(country, 0);

                        string ccsVMServerPassword = passwordDecryption(requesterEmailID, thread);
                        string scriptParameter = "\"" + newIPScanString + " " + siteName + " " + adminReportName + " " + auditReportName + " \"" + scanTemplate + "\"" + " \"" + eachScanEngine + "\"";

                        //call ruby scriptParameter
                        //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Calling ruby script: " + adhocScanReportScript + " " + scriptParameter);
                        Llog.Info("Thread " + thread + ": Calling ruby script: " + adhocScanReportScript + " " + scriptParameter);

                        string resultScan = CallRuby.runScanScript(adhocScanReportScript, ccsVMServerIP, ccsVMServerPort, ccsVMServerUser, ccsVMServerPassword, scriptParameter, thread);
                        //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Callying ruby result: " + resultScan);
                        Llog.Info("Thread " + thread + ": Callying ruby result: " + resultScan);


                        string auditReportPath = reportPath + auditReportName;
                        string adminReportPath = reportPath + adminReportName;
                        string auditReportFullPath = auditReportPath + ".pdf";
                        string adminReportFullPath = adminReportPath + ".csv";
                        string auditReportFullName = auditReportName + ".pdf";
                        string adminReportFullName = adminReportName + ".csv";
                        bool auditReportExists = false;
                        bool adminReportExists = false;

                        if (resultScan.Contains("scansuccess"))
                        {
                            if (File.Exists(auditReportFullPath))
                            {
                                auditReportExists = true;
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Adhoc scan completed and audit report generated: " + auditReportFullPath);
                                Llog.Info("Thread " + thread + ": Adhoc scan completed and audit report generated: " + auditReportFullPath);

                            }
                            else
                            {
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Adhoc scan completed howerver audit report cannot be located: " + auditReportFullPath);
                                Llog.Warn("Thread " + thread + ": Adhoc scan completed howerver audit report cannot be located: " + auditReportFullPath);
                                Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Adhoc scan completed howerver audit report cannot be located: " + auditReportFullPath, csvFileInfo.Name);
                            }

                            if (File.Exists(adminReportFullPath))
                            {
                                adminReportExists = true;
                                Llog.Info("Thread " + thread + ": Adhoc scan completed and admin report generated: " + adminReportFullPath);
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Adhoc scan completed and admin report generated: " + adminReportFullPath);

                            }
                            else
                            {
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Adhoc scan completed howerver admin report cannot be located: " + adminReportFullPath);
                                Llog.Warn("Thread " + thread + ": Adhoc scan completed howerver admin report cannot be located: " + adminReportFullPath);
                                Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Adhoc scan completed howerver admin report cannot be located: " + adminReportFullPath, csvFileInfo.Name);
                            }

                            if (auditReportExists && adminReportExists) 
                            {
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Adhoc scan completed and report generated");
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Start uploading reports to Sharepoint");

                                Llog.Info("Thread " + thread + ": Adhoc scan completed and report generated");
                                Llog.Info("Thread " + thread + ": Start uploading reports to Sharepoint");
                                //Upload reports
                                //emails to requestor                           
                                //call API to upload reports
                                DateTime requestDate = DateTime.ParseExact(timeStamp, "yyyyMMddHHmmssffff", System.Globalization.CultureInfo.InvariantCulture);
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Request time: " + requestDate.ToLongDateString());
                                string uploadFolder = "";
                                bool auditReportUploaded = false;
                                bool adminReportUploaded = false;

                                if (requestDate.Month < 10)
                                {
                                    uploadFolder = "AdhocScan/" + requestDate.Year + "-0" + requestDate.Month;
                                }
                                else
                                {
                                    uploadFolder = "AdhocScan/" + requestDate.Year + "-" + requestDate.Month;
                                }

                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Upload folder: " + uploadFolder);
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Uploading audit report: " + auditReportFullPath);

                                Llog.Info("Thread " + thread + ": Upload folder: " + uploadFolder);
                                Llog.Info("Thread " + thread + ": Uploading audit report: " + auditReportFullPath);

                                string uploadedAuditPath = SPUpload.uploadReport(parseSP._spSite, uploadFolder, auditReportFullPath, thread);


                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Audit report upload result: " + uploadedAuditPath);
                                Llog.Info("Thread " + thread + ": Audit report upload result: " + uploadedAuditPath);

                                if (uploadedAuditPath.Contains("http"))
                                {
                                    auditReportUploaded = true;
                                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Audit report uploaded successfully: " + adminReportFullPath);
                                    Llog.Info("Thread " + thread + ": Audit report uploaded successfully: " + adminReportFullPath);

                                }
                                else
                                {
                                    auditReportUploaded = false;
                                    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Audit report failed to uploaded succes: " + adminReportFullPath, csvFileInfo.Name);
                                    Llog.Warn("Thread " + thread + ": Audit report failed to uploaded succes: " + adminReportFullPath);

                                }

                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Uploading admin report: " + adminReportFullPath);
                                Llog.Info("Thread " + thread + ": Uploading admin report: " + adminReportFullPath);

                                string uploadedAdminPath = SPUpload.uploadReport(parseSP._spSite, uploadFolder, adminReportFullPath, thread);
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Admin report upload result: " + uploadedAdminPath);
                                Llog.Info("Thread " + thread + ": Admin report upload result: " + uploadedAdminPath);
                                if (uploadedAuditPath.Contains("http"))
                                {
                                    adminReportUploaded = true;
                                    Llog.Info("Thread " + thread + ": Admin report uploaded successfully: " + adminReportFullPath);
                                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Admin report uploaded successfully: " + adminReportFullPath);

                                }
                                else
                                {
                                    adminReportUploaded = false;
                                    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Admin report failed to upload: " + adminReportFullPath, csvFileInfo.Name);
                                    Llog.Info("Thread " + thread + ": Admin report failed to upload: " + adminReportFullPath);
                                }

                                if (auditReportUploaded && adminReportUploaded)
                                {
                                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Both Audit and Admin reports uploaded successfully");
                                    Llog.Info("Thread " + thread + ": Both Audit and Admin reports uploaded successfully");

                                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Sending email to requestor");
                                    Llog.Info("Thread " + thread + ": Sending email to requestor");
                                    string subjectR = "CCSVM Adhoc Scan Reports: [Success] Uploaded Reports to SharePoint Succesfully ";
                                    string bodyR = "<b>Audit Report: </b><a href=\'" + uploadedAuditPath + "\'/>" + auditReportFullName + "</a><br><b>Admin Report: </b><a href=\'" + uploadedAdminPath + "\'/>" + adminReportFullName + "</a>";
                                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Email body: " + bodyR);
                                    Llog.Info("Thread " + thread + ": Email body: " + bodyR);

                                    Library.sendEmail(emailAddressFrom, requesterEmailID, subjectR, bodyR);
                                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Sent email to requestor");
                                    Llog.Info("Thread " + thread + ": Sent email to requestor");

                                    //generate the sharepoint link and send email, test whether the link valid
                                    //Move after successfully processed
                                    try
                                    {
                                        if (countOutRange == 0)
                                        {
                                            //File.Move(csvFile, processedFileFullPath);
                                            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The csv file " + csvFile + " has been moved to " + processedFileFullPath);
                                            Llog.Info("Thread " + thread + ": The csv file " + csvFile + " has been moved to " + processedFileFullPath);
                                            processLog.Info(csvFile); //--To be done
                                        }
                                        else
                                        {
                                            processedFileFullPath = processedFileFullPath + "_PartialCompleted";
                                            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The csv file " + csvFile + " has been moved to " + processedFileFullPath);
                                            Llog.Info("Thread " + thread + ": The csv file " + csvFile + " has been moved to " + processedFileFullPath);
                                            processLog.Info(csvFile);
                                        }

                                        csvProcessedSuccess = true;

                                    }
                                    catch (Exception ex)
                                    {
                                        //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Failed to move the csv File to processed folder: " + csvFile);
                                        //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": " + ex.ToString());
                                        Llog.Error("Thread " + thread + ": Failed to move the csv File to processed folder: " + csvFile);
                                        Llog.Error("Thread " + thread + ": " + ex.ToString());
                                        string subject = "CCSVM Adhoc Scan: [Failure] Failed to move the csv file to processed folder";
                                        //File.Move(csvFile, failedFileFullPath);
                                        failedLog.Info(csvFile); //----to be done
                                        body = "Failed to move the csv file \"" + csvFileInfo.Name + "\" to processed folder";
                                        //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);
                                        Llog.Warn("Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);
                                        Library.sendEmail(emailAddressFrom, emailList, subject, body);
                                        csvProcessedFail = true;
                                    }
                                }
                                else
                                {
                                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Either Audit or Admin report upload failed");
                                    Llog.Warn("Thread " + thread + ": Either Audit or Admin report upload failed");
                                    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Either Audit or Admin report upload failed", csvFileInfo.Name);
                                    string subjectR = "CCSVM Adhoc Scan: [Failure] Report uploading to sharepoint failed";
                                    //string bodyR = "<b>Audit Report: </b><a href=\'" + uploadedAuditPath + "\'/>" + auditReportName + "</a><br><b>Admin Report: </b><a href=\'" + uploadedAdminPath + "\'/>" + adminReportname + "</a>";
                                    string bodyR = auditReportFullPath + "<br>" + adminReportFullPath;
                                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Email body: " + bodyR);
                                    Llog.Info("Thread " + thread + ": Email body: " + bodyR);
                                    Library.sendEmail(emailAddressFrom, emailList, subjectR, bodyR);
                                    csvProcessedFail = true;
                                    //File.Move(csvFile, failedFileFullPath);
                                    failedLog.Info(csvFile); //---to be done
                                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);
                                    Llog.Info("Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);

                                }
                            }
                            else
                            {
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": One or more reports didn't generated duing Adhoc Scan");
                                Llog.Warn("Thread " + thread + ": One or more reports didn't generated duing Adhoc Scan");

                                Library.WriteErrorLog(logFilePath, "Thread " + thread + ": One or more reports didn't generated duing Adhoc Scan for the following IPs: " + includedIP, csvFileInfo.Name);

                                string subject = "CCSVM Adhoc Scan: [Failure] One or more reports didn't generated duing Adhoc Scan";
                                //File.Move(csvFile, failedFileFullPath);

                                //includedIP = includedIP.Remove(0, 1);
                                includedIP = includedIP.Replace(",", "<br>");
                               // listIPs = listIPs.Remove(0, 1);
                               // listIPs = listIPs.Replace(",", "<br>");

                                body = "<p>One or more reports didn't generated duing Adhoc Scan for following IPs: <br>"
                                    + includedIP + "</p><p>The request file is: <br>" + csvFileInfo.Name + "</p>";
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);

                                Library.sendEmail(emailAddressFrom, emailList, subject, body);
                                csvProcessedFail = true;
                            }
                        }
                        else
                        {
                            //File.Move(csvFile, failedFileFullPath);

                            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Adhoc scan failed, check the logs: " + csvFile);
                            Llog.Warn("Thread " + thread + ": Adhoc scan failed, check the logs: " + csvFile);

                            Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Adhoc scan failed, check the logs: " + csvFile, csvFileInfo.Name);


                            subject = "CCSVM Adhoc Scan: [Failure] Scan failed";
                            Library.WriteErrorLog(logFilePath, "Thread " + thread + ": CCSVM Adhoc Scan failed for following IPs: " + listIPs.Replace("<br>", ", "), csvFileInfo.Name);

                            listIPs = listIPs.Replace(",", "<br>");

                            //includedIP = includedIP.Remove(0, 1);
                            //includedIP = includedIP.Replace(",", "<br>");
                            body = "<p>The CCSVM Adhoc Scan failed for following IPs: <br>"
                                + listIPs + "</p><p>The request file is: <br>" + csvFileInfo.Name + "</p>";

                            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);
                            Library.sendEmail(emailAddressFrom, emailList, subject, body);
                            csvProcessedFail = true;

                            if (File.Exists(auditReportFullPath))
                            {
                                File.Delete(auditReportFullPath);
                            }
                            if (File.Exists(adminReportFullPath))
                            {
                                File.Delete(adminReportFullPath);
                            }
                            //delete site
                            string deleteSiteResult = CallRuby.runDeleteSiteScript(deleteSite, ccsVMServerIP, ccsVMServerPort, ccsVMServerUser, ccsVMServerPassword, siteName, thread);
                            if (deleteSiteResult.Contains("deletesuccess"))
                            {
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Site delete from CCSVM: " + siteName);
                                Llog.Info("Thread " + thread + ": Site delete from CCSVM: " + siteName);

                            }
                            else
                            {
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Site already deleted from CCSVM: " + siteName);
                                Llog.Info("Thread " + thread + ": Site already deleted from CCSVM: " + siteName);
                                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Site already deleted from CCSVM: " + siteName, csvFileInfo.Name);

                            }
                        }
                    }
                }

                if (csvProcessedFail && File.Exists(csvFile))
                {
                    File.Move(csvFile, failedFileFullPath);
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);
                    Llog.Info("Thread " + thread + ": The csv file " + csvFile + " has been moved to " + failedFileFullPath);

                    failedLog.Info(csvFile);

                }

                if (csvProcessedSuccess && File.Exists(csvFile))
                {
                    File.Move(csvFile, processedFileFullPath);
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The csv file " + csvFile + " has been moved to " + processedFileFullPath);
                    Llog.Info("Thread " + thread + ": The csv file " + csvFile + " has been moved to " + processedFileFullPath);
                    processLog.Info(csvFile);
                }
            }
        }
    }

}