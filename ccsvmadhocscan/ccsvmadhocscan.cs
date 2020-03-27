using CNAVCryptoLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using log4net;

namespace ccsvmadhocscan
{

    public partial class ccsvmadhocscan : ServiceBase
    {
        static string programPath = ConfigurationManager.AppSettings["programPath"];
        static string logPath = ConfigurationManager.AppSettings["logPath"];
        static string logFileSize = ConfigurationManager.AppSettings["logFileSize"];
        static string emailServer = ConfigurationManager.AppSettings["smtpServer"];
        static string emailPort = ConfigurationManager.AppSettings["smtpServerPort"];
        static string logFileName = ConfigurationManager.AppSettings["logFileName"];
        static string logFilePath = logPath + logFileName;
        static string logFileSaveSite = ConfigurationManager.AppSettings["logFileSaveSite"];
        static string logFileSaveSitePath = logPath + logFileSaveSite;
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
        static string maxRunningScan = ConfigurationManager.AppSettings["maxRunningScans"];
        static int maxRunningScans = Convert.ToInt32(maxRunningScan);
        static string threadLogFileName = ConfigurationManager.AppSettings["threadLogFileName"];
        static string csvFailedProcess = ConfigurationManager.AppSettings["csvfailedprocess"];
        static string csvProcessed = ConfigurationManager.AppSettings["csvprocessed"];
        static string csvFailedProcessLogPath = logPath + csvFailedProcess;
        static string csvProcessedLogPath = logPath + csvProcessed;

        static private ILog Llog;
        static private ILog Logger;

        static public String log4netConfigFile; // = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\log4net.config";
        static public String NexposeAdhocScanLog;
        static public String saveSiteLog;

        //timer1 is for adhoc scan 
        private System.Timers.Timer timer1 = null;

        //timer2 is for save site data
        private System.Timers.Timer timer2 = null;

        public ccsvmadhocscan()
        {
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            try
            {
                NexposeAdhocScanLog = ConfigurationManager.AppSettings["NexposeAdhocScanErrorLog"];
                Llog = log4net.LogManager.GetLogger(NexposeAdhocScanLog);
                log4netConfigFile = ConfigurationManager.AppSettings["Log4netConfig"];
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4netConfigFile));
                saveSiteLog = ConfigurationManager.AppSettings["saveSiteErrorLog"];
                Logger = log4net.LogManager.GetLogger(saveSiteLog);


                timer1 = new System.Timers.Timer();
                string timeInterval1 = ConfigurationManager.AppSettings["scanInterval"];
                this.timer1.Interval = Int32.Parse(timeInterval1);
                this.timer1.Elapsed += new ElapsedEventHandler(this.timer1_Tick);
                int timer1Seconds = Int32.Parse(timeInterval1) / 1000;
                timer1.Enabled = true;
                //Library.WriteErrorLog(logFilePath, "Timer set for Adhoc Scan, it will tick every " + timer1Seconds + " seconds");
                Llog.Info("Timer set for Adhoc Scan, it will tick every " + timer1Seconds + " seconds");
                timer2 = new System.Timers.Timer();
                string timeInterval2 = ConfigurationManager.AppSettings["siteInterval"];
                this.timer2.Interval = Int32.Parse(timeInterval2);
                int timer2Seconds = Int32.Parse(timeInterval2) / 1000;
                this.timer2.Elapsed += new ElapsedEventHandler(this.timer2_Tick);
                timer2.Enabled = true;                
                Llog.Info("Timer set for Save Site, it will tick every " + timer2Seconds + " seconds");
                Llog.Info("Refer to ccsvmsavesites.log for the detail logs");
                Llog.Info("CCSVM Adhoc Scan service version 1.0.4.1 started");
                Llog.Info("Waiting for timer to tick");
            }
            catch (Exception ex)
            {
                Llog.Error("Exception occurred during service startup: " + ex.ToString());

                System.Environment.Exit(-1);
            }
        }

        protected override void OnStop()
        {
            try
            {
                //Library.WriteErrorLog(logFilePath, "CCSVM Adhoc Scan service stopping");
                timer1.Enabled = false;
                //Library.WriteErrorLog(logFilePath, "Timer for Adhoc Scan disabled");
                timer2.Enabled = false;
                //Library.WriteErrorLog(logFilePath, "Timer for Save Site disabled");
                //Library.WriteErrorLog(logFilePath, "All timers are disabled, CCSVM Adhoc Scan service stopped");
                //Library.WriteErrorLog(logFilePath, "========================================================================");

                Llog.Info("CCSVM Adhoc Scan service stopping");
                //timer1.Enabled = false;
                Llog.Info("Timer for Adhoc Scan disabled");
                //timer2.Enabled = false;
                Llog.Info("Timer for Save Site disabled");
                Llog.Info("All timers are disabled, CCSVM Adhoc Scan service stopped");
                Llog.Info("========================================================================");

            }
            catch (Exception ex)
            {
                //Library.WriteErrorLog(logFilePath, "Exception occurred during service stop: " + ex.ToString());
                Llog.Error("Exception occurred during service stop: " + ex.ToString());
                System.Environment.Exit(-1);
            }

        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            timer1.Enabled = false;
            //DateTime today = new DateTime();
            //keep track the running scans
            int currentRunningScans = 0;
            //Library.WriteErrorLog(logFilePath, "Timer for Adhoc Scan ticked");
            //Get email csv files
            //Library.WriteErrorLog(logFilePath, "Starting to check new adhoc scan emails");

            Llog.Info("Timer for Adhoc Scan ticked");
            Llog.Info("Starting to check new adhoc scan emails");

            //int csvFileCountNew = 0;
            //try
            //{
            //    //return new email count
            //    // 0 means no new emails
            //    // -1 means not able to open Outlook
            //    // -2 means unknow error
            //    //csvFileCountNew = SaveAttachment.getEmails();               
            //    //int csvFileCountNew = 0;
            //    if (csvFileCountNew > 0)
            //    {
            //        Library.WriteErrorLog(logFilePath, "Total new adhoc scan requests:  " + csvFileCountNew);
            //    }
            //    else if (csvFileCountNew == 0)
            //    {
            //        Library.WriteErrorLog(logFilePath, "No new adhoc scan request: " + csvFileCountNew);
            //    }
            //    else if (csvFileCountNew < 0)
            //    {
            //        Library.WriteErrorLog(logFilePath, "Error during processing save attachments: " + csvFileCountNew);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Library.WriteErrorLog(logFilePath, "Failed to check new emails: " + ex.ToString());
            //    string subject = "CCSVM AdhocScan: Failed to check new emails";
            //    string body = "Failed to check new emails " + ex.ToString();
            //    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
            //}
            //reading email folder
            string[] csvFiles = { };
            //string[] csvColumns = { "SharepointRecordID", "AssetIP", "Requester", "RequesterEmailID", "RequesterComments", "Country", "ScanTemplate" };
            string[] csvColumns = { "SharepointRecordID", "AssetIP", "Requester", "RequesterEmailID", "Country", "ScanTemplate" };
            try
            {
                //Library.WriteErrorLog(logFilePath, "Search for csv files in folder:  " + csvFilePath);
                Llog.Info("Search for csv files in folder:  " + csvFilePath);

                string[] getcsvFiles = Directory.GetFiles(csvFilePath, "*CCSVM_Adhoc_Scan*.csv*");
                //Library.WriteErrorLog(logFilePath, "Total csv files found: " + getcsvFiles.Length);
                Llog.Info("Total csv files found: " + getcsvFiles.Length);

                for (int i = 0; i < getcsvFiles.Length; i++)
                {
                    if (!getcsvFiles[i].Contains("-"))
                    {
                        //Library.WriteErrorLog(logFilePath, "csv file: " + getcsvFiles[i] + " doesn't contain timestamp, appending timestamp");
                        Llog.Info("csv file: " + getcsvFiles[i] + " doesn't contain timestamp, appending timestamp");

                        getcsvFiles[i] = getcsvFiles[i] + "-" + GetTimestamp(DateTime.Now);
                    }
                }
                foreach (string item in getcsvFiles)
                {
                    //Library.WriteErrorLog(logFilePath, "csv file: " + item);
                    Llog.Info("csv file: " + item);

                }
                csvFiles = getcsvFiles;
            }
            catch (Exception ex)
            {
                //Library.WriteErrorLog(logFilePath, "Failed to get csv file list from directory: " + csvFilePath);
                //Library.WriteErrorLog(logFilePath, ex.ToString());
                Llog.Error("Failed to get csv file list from directory: " + csvFilePath);
                Llog.Error(ex.ToString());
                string subject = "CCSVM AdhocScan: Failed to get csv file list from directory";
                string body = "Failed to get csv file list from directory: " + csvFilePath;
                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
            }
            //Start scanning and get reports
            //define the maximum current running scans
            Thread[] childThreads = new Thread[maxRunningScans];
            var watch = new System.Diagnostics.Stopwatch();

            int j = 0;
            int totalScans = 0;
            AdhocScan AdhocScan = new AdhocScan();

            if (csvFiles.Count() == 0)
            {
                //Library.WriteErrorLog(logFilePath, "No csv files found");
                Llog.Info("No csv files found");

            }
            else
            {
                while (true)
                {
                    currentRunningScans = 0;
                    //checking for running scans
                    for (int i = 0; i < maxRunningScans; i++)
                    {
                        try
                        {
                            if (childThreads[i].IsAlive)
                            {
                                //assign the thread which not alive, use the next available
                                currentRunningScans++;
                            }
                        }
                        catch (Exception)
                        {
                            //Library.WriteErrorLog(logFilePath, "Thread: " + i + " not running or not exist");
                        }
                    }

                    Llog.Info("Current running scans: " + currentRunningScans);
                    //Library.WriteErrorLog(logFilePath, "Current running scans: " + currentRunningScans);

                    if (currentRunningScans == maxRunningScans)
                    {
                        //already hit max running scans, sleep 5s
                        Llog.Info("Hit max running scans count, sleep for 60s");
                        //Library.WriteErrorLog(logFilePath, "Hit max running scans count, sleep for 60s");


                        Thread.Sleep(60000);
                        
                        continue;
                    }
                    else if (totalScans == csvFiles.Count())
                    {
                        //all the csv files have been processed
                        //check if any thread running
                        if (currentRunningScans > 0)
                        {
                            //TimeSpan timeSpan = watch.Elapsed;
                            //string a = "Time: " + timeSpan.Minutes + "m ";
                            //Library.WriteErrorLog(logFilePath, a);
                            //int proccessing = timeSpan.Minutes;
                            

                            for (int i = 0; i < maxRunningScans; i++)
                            {
                                try
                                {
                                    if (childThreads[i].IsAlive)
                                    {
                                        Llog.Info("Thread " + i + ": still running");
                                        //Library.WriteErrorLog(logFilePath, "Thread " + i + ": still running");



                                    }
                                }
                                catch (Exception)
                                {
                                    //Library.WriteErrorLog(logFilePath, "Thread: " + i + " not running or not exist");
                                }
                            }
                            Llog.Info("Threads not finish running, sleep for 60s");
                            //Library.WriteErrorLog(logFilePath, "Threads not finish running, sleep for 60s");

                            Thread.Sleep(60000);
                            continue;
                        }
                        else
                        {
                            Llog.Info("All threads finish running");
                            Llog.Info("All the csv files have been processed");
                            //Library.WriteErrorLog(logFilePath, "All threads finish running");
                            //Library.WriteErrorLog(logFilePath, "All the csv files have been processed");
                            break;
                        }
                    }
                    else if (totalScans < csvFiles.Count())
                    {
                        Llog.Info("Start to trigger Adhoc Scan for csv file: " + csvFiles[totalScans]);
                        //Library.WriteErrorLog(logFilePath, "Start to trigger Adhoc Scan for csv file: " + csvFiles[totalScans]);

                        //set the free thread to scan
                        if (totalScans >= maxRunningScans)
                        {
                            for (int k = 0; k < maxRunningScans; k++)
                            {
                                try
                                {
                                    if (!childThreads[k].IsAlive)
                                    {
                                        //assign the thread which not alive, use the next available
                                        j = totalScans % maxRunningScans + k;
                                        break;
                                    }
                                }
                                catch (Exception)
                                {
                                    //Library.WriteErrorLog(logFilePath, "Thread: " + k + " not running or not exist");
                                }
                            }
                        }
                        else
                        {
                            j = totalScans;
                        }
                        try
                        {
                            ThreadStart childref = null;
                            string currentCsvFile = csvFiles[totalScans];
                            int currentThread = j;
                            childref = new ThreadStart(() => AdhocScan.runScan(currentCsvFile, currentThread));
                            childThreads[j] = new Thread(childref);
                            Llog.Info("Created thead " + currentThread + ":" + currentCsvFile);
                            //Library.WriteErrorLog(logFilePath, "Created thead " + currentThread + ":" + currentCsvFile);

                            //watch.Start();

                            Random rnd = new Random();
                            int interval = rnd.Next(10, 61);
                            Llog.Info("Starting new scan in " + interval + " seconds under thread: " + j);
                            //Library.WriteErrorLog(logFilePath, "Starting new scan in " + interval + " seconds under thread: " + j);

                            System.Threading.Thread.Sleep(interval * 1000); //wait for a random time and start next thread 
                            childThreads[j].Start();
                            totalScans++;
                        }
                        catch (Exception ex)
                        {
                            //Library.WriteErrorLog(logFilePath, "Failed to start thread: " + ex.ToString());
                            //Library.WriteErrorLog(logFilePath, "Retry...");
                            Llog.Error("Failed to start thread: " + ex.ToString());
                            Llog.Error("Retry...");
                        }
                    }
                }
            }

            //Library.CheckErrorLogSize(logFilePath);
            //Library.CheckErrorLogSize(csvProcessedLogPath);
            //Library.CheckErrorLogSize(csvFailedProcessLogPath);
            timer1.Enabled = true;
        }

        private void timer2_Tick(object sender, ElapsedEventArgs e)
        {
            timer2.Enabled = false;
            

            Logger.Info("Timer for Save Site ticked");
            Logger.Info( "Start to save site");
            int getSiteResult =  SaveSite.getSite();
            if (getSiteResult == 1)
            {
                //Library.WriteErrorLog(logFileSaveSitePath, "New site list file created successfully, however error duing processing ");
                Logger.Warn( "New site list file created successfully, however error duing processing ");

                //send error email
            }
            else if (getSiteResult == 2)
            {
                //Library.WriteErrorLog(logFileSaveSitePath, "New stie list successfully created and replaced");
                Logger.Info( "New stie list successfully created and replaced");


                //send error email
            }
            else if (getSiteResult == -1)
            {
                //Library.WriteErrorLog(logFileSaveSitePath, "Not able to move the result files, save site failed");
                Logger.Warn( "Not able to move the result files, save site failed");

                //send error email
            }
            else if (getSiteResult == -2)
            {
                //Library.WriteErrorLog(logFileSaveSitePath, "Get site failed due to file issue, check the files");
                Logger.Warn( "Get site failed due to file issue, check the files");

                //send error email
            }
            //Library.CheckErrorLogSize(logFileSaveSitePath);
            timer2.Enabled = true;
        }

        //private void timer3_Tick(object sender, ElapsedEventArgs e)
        //{
        //    timer1.Enabled = false;
        //    Library.WriteErrorLog(logFileSaveSitePath, "Timer for Test ticked");
        //    Library.WriteErrorLog(logFileSaveSitePath, "Start testing");
        //    string timeStamp = GetTimestamp(DateTime.Now);
        //    int thread = 99;
        //    string spSite = "sgdc";
        //    string aduitReportPath = @"D:\ccsadhocscan\ccsvmadhocscan\reports\Administrator-CCSVM-Audit-192.168.112.1-201601311642553264.pdf";
        //    string adminreportPath = @"D:\ccsadhocscan\ccsvmadhocscan\reports\Administrator-CCSVM-Admin-192.168.112.1-201601311642553264.csv";
        //    string auditReportName = "Audit";
        //    string adminReportname = "Admin";
        //    string assetIP = "192.168.112.1";
        //    string requesterEmailID = "Administrator@ibm.local";
        //    //spupload
        //    //SPUpload.uploadReport("sgdc", "2015-12", @"D:\ccsadhocscan\ccsvmadhocscan\reports\huan-CCSVM-Audit-192.168.112.1.pdf");
        //    DateTime requestDate = DateTime.ParseExact(timeStamp, "yyyyMMddHHmmssffff", System.Globalization.CultureInfo.InvariantCulture);
        //    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Request time: " + requestDate.ToLongDateString());
        //    string uploadFolder = "";
        //    if (requestDate.Month < 10)
        //    {
        //        uploadFolder = "AdhocScan/" + requestDate.Year + "-0" + requestDate.Month;
        //    }
        //    else
        //    {
        //        uploadFolder = "AdhocScan/" + requestDate.Year + "-" + requestDate.Month;
        //    }

        //    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Upload folder: " + uploadFolder);
        //    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Uploading audit report: " + aduitReportPath);
        //    string uploadedAuditPath = SPUpload.uploadReport(spSite, uploadFolder, aduitReportPath, thread);
        //    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Uploaded admin report: " + uploadedAuditPath);


        //    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Uploading admin report: " + adminreportPath);
        //    string uploadedAdminPath = SPUpload.uploadReport(spSite, uploadFolder, adminreportPath, thread);
        //    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Uploaded admin report: " + uploadedAdminPath);

        //    string subjectR = "CCSVM Adhoc Scan Reports: " + assetIP;
        //    string bodyR = "<b>Audit Report: </b><a href=\'" + uploadedAuditPath + "\'/>" + auditReportName + "</a><br><b>Admin Report: </b><a href=\'" + uploadedAdminPath + "\'/>" + adminReportname + "</a>";
        //    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Email body: " + bodyR);

        //    Library.sendEmail(emailAddressFrom, requesterEmailID, subjectR, bodyR);
        //    Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Finished testing");
        //    //generate the sharepoint link and send email, test whether the link valid
        //    //timer3.Enabled = true;
        //}

    }
}
