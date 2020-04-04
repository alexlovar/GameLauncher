using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
//using Ionic.Zip;

namespace rlgn
{
    class Updater
    {
        private const string UpdateServer = "http://update1.eqgonline.com";
        private string[] StrictFiles; // important files
        private bool[] StrictFilesDownloadInfo;
        private int baseVersionS; // just a random version number to check against
        private int baseVersionL;
        private SHA1CryptoServiceProvider SHAProvider;
        private WebClient updateServerLoader;
        private WebClient downloadClient;
        private FileStream filestream;
        private StreamReader ufile;
        private string[] ffile;
        private string SHAChecksumL;
        private string SHAChecksumS;
        private int filesToUpdate;
        private int updatedFiles;
        private string baseDir;
        private string tmpDir;
        private int hoursDiff;
        private bool hasChecked;
        private Label lbUpdater;
        private string errors;
        private Func<int, int> uf;
        private ProgressBar pb;
        private ProgressBar pbT;
        private bool haserror;
        public bool stopMusic;

        public Updater(ref ProgressBar Progress, ref ProgressBar pTotal, Func<int, int> UpdateFinished)
        {
            baseDir = System.AppDomain.CurrentDomain.BaseDirectory;
            tmpDir = baseDir + "tmp\\";
            SHAProvider = new SHA1CryptoServiceProvider();
            StrictFiles = new string[] {    "RF_Online.bin" ,
                                            "KudoZ.exe"
                                        }; // These are the files which will be SHA1-checked against the server.
            StrictFilesDownloadInfo = new bool[StrictFiles.Length];
            updateServerLoader = new WebClient();
            hoursDiff = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours; //we'll need that for server choosing
            hasChecked = false;
            uf = UpdateFinished;
            pb = Progress;
            pbT = pTotal;
            haserror = false;
            stopMusic = false;
        }

        public bool hasError()
        {
            return haserror;
        }

        /// <summary>
        ///  Checks if all local files are up to date - StrictFiles against SHA1, others against baseVersion
        ///  returns 0 on "all files up to date", n if n files need updating, -1 on error.
        /// </summary>
        /// 
        public int Check()
        {
            filesToUpdate = 0;
            try
            {
                baseVersionS = Convert.ToInt32(updateServerLoader.DownloadString(UpdateServer + "/updt/baseVersion.php"));
            }
            catch (Exception e)
            {
                errors += "\nCould not retrieve base version from server: " + e.Message;
                haserror = true;
                return -1;
            }
            baseVersionL = Properties.Settings.Default.Version;
            try
            {
                if (Directory.Exists(tmpDir))
                {
                    Directory.Delete(tmpDir, true);
                }

                Directory.CreateDirectory(tmpDir);
            }
            catch (Exception e)
            {
                errors += "\nCould not create temporary directory: " + e.Message;
                return -1;
            }
            // Checking and updating "strict" files
            for (int i = 0; i < StrictFiles.Length; i++)
            {
                if (File.Exists(baseDir + StrictFiles[i]))
                {
                    filestream = new FileStream(baseDir + StrictFiles[i], FileMode.Open);
                    SHAChecksumL = BitConverter.ToString(SHAProvider.ComputeHash(filestream)).ToLower().Replace("-", "");
                    filestream.Close();
                    filestream.Dispose();
                    try
                    {
                        SHAChecksumS = updateServerLoader.DownloadString(UpdateServer + "/updt/file.php?f=" + StrictFiles[i]).ToLower();
                    }
                    catch (Exception ex)
                    {
                        errors += "\nCould not retrieve SHA-1 sums from server: " + ex.Message;
                        return -1;
                    }
                    if (SHAChecksumL != SHAChecksumS)
                    {
                        StrictFilesDownloadInfo[i] = true;
                    }
                    else
                    {
                        StrictFilesDownloadInfo[i] = false;
                    }
                }
                else
                {
                    errors += "\n File does not exist: " + StrictFiles[i];
                    return -1;
                }
            }

            string ufiles = "";
            // Checking Version numbers
            if (baseVersionL != baseVersionS)
            {
                // retrieve changed file list
                int cfiles = Convert.ToInt32(updateServerLoader.DownloadString(UpdateServer + "/updt/vcmp.php?lver=" + baseVersionL.ToString()));
                filesToUpdate += cfiles;
                
                try
                {
                    ufiles = updateServerLoader.DownloadString(UpdateServer + "/updt/flfversion.php?lver=" + baseVersionL.ToString() + "&tz=" + hoursDiff.ToString());
                    if (ufiles.Contains("Launcher_01.mp3"))
                    {
                        stopMusic = true;
                    }
                }
                catch (Exception ex)
                {
                    errors += "\n" + ex.Message;
                    return -1;
                }
            }
            using (StreamWriter oftxt = new StreamWriter(baseDir + "ufiles.txt", false))
            {
                oftxt.Write(ufiles);
                for (int i = 0; i < StrictFiles.Length; i++)
                {
                    if (StrictFilesDownloadInfo[i])
                    {
                        string str = updateServerLoader.DownloadString(UpdateServer + "/updt/strict.php?f=" + StrictFiles[i]);
                        string url = str.Substring(0, str.IndexOf(StrictFiles[i]));
                        string sha = str.Substring(str.Length - 40, 40) + "\n";
                        oftxt.Write(url.Substring(0, url.Length - 1) + "#" + StrictFiles[i] + "#" + sha);
                        filesToUpdate++;
                    }
                }
            }

            hasChecked = true;

            return filesToUpdate;
        }

        /// <summary>
        ///  updates all files that were previously discovered
        ///  returns number of updated files, -1 on error.
        /// </summary>
        public int Update()
        {
            if (!hasChecked || !File.Exists(baseDir + "ufiles.txt"))
            {
                return -1;
            }
            updatedFiles = 0;
            hasChecked = false;
            
            try
            {
                ufile = new StreamReader(baseDir + "ufiles.txt");
            }
            catch (Exception e)
            {
                errors += "\nMissing file: ufiles.txt " + e.Message;
                return -1;
            }

            pb.Minimum = 0;
            pb.Maximum = 100;
            pb.Value = 0;
            pbT.Minimum = 0;
            pbT.Maximum = (double)filesToUpdate; // makes it easy to indicate progress, just ++ the value
            pbT.Value = 0;
            

            downloadClient = new WebClient();
            downloadClient.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadClient_DownloadFileCompleted);
            downloadClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadClient_DownloadProgressChanged);

            ffile = ufile.ReadLine().Split(new char[] { '#' }, 3, StringSplitOptions.RemoveEmptyEntries);
            if (!Directory.Exists(Path.GetDirectoryName(tmpDir + ffile[1].Replace('/', '\\'))))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(tmpDir + ffile[1].Replace('/', '\\')));
            }
            downloadClient.DownloadFileAsync(new Uri(ffile[0] + "/" + ffile[1]), tmpDir + ffile[1].Replace('/', '\\'));

            return 0; // never used - could just as well be void... but maybe we'll need it in the future
        }

        void downloadClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            pb.Value = (double)e.ProgressPercentage;
            this.lbUpdater.Content = "Downloading file " + (updatedFiles + 1).ToString() + " of " + filesToUpdate.ToString() + ", " + (e.BytesReceived / 1024).ToString() + " kB of " + (e.TotalBytesToReceive / 1024).ToString() + " kB completed.";
        }

        void downloadClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            pb.Value = 0;
            pbT.Value += 1;

           /* if (ffile[1].EndsWith("zip"))
            {
                using (ZipFile zipped = ZipFile.Read(tmpDir + ffile[1]))
                {
                    if (zipped.Count > 1)
                    {
                        MessageBox.Show("\nZip file invalid: " + ffile[1] + ". More than one file included.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        foreach (ZipEntry ze in zipped)
                        {
                            ze.Extract(ExtractExistingFileAction.OverwriteSilently);
                            File.Delete(tmpDir + ffile[1]);
                            ffile[1] = ze.FileName;
                        }
                    }
                }
            } */

            if (checkFile(tmpDir + ffile[1], ffile[2]))
            {
                // file is good. We copy it.
                if (ffile[1].Contains('/'))
                {
                    DirectoryInfo dir = new DirectoryInfo(baseDir + ffile[1].Replace('/', '\\').Substring(0, ffile[1].LastIndexOf('/')));
                    if (!dir.Exists)
                    {
                        dir.Create();
                    }
                }
                File.Copy(tmpDir + ffile[1].Replace('/', '\\'), baseDir + ffile[1].Replace('/', '\\'), true);
                updatedFiles++;
                // next file
                if (ufile.Peek() >= 0)
                {
                    ffile = ufile.ReadLine().Split(new char[] { '#' }, 3, StringSplitOptions.RemoveEmptyEntries);
                    if (!Directory.Exists(Path.GetDirectoryName(tmpDir + ffile[1].Replace('/', '\\'))))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(tmpDir + ffile[1].Replace('/', '\\')));
                    }
                    downloadClient.DownloadFileAsync(new Uri(ffile[0] + "/" + ffile[1]), tmpDir + ffile[1].Replace('/', '\\'));
                }

                // no more next file
                else
                {
                    // we are done with updating.
                    Directory.Delete(tmpDir, true);
                    ufile.Close();
                    ufile.Dispose();
                    Properties.Settings.Default.Version = baseVersionS;
                    Properties.Settings.Default.Save();
                    downloadClient.Dispose();
                    updateServerLoader.Dispose();
                    uf(updatedFiles - filesToUpdate);
                }
            }
            else
            {
                // file did not download correctly. We download it again.
                File.Delete(tmpDir + ffile[1].Replace('/', '\\'));
                downloadClient.DownloadFileAsync(new Uri(ffile[0] + "/" + ffile[1]), tmpDir + ffile[1].Replace('/', '\\'));
            }
        }

        private bool checkFile(string path, string sha1sum)
        {
            bool ret = false;
            path = path.Replace('/', '\\');
            FileStream fs = new FileStream(path, FileMode.Open);
            string sha1calc = BitConverter.ToString(SHAProvider.ComputeHash(fs)).ToLower().Replace("-", "");
            fs.Close();
            fs.Dispose();
            if (sha1calc.ToLower() == sha1sum.ToLower())
            {
                ret = true;
            }

            return ret;
        }

        public string lastError()
        {
            string ret = errors;
            errors = "";
            return ret;
        
        }

        public void CheckRepair()
        {
            filesToUpdate = 0;
            stopMusic = true;
            try
            {
                if (Directory.Exists(tmpDir))
                {
                    Directory.Delete(tmpDir, true);
                }

                Directory.CreateDirectory(tmpDir);
            }
            catch (Exception e)
            {
                errors += "\nCould not create temporary directory: " + e.Message;
                return;
            }


            // Retrieve file list from server

            string ufiles;
            lbUpdater.Content = "Repairing files...";

            try
            {
                ufiles = updateServerLoader.DownloadString(UpdateServer + "/updt/filelist.php");
                using (StreamWriter offile = new StreamWriter(baseDir + "filelist.txt"))
                {
                    offile.Write(ufiles);
                }

                StreamReader filelist = new StreamReader(baseDir + "filelist.txt");
                string[] file = new string[3];
                string list;
                StreamWriter oftxt = new StreamWriter(baseDir + "ufiles.txt");
                while (filelist.Peek() >= 0)
                {
                    list = filelist.ReadLine();
                    file = list.Replace('/', '\\').Split(new char[] { '#' }, 3, StringSplitOptions.None);

                    if (File.Exists(baseDir + file[1]))
                    {
                        if (!checkFile(baseDir + file[1], file[2]))
                        {
                            oftxt.WriteLine(list);
                            filesToUpdate++;
                        }
                    }
                    else
                    {
                        oftxt.WriteLine(list);
                        filesToUpdate++;
                    }
                }
                oftxt.Close();
                oftxt.Dispose();
                filelist.Close();
                filelist.Dispose();
            }
            catch (Exception ex)
            {
                errors += "\n" + ex.Message;
                return;
            }
            hasChecked = true;
            File.Delete("filelist.txt");

            if (filesToUpdate > 0)
            {
                Update();
            }
            else
            {
                lbUpdater.Content = "File check completed. All files OK, ready for Login.";
                this.pb.Visibility = Visibility.Hidden;
                Directory.Delete(tmpDir, true);
                uf(-42);
            }
        }
    }
}
