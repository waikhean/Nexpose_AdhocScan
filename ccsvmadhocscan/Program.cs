using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ccsvmadhocscan
{
    static class Program
    {
        static private ILog Llog;//= log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static public String log4netConfigFile; // = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\log4net.config";
        static public String NexposeAdhocScanLog;
        static private ILog Logger;//= log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static public String saveSiteLog;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

            string programPath = ConfigurationManager.AppSettings["programPath"];
            string logPath = ConfigurationManager.AppSettings["logPath"];
            string logFileSize = ConfigurationManager.AppSettings["logFileSize"];
            string emailServer = ConfigurationManager.AppSettings["smtpServer"];
            string emailPort = ConfigurationManager.AppSettings["smtpServerPort"];
            string logFileName = ConfigurationManager.AppSettings["logFileName"];
            string logFilePath = logPath + logFileName;
            string rubyPath = ConfigurationManager.AppSettings["rubyPath"];
            string reportPath = ConfigurationManager.AppSettings["reportPath"];
            string emailAddressFrom = ConfigurationManager.AppSettings["emailAddress"];
            string adminEmailAddress = ConfigurationManager.AppSettings["adminEmailAddress"];
            string adhocScanReportScript = rubyPath + @"scripts\run_adhoc_scan_reports.rb";
            string includedFilePath = ConfigurationManager.AppSettings["csvFileinclude"];
            string excludedFilePath = ConfigurationManager.AppSettings["csvFileExclude"];
            string ccsVMServerIP = ConfigurationManager.AppSettings["ccsVMServerIP"];
            string ccsVMServerPort = ConfigurationManager.AppSettings["ccsVMServerPort"];
            string ccsVMServerUser = ConfigurationManager.AppSettings["ccsVMServerUser"];
            string passworFile = ConfigurationManager.AppSettings["ccsVMServerPassword"];
           // string csvFilePath = ConfigurationManager.AppSettings["csvFilePath"];
            string csvFilePathFailed = ConfigurationManager.AppSettings["csvFilePathFailed"];
            string csvFilePathProcessed = ConfigurationManager.AppSettings["csvFilePathProcessed"];
            string getSiteIncluded = rubyPath + @"scripts\get_site_included_range.rb";
            string getSiteExcluded = rubyPath + @"scripts\get_site_excluded_range.rb";
            string tempLogFilePath = Directory.GetCurrentDirectory().ToString() + @"\" + logFileName;
            //to check whether all the path existis
            

            try
            {

                saveSiteLog = ConfigurationManager.AppSettings["saveSiteErrorLog"];
                Logger = log4net.LogManager.GetLogger(saveSiteLog);

                NexposeAdhocScanLog = ConfigurationManager.AppSettings["NexposeAdhocScanErrorLog"];
                Llog = log4net.LogManager.GetLogger(NexposeAdhocScanLog);
                log4netConfigFile = ConfigurationManager.AppSettings["Log4netConfig"];
                log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4netConfigFile));

                if (!(Directory.Exists(programPath)))
                {
                    Llog.Info("programPath doesn't exist: " + programPath);
                    try
                    {
                        Directory.CreateDirectory(programPath);
                        Llog.Info("Created folder: " + programPath);
                    }
                    catch (Exception ex)
                    {
                        Llog.Info(ex.ToString());
                        string subject = "CCSVM SaveSites: Failed to create folder: " + programPath;
                        string body = ex.ToString();
                        Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                        Environment.Exit(1);
                    }
                }

                if (!(Directory.Exists(logPath)))
                {
                    Llog.Info("logPath doesn't exist: " + logPath);
                    try
                    {
                        Directory.CreateDirectory(logPath);
                        Llog.Info("Created folder: " + logPath);
                    }
                    catch (Exception ex)
                    {
                        Llog.Info(ex.ToString());
                        string subject = "CCSVM SaveSites: Failed to create folder: " + logPath;
                        string body = ex.ToString();
                        Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                        Environment.Exit(1);
                    }
                }

                if (!(Directory.Exists(rubyPath)))
                {
                    Llog.Info("Failed to find ruby folder: " + rubyPath);
                    string subject = "CCSVM SaveSites: Failed to find ruby folder";
                    string body = "Failed to find ruby folder: " + rubyPath;
                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                    Environment.Exit(1);
                }

                if (!File.Exists(passworFile))

                {
                    //Library.WriteErrorLog(logFilePath, "Failed to find the password file: " + passworFile);
                    Llog.Error("Failed to find the password file: " + passworFile);

                    string subject = "CCSVM SaveSites: Failed to find the password file";
                    string body = "Failed to find the password file: " + passworFile;
                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                    Environment.Exit(1);
                }

                if (!(File.Exists(adhocScanReportScript)))
                {
                    //Library.WriteErrorLog(logPath, "Cannot find ruby script: " + adhocScanReportScript);
                    Llog.Error("Cannot find ruby script: " + adhocScanReportScript);

                    string subject = "CCSVM SaveSites: Cannot find ruby script";
                    string body = "Cannot find ruby script" + adhocScanReportScript;
                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                    Environment.Exit(1);
                }

                if (!(File.Exists(getSiteIncluded)))
                {
                    //Library.WriteErrorLog(logPath, "Cannot find ruby script: " + getSiteIncluded);
                    Llog.Error("Cannot find ruby script: " + getSiteIncluded);

                    string subject = "CCSVM SaveSites: Cannot find ruby script";
                    string body = "Cannot find ruby script" + getSiteIncluded;
                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                    Environment.Exit(1);
                }

                if (!(File.Exists(getSiteExcluded)))
                {
                    //Library.WriteErrorLog(logFilePath, "Cannot find ruby script: " + getSiteExcluded);
                    Llog.Error("Cannot find ruby script: " + getSiteExcluded);
                    string subject = "CCSVM SaveSites: Cannot find ruby script";
                    string body = "Cannot find ruby script" + getSiteExcluded;
                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                    Environment.Exit(1);
                }


                //Library.WriteErrorLog(logFilePath, "========================================================================");
                //Library.WriteErrorLog(logFilePath, "All required files and folders exists, starting CCSVM Adhoc Scan service");

                Llog.Info("========================================================================");
                Llog.Info("All required files and folders exists, starting CCSVM Adhoc Scan service");

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new ccsvmadhocscan()
                };
                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
                Llog.Error("Exception during start CCSVM Adhoc Scan service: " + ex.ToString());

                Environment.Exit(1);
            }
        }
    }
}
