using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Outlook = Microsoft.Office.Interop.Outlook;


//namespace ccsvmadhocscan
//{

//    class SaveAttachment
//    {
//        static string csvFilePath = ConfigurationManager.AppSettings["csvFilePath"];
//        static string emailToProcessFolder = ConfigurationManager.AppSettings["emailToProcessFolder"];
//        static string emailProcessedFolder = ConfigurationManager.AppSettings["emailProcessedFolder"];
//        static string emailFailedFolder = ConfigurationManager.AppSettings["emailFailedFolder"];
//        static string logPath = ConfigurationManager.AppSettings["logPath"];
//        static string logFileName = ConfigurationManager.AppSettings["logFileName"];
//        static string logFilePath = logPath + logFileName;
//        static string emailAddressFrom = ConfigurationManager.AppSettings["emailAddress"];
//        static string adminEmailAddress = ConfigurationManager.AppSettings["adminEmailAddress"];
//        static string outlookProfileName = ConfigurationManager.AppSettings["outlookProfileName"];

//        public static String GetTimestamp(DateTime value)
//        {
//            return value.ToString("yyyyMMddHHmmssffff");
//        }

//        //return new email count
//        // 0 means no new emails
//        // -1 means not able to open Outlook
//        // -2 means unknow error
//        public static int getEmails()
//        {
//            try
//            {
//                Microsoft.Office.Interop.Outlook.Application app = null;
//                Microsoft.Office.Interop.Outlook._NameSpace ns = null;
//                Microsoft.Office.Interop.Outlook.MAPIFolder toProcessFolderM = null;
//                Microsoft.Office.Interop.Outlook.MAPIFolder processedFolderM = null;
//                Microsoft.Office.Interop.Outlook.MAPIFolder failedFolderM = null;
//                int csvFilesCount = 0;
//                bool ownsMutex = false;
//                Mutex mutex = null;
//                string CcsAdhocScanMutex = null;
//                while (!ownsMutex)
//                {
//                    using (mutex = new Mutex(true, CcsAdhocScanMutex, out ownsMutex))
//                    {
//                        if (ownsMutex)
//                        {
//                            try
//                            {
//                                Library.WriteErrorLog(logFilePath, "Start to create new Outlook application");
//                                System.Diagnostics.Process[] processesO = System.Diagnostics.Process.GetProcessesByName("OUTLOOK");
//                                System.Diagnostics.Process[] processesS = System.Diagnostics.Process.GetProcessesByName("SaveOutlookAttachments");
//                                int collCountO = processesO.Length;
//                                int collCountS = processesS.Length;
//                                int sleepCount = 0;
//                                while (true)
//                                {

//                                    if (collCountO ==0 && collCountS == 0)
//                                    {
//                                        break;
//                                    }
//                                    if (collCountS != 0 && sleepCount < 6)
//                                    {
//                                        Library.WriteErrorLog(logFilePath, "Waiting SaveOutlookAttachments process to exit, sleep 10s");
//                                        System.Threading.Thread.Sleep(10000);
//                                        sleepCount++;
//                                        processesS = System.Diagnostics.Process.GetProcessesByName("SaveOutlookAttachments");
//                                        collCountS = processesS.Length;
//                                        processesO = System.Diagnostics.Process.GetProcessesByName("OUTLOOK");
//                                        collCountO = processesO.Length;
//                                        continue;
//                                    }
//                                    //kill Outlook since no program using
//                                    else if(collCountS == 0 && collCountO !=0)
//                                    {
//                                        System.Diagnostics.Process[] processesO2 = System.Diagnostics.Process.GetProcessesByName("OUTLOOK");
//                                        int collCountO2 = processesO2.Length;
//                                        if (collCountO2 > 0)
//                                        {
//                                            try
//                                            {
//                                                foreach (Process process in processesO2)
//                                                {
//                                                    process.Kill();
//                                                    Library.WriteErrorLog(logFilePath, "Closed Outlook process");
//                                                }
//                                            }
//                                            catch (Exception ex)
//                                            {
//                                                Library.WriteErrorLog(logFilePath, "Failed to close Outlook process: " + ex.ToString()); ;
//                                            }
//                                        }
//                                        app = new Microsoft.Office.Interop.Outlook.Application();
//                                        Library.WriteErrorLog(logFilePath, "Created new Outlook application");
//                                        break;
//                                    }
//                                    //kill the process after 60 seconds 
//                                    else if (sleepCount >= 6)
//                                    {
//                                        //processesS = System.Diagnostics.Process.GetProcessesByName("SaveOutlookAttachments");                                        
//                                        processesO = System.Diagnostics.Process.GetProcessesByName("OUTLOOK");                                       
//                                        try
//                                        {
//                                            foreach (Process process in processesS)
//                                            {
//                                                process.Kill();
//                                                Library.WriteErrorLog(logFilePath, "Closed SaveOutlookAttachments process");
//                                            }
//                                            foreach (Process process in processesO)
//                                            {
//                                                process.Kill();
//                                                Library.WriteErrorLog(logFilePath, "Closed Outlook process");
//                                            }
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            Library.WriteErrorLog(logFilePath, "Failed to close Outlook process: " + ex.ToString());
//                                        }
//                                        //System.Threading.Thread.Sleep(1000);
//                                        app = new Microsoft.Office.Interop.Outlook.Application();
//                                        Library.WriteErrorLog(logFilePath, "Created new Outlook application");
//                                        break;
//                                    }
//                                    else
//                                    {
//                                        app = new Microsoft.Office.Interop.Outlook.Application();
//                                        Library.WriteErrorLog(logFilePath, "Created new Outlook application");
//                                        break;
//                                    }
//                                }
//                            }
//                            catch (Exception ex)
//                            {
//                                Library.WriteErrorLog(logFilePath, "Failed to create new Outlook application: " + ex.ToString());
//                                //string subject = "CCSVM AdhocScan: Failed to create new Outlook application";
//                                //string body = "Failed to create new Outlook application " + ex.ToString();
//                                //Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
//                                mutex.ReleaseMutex();
//                                return -1; // error
//                            }
//                            try
//                            {
//                                Library.WriteErrorLog(logFilePath, "Using Outlook to connect to Exchange server");
//                                ns = app.GetNamespace("MAPI");
//                                ns.Logon(outlookProfileName, null, false, false);
//                                Library.WriteErrorLog(logFilePath, "Connected to exchange, check new emails");
//                                toProcessFolderM = ns.Folders[1].Folders[emailToProcessFolder];
//                                processedFolderM = ns.Folders[1].Folders[emailProcessedFolder];
//                                failedFolderM = ns.Folders[1].Folders[emailFailedFolder];
//                                Library.WriteErrorLog(logFilePath, "Total Email to process: " + toProcessFolderM.Items.Count);
//                                int totalEmailsCount = toProcessFolderM.Items.Count;
//                                if (totalEmailsCount > 0)
//                                {
//                                    for (int i = 1; i <= totalEmailsCount; i++)
//                                    {
//                                        Library.WriteErrorLog(logFilePath, "Processing email: " + i + " out of total " + totalEmailsCount);
//                                        Microsoft.Office.Interop.Outlook.MailItem mailItem = toProcessFolderM.Items[1];
//                                        //if (mailItem.UnRead) // I only process the mail if unread
//                                        //{
//                                        try
//                                        {
//                                            if (mailItem.Subject.Contains("CCSVM_Adhoc_Scan"))
//                                            {
//                                                Library.WriteErrorLog(logFilePath, "Start processing email: " + mailItem.Subject);
//                                                Library.WriteErrorLog(logFilePath, "Total attachments : " + mailItem.Attachments.Count);
//                                                int totalAttachmentsCount = mailItem.Attachments.Count;
//                                                if (totalAttachmentsCount > 0)
//                                                {
//                                                    for (int j = 1; j <= totalAttachmentsCount; j++)
//                                                    {
//                                                        string timeStamp = GetTimestamp(DateTime.Now);
//                                                        string fileNameToSave = csvFilePath + mailItem.Attachments[1].FileName + '-' + timeStamp;
//                                                        Library.WriteErrorLog(logFilePath, "Saving attachment: " + mailItem.Attachments[1].FileName + " as: " + fileNameToSave);
//                                                        mailItem.Attachments[1].SaveAsFile(fileNameToSave);
//                                                        csvFilesCount++;
//                                                        mailItem.Move(processedFolderM);
//                                                    }
//                                                }
//                                                else
//                                                {
//                                                    Library.WriteErrorLog(logFilePath, "No email attachment found for this email, move to failed folder");
//                                                    string subject = "CCSVM AdhocScan: Failed to find email attachment";
//                                                    string body = "Failed to find attachment: " + mailItem.Subject;
//                                                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
//                                                    mailItem.Move(failedFolderM);
//                                                }
//                                            }
//                                            else
//                                            {
//                                                Library.WriteErrorLog(logFilePath, "The email subject doesn't contain the required subject, move to failed folder");
//                                                string subject = "CCSVM AdhocScan: The email subject doesn't contain the required subject";
//                                                string body = "Failed to find the required subject: " + mailItem.Subject;
//                                                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
//                                                mailItem.Move(failedFolderM);
//                                            }
//                                        }
//                                        catch (Exception ex)
//                                        {
//                                            Library.WriteErrorLog(logFilePath, "Failed to save attahcment: " + ex.ToString());
//                                            string subject = "CCSVM AdhocScan: Failed to save attachment";
//                                            string body = "Failed to save attachment: " + ex.ToString();
//                                            Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
//                                            continue;
//                                        }
//                                    }
//                                }
//                                else
//                                {
//                                    Library.WriteErrorLog(logFilePath, "No email found during this run");
//                                }
//                            }
//                            catch (Exception ex)
//                            {
//                                Library.WriteErrorLog(logFilePath, "Failed to get new emails " + ex.ToString());
//                                string subject = "CCSVM AdhocScan: Failed to get new emails";
//                                string body = "Failed to get new emails" + ex.ToString();
//                                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
//                                mutex.ReleaseMutex();
//                                return -2;
//                            }

//                            try
//                            {
//                                app.Quit();
//                            }
//                            catch (Exception ex)
//                            {
//                                Library.WriteErrorLog(logFilePath, "Failed to close Outlook application: " + ex.ToString());
//                                string subject = "CCSVM AdhocScan: Failed to close Outlook application";
//                                string body = "Failed to close Outlook application " + ex.ToString();
//                                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
//                            }
//                            ns = null;
//                            app = null;
//                            toProcessFolderM = null;
//                            processedFolderM = null;
//                            Library.WriteErrorLog(logFilePath, "CCS Save Attachments Finished");                            
//                            //close Outlook after using
//                            System.Diagnostics.Process[] processesO1 = System.Diagnostics.Process.GetProcessesByName("OUTLOOK");
//                            int collCountO1 = processesO1.Length;
//                            if (collCountO1 > 0)
//                            {
//                                try
//                                {
//                                    foreach (Process process in processesO1)
//                                    {
//                                        process.Kill();
//                                        Library.WriteErrorLog(logFilePath, "Closed Outlook process");
//                                    }
//                                }
//                                catch (Exception ex)
//                                {
//                                    Library.WriteErrorLog(logFilePath, "Failed to close Outlook process: " + ex.ToString()); ;
//                                }
//                            }       
//                            mutex.ReleaseMutex();
//                            return csvFilesCount;
//                        }
//                        else
//                        {
//                            Thread.Sleep(5000);
//                        }
//                    }
//                }
//                //failed during mutex
//                return -1;
//            }
//            catch (Exception ex)
//            {
//                Library.WriteErrorLog(logFilePath, "Failed to during checking new emails: " + ex.ToString());
//                string subject = "CCSVM AdhocScan: Failed to during checking new emails";
//                string body = ex.ToString();
//                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
//                return -2;
//            }
//        }
//    }
//}
