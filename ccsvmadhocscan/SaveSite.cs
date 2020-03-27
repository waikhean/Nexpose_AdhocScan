using CNAVCryptoLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ccsvmadhocscan
{
    class SaveSite
    {
        static string programPath = ConfigurationManager.AppSettings["programPath"];
        static string logPath = ConfigurationManager.AppSettings["logPath"];
        static string logFileSize = ConfigurationManager.AppSettings["logFileSize"];
        static string emailServer = ConfigurationManager.AppSettings["smtpServer"];
        static string emailPort = ConfigurationManager.AppSettings["smtpServerPort"];
        static string logFileName = ConfigurationManager.AppSettings["logFileName"];
        static string logFileSaveSite = ConfigurationManager.AppSettings["logFileSaveSite"];
        static string logFileSaveSitePath = logPath + logFileSaveSite;
        static string rubyPath = ConfigurationManager.AppSettings["rubyPath"];
        static string emailAddressFrom = ConfigurationManager.AppSettings["emailAddress"];
        static string adminEmailAddress = ConfigurationManager.AppSettings["adminEmailAddress"];
        static string getSiteIncluded = rubyPath + @"scripts\get_site_included_range.rb";
        static string getSiteExcluded = rubyPath + @"scripts\get_site_excluded_range.rb";
        static string includedFilePath = ConfigurationManager.AppSettings["csvFileinclude"];
        static string excludedFilePath = ConfigurationManager.AppSettings["csvFileExclude"];
        static string ccsVMServerIP = ConfigurationManager.AppSettings["ccsVMServerIP"];
        static string ccsVMServerPort = ConfigurationManager.AppSettings["ccsVMServerPort"];
        static string ccsVMServerUser = ConfigurationManager.AppSettings["ccsVMServerUser"];
        static string passworFile = ConfigurationManager.AppSettings["ccsVMServerPassword"];
        static string password = "xyz123!@#";


        static private ILog Llog;//= log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static public String log4netConfigFile; // = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\log4net.config";
        static public String saveSiteLog;

        public static int getSite()
        {

            saveSiteLog = ConfigurationManager.AppSettings["saveSiteErrorLog"];
            Llog = log4net.LogManager.GetLogger(saveSiteLog);
            log4netConfigFile = ConfigurationManager.AppSettings["Log4netConfig"];
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(log4netConfigFile));


            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            passwordBytes = SHA256.Create().ComputeHash(passwordBytes);
            byte[] bytesAlreadyEncrypted = null;
            try
            {
                bytesAlreadyEncrypted = File.ReadAllBytes(passworFile);
            }
            catch (Exception ex)
            {
                //Library.WriteErrorLog(logFileSaveSitePath, "failed to read the password file");
                //Library.WriteErrorLog(logFileSaveSitePath, ex.ToString());

                Llog.Info("failed to read the password file");
                Llog.Info(ex.ToString());

                string subject = "CCSVM SaveSites: Failed to read the password file";
                string body = ex.ToString();
                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                return -1;
            }
            byte[] bytesDecrypted = new CryptoLibrary().AES_Decrypt(bytesAlreadyEncrypted, passwordBytes);
            string ccsVMServerPassword = Encoding.UTF8.GetString(bytesDecrypted);
            //Library.WriteErrorLog(logFileSaveSitePath, "userPassword:" + userPassword);
            DateTime today = DateTime.Now;

            //Library.WriteErrorLog(logFileSaveSitePath, "Included IP List file path: " + includedFilePath);
            //Library.WriteErrorLog(logFileSaveSitePath, "Excluded IP List file path: " + excludedFilePath);

            Llog.Info("Included IP List file path: " + includedFilePath);
            Llog.Info("Excluded IP List file path: " + excludedFilePath);



            string month = "";
            if (today.Month <10)
            {
                month = "0" + today.Month;
            }
            string day = "";
            if (today.Day < 10)
            {
                day = "0" + today.Day;
            }
            string includedFilePathNew = Path.GetDirectoryName(includedFilePath) + "\\" + Path.GetFileNameWithoutExtension(includedFilePath) + "-" + today.Year + month + day + Path.GetExtension(includedFilePath);
            string excludedFilePathNew = Path.GetDirectoryName(excludedFilePath) + "\\" + Path.GetFileNameWithoutExtension(excludedFilePath) + "-" + today.Year + month + day + Path.GetExtension(excludedFilePath);
            string includedFilePathTmp = includedFilePath + "-old";
            string excludedFilePathTmp = excludedFilePath + "-old";
            bool getIncluded = false;
            bool getExcluded = false;
            bool processIncluded = false;
            bool processExcluded = false;
            bool otherError = false;  //error code -2
            //Library.WriteErrorLog(logFileSaveSitePath, "New Included IP List file path: " + includedFilePathNew);
            //Library.WriteErrorLog(logFileSaveSitePath, "Calling included ruby script: " + getSiteIncluded + " " + includedFilePathNew);

            Llog.Info("New Included IP List file path: " + includedFilePathNew);
            Llog.Info("Calling included ruby script: " + getSiteIncluded + " " + includedFilePathNew);

            string resultIncluded = CallRuby.runSiteScript(getSiteIncluded, ccsVMServerIP, ccsVMServerPort, ccsVMServerUser, ccsVMServerPassword, includedFilePathNew);

            Llog.Info("Calling included ruby script result: " + resultIncluded);

            //Library.WriteErrorLog(logFileSaveSitePath, "Calling included ruby script result: " + resultIncluded);

            if (resultIncluded.Contains("getsuccess"))
            {
                getIncluded = true;
                if (File.Exists(includedFilePathNew))
                {
                    //Library.WriteErrorLog(logFileSaveSitePath, "Calling ruby script successfully");
                    //Library.WriteErrorLog(logFileSaveSitePath, "Starting to replace the included csv file");

                    Llog.Info("Calling ruby script successfully");
                    Llog.Info("Starting to replace the included csv file");

                    FileStream streamIncluded = null;
                    while (true)
                    {
                        //remove old file
                        if (File.Exists(includedFilePathTmp))
                        {
                            //Library.WriteErrorLog(logFileSaveSitePath, "Clean up old file: " + includedFilePathTmp);
                            Llog.Info("Clean up old file: " + includedFilePathTmp);

                            try
                            {
                                File.Delete(includedFilePathTmp);
                            }
                            catch (Exception ex)
                            {
                                //Library.WriteErrorLog(logFileSaveSitePath, "Failed to remove old file: " + includedFilePathTmp);
                                //Library.WriteErrorLog(logFileSaveSitePath, ex.ToString());

                                Llog.Error("Failed to remove old file: " + includedFilePathTmp);
                                Llog.Error(ex.ToString());

                                string subject = "CCSVM SaveSites: Failed to remove old file";
                                string body = includedFilePathTmp + ": " + ex.ToString();
                                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                                otherError = true;
                                return -2;
                            }
                        }
                        //replace the file
                        if (File.Exists(includedFilePath))
                        {
                            try
                            {
                                streamIncluded = File.Open(includedFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                                streamIncluded.Close();
                                streamIncluded = null;
                                try
                                {
                                    File.Move(includedFilePath, includedFilePathTmp);
                                    //Library.WriteErrorLog(logFileSaveSitePath, "File successfully backup as: " + includedFilePathTmp);
                                    Llog.Info("File successfully backup as: " + includedFilePathTmp);

                                }
                                catch (Exception ex)
                                {
                                    //Library.WriteErrorLog(logFileSaveSitePath, "Failed to backup : " + includedFilePath);
                                    //Library.WriteErrorLog(logFileSaveSitePath, ex.ToString());

                                    Llog.Error("Failed to backup : " + includedFilePath);
                                    Llog.Error(ex.ToString());

                                    string subject = "CCSVM SaveSites: Failed to backup included csv file";
                                    string body = includedFilePath + ": " + ex.ToString();
                                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                                    otherError = true;
                                    return -2;
                                }
                            }
                            catch (Exception)
                            {
                                //Library.WriteErrorLog(logFileSaveSitePath, "File in use, sleep for 5 seconds: " + includedFilePath);
                                Llog.Error("File in use, sleep for 5 seconds: " + includedFilePath);
                                System.Threading.Thread.Sleep(5000);
                                continue;
                            }
                            //replace with the new file
                            try
                            {
                                File.Move(includedFilePathNew, includedFilePath);
                            }
                            catch (Exception ex)
                            {
                                //Library.WriteErrorLog(logFileSaveSitePath, "Failed to move the new file: " + includedFilePathNew);
                                Llog.Error("Failed to move the new file: " + includedFilePathNew);
                                Llog.Error(ex.ToString());
                                //Library.WriteErrorLog(logFileSaveSitePath, ex.ToString());
                                string subject = "CCSVM SaveSites: Failed to move the new file";
                                string body = includedFilePathNew + ": " + ex.ToString();
                                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                                otherError = true;
                                return -2;
                            }
                            processIncluded = true;
                            break;
                        }
                        else
                        {
                            //Library.WriteErrorLog(logFileSaveSitePath, "File not exist: " + includedFilePath);
                            Llog.Info("File not exist: " + includedFilePath);

                            try
                            {
                                File.Move(includedFilePathNew, includedFilePath);
                            }
                            catch (Exception ex)
                            {
                                //Library.WriteErrorLog(logFileSaveSitePath, "Failed to move the new file: " + includedFilePathNew);
                                //Library.WriteErrorLog(logFileSaveSitePath, ex.ToString());
                                Llog.Error("Failed to move the new file: " + includedFilePathNew);
                                Llog.Error(ex.ToString());
                                string subject = "CCSVM SaveSites: Failed to move the new file";
                                string body = includedFilePathNew + ": " + ex.ToString();
                                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                                return -2;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    Llog.Error("Calling included ruby script failed with unknown reason, check logs");
                    //Library.WriteErrorLog(logFileSaveSitePath, "Calling included ruby script failed with unknown reason, check logs");

                    //Library.WriteErrorLog(logFileSaveSitePath, resultIncluded);
                }
            }
            else
            {
                Llog.Error("Calling included ruby script failed with unknown reason, check logs");
                //Library.WriteErrorLog(logFileSaveSitePath, "Calling included ruby script failed with unknown reason, check logs");

                //Library.WriteErrorLog(logFileSaveSitePath, resultIncluded);
            }

            //processing for excluded ip list
            //Library.WriteErrorLog(logFileSaveSitePath, "New Excluded IP List file path: " + excludedFilePathNew);
            //Library.WriteErrorLog(logFileSaveSitePath, "Calling excluded ruby script: " + getSiteExcluded + " " + excludedFilePathNew);
            Llog.Info("New Excluded IP List file path: " + excludedFilePathNew);
            Llog.Info("Calling excluded ruby script: " + getSiteExcluded + " " + excludedFilePathNew);

            string resultExcluded = CallRuby.runSiteScript(getSiteExcluded, ccsVMServerIP, ccsVMServerPort, ccsVMServerUser, ccsVMServerPassword, excludedFilePathNew);
            //Library.WriteErrorLog(logFileSaveSitePath, "Calling excluded ruby script result: " + resultExcluded);
            Llog.Info("Calling excluded ruby script result: " + resultExcluded);
            if (resultExcluded.Contains("getsuccess"))
            {
                getExcluded = true;
                if (File.Exists(excludedFilePathNew))
                {
                    //Library.WriteErrorLog(logFileSaveSitePath, "Calling excluded ruby script successfully");
                    //Library.WriteErrorLog(logFileSaveSitePath, "Starting to replace the excluded csv file");
                    Llog.Info("Calling excluded ruby script successfully");
                    Llog.Info("Starting to replace the excluded csv file");

                    FileStream streamExcluded = null;
                    while (true)
                    {
                        //remove old file
                        if (File.Exists(excludedFilePathTmp))
                        {
                            //Library.WriteErrorLog(logFileSaveSitePath, "Clean up old file: " + excludedFilePathTmp);
                            Llog.Info("Clean up old file: " + excludedFilePathTmp);

                            try
                            {
                                File.Delete(excludedFilePathTmp);
                            }
                            catch (Exception ex)
                            {
                                //Library.WriteErrorLog(logFileSaveSitePath, "Failed to remove old file: " + excludedFilePathTmp);
                                //Library.WriteErrorLog(logFileSaveSitePath, ex.ToString());
                                Llog.Error("Failed to remove old file: " + excludedFilePathTmp);
                                Llog.Error(ex.ToString());

                                string subject = "CCSVM SaveSites: Failed to remove old file";
                                string body = excludedFilePathTmp + ": " + ex.ToString();
                                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                                otherError = true;
                                return -2;
                            }
                        }
                        //replace the file
                        if (File.Exists(excludedFilePath))
                        {
                            try
                            {
                                streamExcluded = File.Open(excludedFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
                                streamExcluded.Close();
                                streamExcluded = null;
                                try
                                {
                                    File.Move(excludedFilePath, excludedFilePathTmp);
                                    Llog.Info("File successfully backup as: " + excludedFilePathTmp);
                                    //Library.WriteErrorLog(logFileSaveSitePath, "File successfully backup as: " + excludedFilePathTmp);

                                }
                                catch (Exception ex)
                                {
                                    //Library.WriteErrorLog(logFileSaveSitePath, "Failed to backup : " + excludedFilePath);
                                    //Library.WriteErrorLog(logFileSaveSitePath, ex.ToString());

                                    Llog.Error("Failed to backup : " + excludedFilePath);
                                    Llog.Error(ex.ToString());

                                    string subject = "CCSVM SaveSites: Failed to backup file";
                                    string body = excludedFilePath + ": " + ex.ToString();
                                    Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                                    otherError = true;
                                    return -2;
                                }
                            }
                            catch (Exception)
                            {
                                //Library.WriteErrorLog(logFileSaveSitePath, "File in use, sleep for 5 seconds: " + excludedFilePath);
                                Llog.Info("File in use, sleep for 5 seconds: " + excludedFilePath);

                                System.Threading.Thread.Sleep(5000);
                                continue;
                            }

                            //replace with the new file
                            try
                            {
                                File.Move(excludedFilePathNew, excludedFilePath);
                            }
                            catch (Exception ex)
                            {
                                //Library.WriteErrorLog(logFileSaveSitePath, "Failed to move the new file: " + excludedFilePathNew);
                                //Library.WriteErrorLog(logFileSaveSitePath, ex.ToString());
                                Llog.Error("Failed to move the new file: " + excludedFilePathNew);
                                Llog.Error(ex.ToString());

                                string subject = "CCSVM SaveSites: Failed to move the new file";
                                string body = excludedFilePathNew + ": " + ex.ToString();
                                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                                otherError = true;
                                return -2;
                            }
                            processExcluded = true;
                            break;
                        }
                        else
                        {
                            //Library.WriteErrorLog(logFileSaveSitePath, "File not exist: " + excludedFilePath);
                            Llog.Error("File not exist: " + excludedFilePath);


                            try
                            {
                                File.Move(excludedFilePathNew, excludedFilePath);
                            }
                            catch (Exception ex)
                            {
                                //Library.WriteErrorLog(logFileSaveSitePath, "Failed to move the new file: " + excludedFilePathNew);
                                //Library.WriteErrorLog(logFileSaveSitePath, ex.ToString());
                                Llog.Error("Failed to move the new file: " + excludedFilePathNew);
                                Llog.Error(ex.ToString());
                                string subject = "CCSVM SaveSites: Failed to move the new file";
                                string body = excludedFilePathNew + ": " + ex.ToString();
                                Library.sendEmail(emailAddressFrom, adminEmailAddress, subject, body);
                                return -2;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    //Library.WriteErrorLog(logFileSaveSitePath, "Calling excluded ruby script failed with unknown reason, check logs");
                    Llog.Error("Calling excluded ruby script failed with unknown reason, check logs");

                }
            }
            else
            {
                //Library.WriteErrorLog(logFileSaveSitePath, "Calling excluded ruby script failed with unknown reason, check logs");
                Llog.Error("Calling excluded ruby script failed with unknown reason, check logs");

            }

            //Library.WriteErrorLog(logFileSaveSitePath, "Finish running");
            Llog.Info("Finish running");

            if (otherError)
            {
                return -2;
            }
            else
            {
                if (getIncluded & getExcluded)
                {
                    if (processIncluded & processExcluded)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    return -1;
                }
            }
        }
    }
}
