using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;


namespace ccsvmadhocscan
{
    static class CallRuby
    {
        static string logPath = ConfigurationManager.AppSettings["logPath"];
        static string logFileName = ConfigurationManager.AppSettings["logFileName"];
        static string logFilePath = logPath + logFileName;
        static string logFileSaveSite = ConfigurationManager.AppSettings["logFileSaveSite"];
        static string logFileSaveSitePath = logPath + logFileSaveSite;
        static string rubyPath = ConfigurationManager.AppSettings["rubyPath"];
        static string emailAddressFrom = ConfigurationManager.AppSettings["emailAddress"];
        static string adminEmailAddress = ConfigurationManager.AppSettings["adminEmailAddress"];

        static private ILog Llog;//= log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static private ILog Logger;//= log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static public String log4netConfigFile; // = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\log4net.config";
        static public String NexposeAdhocScanLog;
        static public String saveSiteLog;


        public static string runScanScript(string scriptName, string serverName, string serverPort, string userName, string userPassword, string scriptParameter, int thread)
        {
            NexposeAdhocScanLog = ConfigurationManager.AppSettings["NexposeAdhocScanErrorLog"];
            Llog = log4net.LogManager.GetLogger(NexposeAdhocScanLog);
            log4netConfigFile = ConfigurationManager.AppSettings["Log4netConfig"];
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4netConfigFile));
            saveSiteLog = ConfigurationManager.AppSettings["saveSiteErrorLog"];
            Logger = log4net.LogManager.GetLogger(saveSiteLog);

            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Below parameter received:");
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": "  + scriptName);
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ":" + serverName);
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ":" + serverPort);
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ":" + userName);
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ":" + userPassword);
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ":" + scriptParameter);
            using (Process p = new Process())
            {
                try
                {
                    ProcessStartInfo info = new ProcessStartInfo(rubyPath + @"bin\ruby.exe");
                    //Library.WriteErrorLog(programLogFilePath, userPassword);
                    string password = userPassword.TrimEnd(Environment.NewLine.ToCharArray());
                    info.Arguments = scriptName + " " + serverName + " " + serverPort + " " + userName + " " + userPassword + " " + scriptParameter; // set args
                    //Library.WriteErrorLog(programLogFilePath, info.Arguments);
                    //Debug.WriteLine(info.Arguments);
                    info.RedirectStandardInput = true;
                    info.RedirectStandardOutput = true;
                    info.UseShellExecute = false;
                    p.StartInfo = info;
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return output;
                    // process output
                }
                catch (Exception ex)
                {
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Failed to trigger ruby script: " + scriptName);
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": " + ex.ToString());
                    Llog.Warn("Thread " + thread + ": Failed to trigger ruby script: " + scriptName);
                    Llog.Warn("Thread " + thread + ": " + ex.ToString());
                    string subject = "Failed to trigger ruby script " + scriptName;
                    string body = ex.ToString();
                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                    return "-1";
                }
            }
        }

        public static string runSiteScript(string scriptName, string serverName, string serverPort, string userName, string userPassword, string scriptParameter)
        {
            //Library.WriteErrorLog(logFileSaveSitePath, "Below parameter received:");
            //Library.WriteErrorLog(logFileSaveSitePath, scriptName);
            //Library.WriteErrorLog(logFileSaveSitePath, serverName);
            //Library.WriteErrorLog(logFileSaveSitePath, serverPort);
            //Library.WriteErrorLog(logFileSaveSitePath, userNmame);
            //Library.WriteErrorLog(logFileSaveSitePath, userPassword);
            //Library.WriteErrorLog(logFileSaveSitePath, scriptParameter);
            using (Process p = new Process())
            {
                try
                {
                    ProcessStartInfo info = new ProcessStartInfo(rubyPath + @"bin\ruby.exe");
                    //Library.WriteErrorLog(programLogFilePath, userPassword);
                    string password = userPassword.TrimEnd(Environment.NewLine.ToCharArray());
                    info.Arguments = scriptName + " " + serverName + " " + serverPort + " " + userName + " " + userPassword + " " + scriptParameter; // set args
                    //Library.WriteErrorLog(programLogFilePath, info.Arguments);
                    //Debug.WriteLine(info.Arguments);
                    info.RedirectStandardInput = true;
                    info.RedirectStandardOutput = true;
                    info.UseShellExecute = false;
                    p.StartInfo = info;
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return output;
                    // process output
                }
                catch (Exception ex)
                {
                    //Library.WriteErrorLog(logFilePath, "Failed to trigger ruby script: " + scriptName);
                    //Library.WriteErrorLog(logFileSaveSitePath, ex.ToString());
                    Logger.Warn("Failed to trigger ruby script: " + scriptName);
                    Logger.Warn(ex.ToString());
                    string subject = "Failed to trigger ruby script " + scriptName;
                    string body = ex.ToString();
                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                    return "-1";
                }
            }
        }

        public static string runDeleteSiteScript(string scriptName, string serverName, string serverPort, string userName, string userPassword, string scriptParameter, int thread)
        {
            //Library.WriteErrorLog(logFileSaveSitePath, "Below parameter received:");
            //Library.WriteErrorLog(logFileSaveSitePath, scriptName);
            //Library.WriteErrorLog(logFileSaveSitePath, serverName);
            //Library.WriteErrorLog(logFileSaveSitePath, serverPort);
            //Library.WriteErrorLog(logFileSaveSitePath, userName);
            //Library.WriteErrorLog(logFileSaveSitePath, userPassword);
            //Library.WriteErrorLog(logFileSaveSitePath, scriptParameter);
            using (Process p = new Process())
            {
                try
                {
                    ProcessStartInfo info = new ProcessStartInfo(rubyPath + @"bin\ruby.exe");
                    //Library.WriteErrorLog(programLogFilePath, userPassword);
                    string password = userPassword.TrimEnd(Environment.NewLine.ToCharArray());
                    info.Arguments = scriptName + " " + serverName + " " + serverPort + " " + userName + " " + userPassword + " " + scriptParameter; // set args
                    //Library.WriteErrorLog(programLogFilePath, info.Arguments);
                    //Debug.WriteLine(info.Arguments);
                    info.RedirectStandardInput = true;
                    info.RedirectStandardOutput = true;
                    info.UseShellExecute = false;
                    p.StartInfo = info;
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    return output;
                    // process output
                }
                catch (Exception ex)
                {
                    //Library.WriteErrorLog(logFilePath, "Failed to trigger ruby script: " + scriptName);
                    //Library.WriteErrorLog(logFileSaveSitePath, ex.ToString());
                    Logger.Warn("Failed to trigger ruby script: " + scriptName);
                    Logger.Warn(ex.ToString());
                    string subject = "Failed to trigger ruby script " + scriptName;
                    string body = ex.ToString();
                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                    return "-1";
                }
            }
        }
    }
}
