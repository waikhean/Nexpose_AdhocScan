using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;
using log4net;

namespace ccsvmadhocscan
{
    class SPUpload
    {
        static string uploadLibrary = ConfigurationManager.AppSettings["uploadLibrary"];
        static string uploadServer = ConfigurationManager.AppSettings["uploadServer"];
        static string uploadFolder = ConfigurationManager.AppSettings["uploadFolder"];
        static string logPath = ConfigurationManager.AppSettings["logPath"];
        static string logFileSize = ConfigurationManager.AppSettings["logFileSize"];
        static string logFileName = ConfigurationManager.AppSettings["logFileName"];
        static string logFilePath = logPath + logFileName;

        static private ILog Llog;

        static public String log4netConfigFile; 
        static public String NexposeAdhocScanLog;
        static public String saveSiteLog;


        public static string newFolder(String dstCountryName, String dstFolderName, int thread)
        {
            

            string library = ConfigurationManager.AppSettings["uploadLibrary"];
            string server = ConfigurationManager.AppSettings["uploadServer"];
            string uploadFolder = ConfigurationManager.AppSettings["uploadFolder"];
            ListsSoap.ListsSoapClient listsClient = new ListsSoap.ListsSoapClient();
            string dstLink = "https://" + server + "/" + dstCountryName + "/_vti_bin/Lists.asmx";

            EndpointAddress dstEndpoint = new EndpointAddress(dstLink);
            listsClient.Endpoint.Address = dstEndpoint;
            string xmlCommand;
            XmlDocument doc = new XmlDocument();
            string dstFolder = uploadFolder + '/' + dstFolderName;
            // Write to log
            //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": " + dstFolder);
            Llog.Info("Thread " + thread + ": " + dstFolder);
            

            xmlCommand = "<Method ID='1' Cmd='New'><Field Name='FSObjType'>1</Field><Field Name='BaseName'>" + dstFolder + "</Field><Field Name='ID'>New</Field></Method>";
            XmlElement ele = doc.CreateElement("Batch");
            ele.SetAttribute("OnError", "Continue");
            ele.InnerXml = xmlCommand;
            if (listsClient.ClientCredentials != null)
            {
                listsClient.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
            }
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
(se, cert, chain, sslerror) =>
{
    return true;
};
            try
            {
                listsClient.Open();
                XElement elex = XElement.Parse(ele.OuterXml);
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": " + elex.ToString());
                XElement node1 = listsClient.UpdateListItems(library, elex);
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": " + node1.ToString());
                return node1.ToString();
            }
            catch (Exception ex)
            {
                Llog.Error("Thread " + thread + ": create sharepoint folder failed: " + ex.ToString());
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": create sharepoint folder failed: " + ex.ToString());
                return "-1";
            }
            finally
            {
                if (listsClient.State == CommunicationState.Faulted)
                {
                    listsClient.Abort();
                }

                if (listsClient.State != CommunicationState.Closed)
                {
                    listsClient.Close();
                }
            }

        }

        public static string uploadReport(string country, string folder, string file, int thread)
        {
            NexposeAdhocScanLog = ConfigurationManager.AppSettings["NexposeAdhocScanErrorLog"];
            Llog = log4net.LogManager.GetLogger(NexposeAdhocScanLog);
            log4netConfigFile = ConfigurationManager.AppSettings["Log4netConfig"];
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4netConfigFile));
            saveSiteLog = ConfigurationManager.AppSettings["saveSiteErrorLog"];

            try
            {

                string countryName = country;
                string folderName = folder;
                string fileName = file;
                //Process p = new Process();
                //p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
               // p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                //p.StartInfo.CreateNoWindow = true;
                //p.StartInfo.UseShellExecute = false;
                //p.StartInfo.FileName = ("iexplore.exe");
                //p.StartInfo.Arguments = ("https://sp.ibm.local/");
                //p.StartInfo.Arguments = ("https://spsens-bbs.bayer-ag.com/sites/000010/");
                //System.Diagnostics.Process.Start("https://spsens-bbs.bayer-ag.com/sites/000010/");
                //p.Start();
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + " " + p.Id);            
                System.Threading.Thread.Sleep(10000);
                //ShowWindow(p.MainWindowHandle, 0);
                //p.CloseMainWindow();
                //try
                //{
                //    foreach (Process ieprocess in Process.GetProcessesByName("IEXPLORE"))
                //    {
                //        ieprocess.Kill();
                //        //Library.WriteErrorLog(logFilePath, "ie running");
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.ToString());
                //}
                //create upload subFolder

                string[] folders = folder.Split('/');
                Llog.Info("Thread " + thread + ": Try creating folder: " + folders[0]);
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Try creating folder: " + folders[0]);

                string newUploadFolder = newFolder(countryName, folders[0], thread);
                /*
                 * No condition checking when message returned from SharePoint
                 * Add condition
                 * "0x00000000" error code is the folder that had been updated. There's no error found, just return this code which designed by SharePoint.
                 * "0x8107090d" error code is the folder that had been created.
                 * Edited by Qi Wei 31-Oct-2016 
                */
                if (newUploadFolder.Contains("0x8107090d"))
                {
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The " +folderName + "has been created.");
                    Llog.Info("Thread " + thread + ": The " + folderName + "has been created.");
                }
                //   Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Try creating folder result: " + newUploadFolder);

                string newFolderResult = newFolder(countryName, folderName, thread);
                if (newFolderResult.Contains("0x8107026f"))
                {
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Failed to create required folder in Sharepoint: " + folderName);
                    Llog.Warn("Thread " + thread + ": Failed to create required folder in Sharepoint: " + folderName);

                    return "Failed";
                }
                else if (newFolderResult.Contains("0x8107090d")) 
                {
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The required folder created in Sharepoint successfully: " + folderName);
                    Llog.Warn("Thread " + thread + ": The required folder created in Sharepoint successfully: " + folderName);
                    string uploadFileResult = uploadFile(countryName, folderName, fileName, thread);
                    return uploadFileResult;
                }
                else if(newFolderResult.Contains("0x00000000")) 
                {
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": The required folder already created in Sharepoint: " + folderName);
                    Llog.Info("Thread " + thread + ": The required folder already created in Sharepoint: " + folderName);

                    string uploadFileResult = uploadFile(countryName, folderName, fileName, thread);
                    return uploadFileResult;
                }
                else
                {
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Failed to create required folder in Sharepoint: " + folderName);
                    Llog.Info("Thread " + thread + ": Failed to create required folder in Sharepoint: " + folderName);
                    return "Failed";
                }
            }
            catch (Exception ex)
            {
                Llog.Error(ex.ToString());
                return "Failed";
            }
        }

        public static string uploadFile(String dstCountryName, String dstFolderName, String srcfileName, int thread)
        {

            NexposeAdhocScanLog = ConfigurationManager.AppSettings["NexposeAdhocScanErrorLog"];
            Llog = log4net.LogManager.GetLogger(NexposeAdhocScanLog);
            log4netConfigFile = ConfigurationManager.AppSettings["Log4netConfig"];
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4netConfigFile));
            saveSiteLog = ConfigurationManager.AppSettings["saveSiteErrorLog"];

            ListsSoap.ListsSoapClient listsClient = new ListsSoap.ListsSoapClient();
            string dstLink = "https://" + uploadServer + "/" + dstCountryName + "/_vti_bin/Copy.asmx";

            string dstFolder = uploadFolder + '/' + dstFolderName;
            EndpointAddress dstEndpoint = new EndpointAddress(dstLink);
            listsClient.Endpoint.Address = dstEndpoint;
            CopySoap.CopySoapClient copyClient = new CopySoap.CopySoapClient();
            if (copyClient.ClientCredentials != null)
            {
                copyClient.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
            }
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
(se, cert, chain, sslerror) =>
{
    return true;
};
            try
            {
                copyClient.Open();
                string srcFile = srcfileName;

                if (!File.Exists(srcFile))
                {
                    throw new ArgumentException(String.Format("{0} does not exist",
                    srcFile), "srcUrl");
                }

                FileStream fStream = File.OpenRead(srcFile);
                string fileFullName = fStream.Name;
                String[] fileNames = fileFullName.Split('\\');
                String fileName = fileNames[fileNames.Length - 1];
                string[] destinationUrl = { "https://" + uploadServer + "/" + dstCountryName + "/" + uploadLibrary + "/" + dstFolder + "/" + fileName };

                byte[] contents = new byte[fStream.Length];
                fStream.Read(contents, 0, (int)fStream.Length);
                fStream.Close();
                // Description Information Field
                CopySoap.FieldInformation descInfo = new CopySoap.FieldInformation
                {
                    DisplayName = "Description",
                    Type = CopySoap.FieldType.Text,
                    Value = "Security Reports"
                };

                CopySoap.FieldInformation[] fileInfoArray = { descInfo };
                CopySoap.CopyResult[] arrayOfResults;
                uint result = copyClient.CopyIntoItems(fileName, destinationUrl, fileInfoArray, contents, out arrayOfResults);
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Upload Result: " + result);
                Llog.Info("Thread " + thread + ": Upload Result: " + result);

                // Check for Errors
                foreach (CopySoap.CopyResult copyResult in arrayOfResults)
                {
                    string msg = "====================================" +
                     "\nUrl: " + copyResult.DestinationUrl +
            "\nError Code: " + copyResult.ErrorCode +
"\nMessage: " + copyResult.ErrorMessage +
"====================================";
                    //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": " + msg);
                }
                return destinationUrl[0];
            }
            catch (Exception ex)
            {
                //Library.WriteErrorLog(logFilePath, "Thread " + thread + ": Upload file failed: " + ex.ToString());
                Llog.Error("Thread " + thread + ": Upload file failed: " + ex.ToString());

                return "-1";
            }
            finally
            {
                if (copyClient.State == CommunicationState.Faulted)
                {
                    copyClient.Abort();
                }

                if (copyClient.State != CommunicationState.Closed)
                {
                    copyClient.Close();
                }

            }
        }
    }
}
