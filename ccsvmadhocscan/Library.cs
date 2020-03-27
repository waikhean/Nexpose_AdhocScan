using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using log4net;


namespace ccsvmadhocscan
{
    public static class Library
    {
        static string logFileSize = ConfigurationManager.AppSettings["logFileSize"];
        static string emailServer = ConfigurationManager.AppSettings["smtpServer"];
        static string emailPort = ConfigurationManager.AppSettings["smtpServerPort"];
        static string logPath = ConfigurationManager.AppSettings["logPath"];
        static string logFileName = ConfigurationManager.AppSettings["logFileName"];
        static string logFilePath = logPath + logFileName;
        static string logFileSaveSite = ConfigurationManager.AppSettings["logFileSaveSite"];
        static string logFileSaveSitePath = logPath + logFileSaveSite;
        static string emailAddressFrom = ConfigurationManager.AppSettings["emailAddress"];
        static string adminEmailAddress = ConfigurationManager.AppSettings["adminEmailAddress"];
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        static private ILog Llog;//= log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static public String log4netConfigFile; // = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\log4net.config";
        static public String NexposeAdhocScanLog;

        

        public static void sendEmail(string sender, string receivers, string subject, string body)

        {
            NexposeAdhocScanLog = ConfigurationManager.AppSettings["NexposeAdhocScanErrorLog"];
            Llog = log4net.LogManager.GetLogger(NexposeAdhocScanLog);
            log4netConfigFile = ConfigurationManager.AppSettings["Log4netConfig"];
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4netConfigFile));

            if (!File.Exists(logFileName))
            {
                File.CreateText(logFileName);
            }
            // Set Status to Locked
            //_readWriteLock.EnterWriteLock();
            MailMessage mail = new MailMessage(sender, receivers);
            SmtpClient client = new SmtpClient();
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = true;
            client.Host = emailServer;
            client.Port = Int32.Parse(emailPort);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            try
            {
                client.Send(mail);
            }
            catch (Exception ex)
            {
                Llog.Error("Sending email failed: " + ex.ToString());
                Llog.Error("Failed email subject: " + subject);
                Llog.Error("Failed email body: " + body);
            }

        }



        private static string GetLogFolderName(string projectName)
        {
            string logFolder = @"\Logs\";
            switch (projectName)
            {
                case "ccsvmadhocscan":
                case "ccssmadhocscan":
                    logFolder = @"\Logs\";
                    break;
                default:
                    break;
            }
            Console.WriteLine("debug lib: {0} | {1}", projectName, logFolder);
            return logFolder;
        }

        private static string GetProjectName(string fullname)
        {
            string[] arr = fullname.Split(',');
            return arr[0];
        }
        public static void WriteErrorLog(string logFileName, string Message, string filename = "")
        {
            //StreamWriter sw = null;
            try
            {
                string projectName = GetProjectName(Assembly.GetCallingAssembly().FullName);
                string logFolder = GetLogFolderName(projectName);

                string path = logPath;
                if (filename == "") logFileName = path + projectName + ".log";
                else logFileName = $"{path}Error log for {filename}.log";

                StreamWriter sw = new StreamWriter(logFileName, true);
                sw.WriteLine(DateTime.Now.ToString() + ": " + Message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {
                //WriteErrorLog(logFileName, e.ToString());
                Llog.Error(e.ToString());

            }

        }


    }
}
