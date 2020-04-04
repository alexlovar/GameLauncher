using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Media;
using System.IO;
using System.Management;
using System.Management.Instrumentation;
using System.Globalization;
using System.Diagnostics;

namespace rlgn
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool SetupActive = false;
        private EngineSettings engineSettings;
        private Cursor tmpCursor;
        private bool gotZones = false;
        private Login RFLogin;
        private Brush[] brh;
        private int baseVersionS;
        private bool pdnu;
        private System.Threading.Timer wdt;
        private string IP = "64.31.6.86";


        public MainWindow()
        {
            InitializeComponent();
            tbUser.Text = (Properties.Settings.Default.Username.Length > 1 ? Properties.Settings.Default.Username : "Username");
            if (tbUser.Text != "Username")
            {
                tbUser_GotFocus(tbUser, new RoutedEventArgs());
            }
            this.imLoginHover.Visibility = System.Windows.Visibility.Hidden;
            this.imSetupHover.Visibility = System.Windows.Visibility.Hidden;
            engineSettings = new EngineSettings();
            this.cbWindowMode.IsChecked = !engineSettings.FullScreen;
            pdnu = false;
            //using (WebClient cli = new WebClient())
            //{
            //    /*if (cli.DownloadString("#") != "OK")
            //    {
            //        MessageBox.Show("The server is unavailable. Please check if you are connected to the Internet.", "Server connection error", MessageBoxButton.OK, MessageBoxImage.Error);
            //        Application.Current.Shutdown();
            //    }*/
            //    //string secmsg = cli.DownloadString("#");
            //    //if (secmsg != "6v6v")
            //    //{
            //    //    MessageBox.Show("You seem to be using an outdated version of the launcher, or trying to connect to the wrong server. Error: " + secmsg, "Contact your admin", MessageBoxButton.OK, MessageBoxImage.Error);
            //    //    Application.Current.Shutdown();
            //    //}
            //}
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btSetup_MouseEnter(object sender, MouseEventArgs e)
        {
            this.imSetupNormal.Visibility = System.Windows.Visibility.Hidden;
            this.imSetupHover.Visibility = System.Windows.Visibility.Visible;
        }

        private void btSetup_MouseLeave(object sender, MouseEventArgs e)
        {
            this.imSetupHover.Visibility = System.Windows.Visibility.Hidden;
            if (!SetupActive)
            {
                this.imSetupNormal.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void btSetup_Click(object sender, RoutedEventArgs e)
        {
            showSetup();
        }

        private void bLogin_MouseEnter(object sender, MouseEventArgs e)
        {
            this.imLoginNormal.Visibility = System.Windows.Visibility.Hidden;
            this.imLoginHover.Visibility = System.Windows.Visibility.Visible;
        }

        private void bLogin_MouseLeave(object sender, MouseEventArgs e)
        {
            this.imLoginHover.Visibility = System.Windows.Visibility.Hidden;
            this.imLoginNormal.Visibility = System.Windows.Visibility.Visible;
        }

        private void bLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!gotZones)
            {
                Properties.Settings.Default.Username = tbUser.Text;
                Properties.Settings.Default.Save();
                engineSettings.FullScreen = !((bool)cbWindowMode.IsChecked);
                engineSettings.SaveSettings();
                RFLogin = new Login(tbUser.Text, tbPassword.Password, IP);
                int returnvalue = RFLogin.getZones();
                if (returnvalue != 0)
                {
                    MessageBox.Show(RFLogin.getLastError(), "RF Launcher Error " + returnvalue.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    gotZones = true;
                    
                    string[] zoneservers = RFLogin.getServerList();
                    byte[] zonestates = RFLogin.getServerStates();
                    brh = new Brush[zoneservers.Length];
                    for (int i = 0; i < zoneservers.Length; i++)
                    {
                        ComboBoxItem tmp = new ComboBoxItem();
                        if (zonestates[i] == 1)
                            tmp.Foreground = Brushes.Green;
                        else
                            tmp.Foreground = Brushes.Red;
                        brh[i] = tmp.Foreground;
                        tmp.Content = zoneservers[i];
                        tmp.IsEnabled = (zonestates[i] == 1);
                        cbZones.Items.Add(tmp);
                    }
                    //cbZones.SelectedIndex = ((Properties.Settings.Default.LastZone > zoneservers.Length) || (zonestates[Properties.Settings.Default.LastZone] != 0) ? 0 : Properties.Settings.Default.LastZone);
                    cbZones.SelectedIndex = 0;
                    imLoginHover.Source = new BitmapImage(new Uri("/rlgn;component/Images/start_hover.png", UriKind.Relative));
                    imLoginNormal.Source = new BitmapImage(new Uri("/rlgn;component/Images/start_normal.png", UriKind.Relative));

                    tbPassword.Visibility = System.Windows.Visibility.Hidden;
                    tbUser.Visibility = System.Windows.Visibility.Hidden;
                    imTextPass.Visibility = System.Windows.Visibility.Hidden;
                    imTextUser.Visibility = System.Windows.Visibility.Hidden;
                    lbLoginDetails.Visibility = System.Windows.Visibility.Hidden;
                   // lbSelectZone.Visibility = System.Windows.Visibility.Visible;
                   // cbZones.Visibility = System.Windows.Visibility.Visible;
                    bLogin_Click(sender, e);
                }
            }
            else
            {
                if (cbZones.SelectedIndex < 0)
                {
                    bool anyopen = false;
                    byte[] zonestates = RFLogin.getServerStates();
                    foreach (byte b in zonestates)
                    {
                        if (b == 1)
                            anyopen = true;
                    }
                    if (anyopen)
                    {
                        MessageBox.Show("Please select a Server!", "RF Launcher Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("At the moment, all servers are closed. Please contact an admin or try again in a few minutes.", "RF Launcher Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    engineSettings.Resolution = ((bool)cbWindowMode.IsChecked ? cbWMResolution.SelectedItem.ToString() : cbResolution.SelectedItem.ToString());
                    engineSettings.SaveSettings();
                    Properties.Settings.Default.LastZone = cbZones.SelectedIndex;
                    Properties.Settings.Default.Save();
                    int returnvalue = RFLogin.doLogin((byte)cbZones.SelectedIndex);
                    if (returnvalue != 0)
                    {
                        MessageBox.Show(RFLogin.getLastError(), "RF Launcher Error " + returnvalue.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        System.Diagnostics.Process rfproc = new System.Diagnostics.Process();
                        try
                        {

                            rfproc.StartInfo.FileName = "RF_Online.bin";
                            rfproc.StartInfo.UseShellExecute = false;
                            rfproc.Start();
                            System.Threading.Thread.Sleep(250);
                            Application.Current.Shutdown();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Process Spawn error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        //if (rfproc.Id != 0 && !rfproc.HasExited)
                        //{
                        //    // rf_online.bin has started
                        //    RFLogin.DisposeSocket();

                        //    HideAndWatch(rfproc, RFLogin.Username);
                        //    Properties.Settings.Default.ClE = true;
                        //    Properties.Settings.Default.Save();
                        //    Application.Current.Shutdown();
                        //}
                    }
                }
            }
        }

        private void HideAndWatch(System.Diagnostics.Process rfproc, string username)
        {
            this.ShowInTaskbar = false;
            this.Close();

            wdt = new System.Threading.Timer((e) => { CheckForWpe(username, wdt); }, null, 0, Convert.ToInt64(TimeSpan.FromMinutes(2).TotalMilliseconds));

            rfproc.WaitForExit();

            //MessageBox.Show(rfproc.ExitCode.ToString(), "Exit code", MessageBoxButton.OK, MessageBoxImage.Information);
            //It's simple - exit code 0 is thrown reliably on a clean exit
            //and terminating over the task manager produces "1" as an exit code
            //so we will just check for !=0

            if (rfproc.ExitCode != 0)
            {
                try
                {
                    System.Net.WebRequest req = System.Net.WebRequest.Create("#");
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.Method = "POST";
                    //We need to count how many bytes we're sending. 
                    //Post'ed Faked Forms should be name=value&
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes("user=" + username + "&reason=1&details=" + rfproc.ExitCode.ToString());
                    req.ContentLength = bytes.Length;
                    System.IO.Stream os = req.GetRequestStream();
                    os.Write(bytes, 0, bytes.Length);
                    os.Close();
                }
                catch 
                {
                }
            }
            wdt.Dispose();
        }

        private void CheckForWpe(string username, System.Threading.Timer t)
        {
            Process[] processlist = Process.GetProcesses();
            string[] suspicions = {"PacketEditor", "Packedit", "Scapy", "WPE", "cheateng"};
            bool foundsome = false;
            foreach (Process p in processlist)
            {
                for (int i = 0; i < suspicions.Length; i++)
                {
                    if (suspicions[i].ToLower().IndexOf(p.ProcessName.ToLower()) >= 0 || p.ProcessName.ToLower().IndexOf(suspicions[i].ToLower()) >= 0)
                    {
                        try
                        {
                            System.Net.WebRequest req = System.Net.WebRequest.Create("#");
                            req.ContentType = "application/x-www-form-urlencoded";
                            req.Method = "POST";
                            //We need to count how many bytes we're sending. 
                            //Post'ed Faked Forms should be name=value&
                            byte[] bytes = System.Text.Encoding.ASCII.GetBytes("user=" + username + "&reason=3&details=" + p.ProcessName);
                            req.ContentLength = bytes.Length;
                            System.IO.Stream os = req.GetRequestStream();
                            os.Write(bytes, 0, bytes.Length);
                            os.Close();
                            foundsome = true;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            if (foundsome)
            {
                t.Dispose();
            }
        }

        private void showSetup()
        {
            lbLoginDetails.Visibility = System.Windows.Visibility.Hidden;
            imTextPass.Visibility = System.Windows.Visibility.Hidden;
            imTextUser.Visibility = System.Windows.Visibility.Hidden;
            tbPassword.Visibility = System.Windows.Visibility.Hidden;
            tbUser.Visibility = System.Windows.Visibility.Hidden;
            btSetup.Visibility = System.Windows.Visibility.Hidden;
            bLogin.Visibility = System.Windows.Visibility.Hidden;
            btSetup.Visibility = System.Windows.Visibility.Hidden;
            imLoginHover.Visibility = System.Windows.Visibility.Hidden;
            imLoginNormal.Visibility = System.Windows.Visibility.Hidden;
            imSetupHover.Visibility = System.Windows.Visibility.Hidden;
            imSetupNormal.Visibility = System.Windows.Visibility.Hidden;
            cbWindowMode.Visibility = System.Windows.Visibility.Hidden;
            btVolume.IsEnabled = false;
            imSoundOff.Visibility = System.Windows.Visibility.Hidden;
            imSoundOn.Visibility = System.Windows.Visibility.Hidden;
            //lbUpdate.Visibility = System.Windows.Visibility.Hidden;
            lbSelectZone.Visibility = System.Windows.Visibility.Hidden;
            cbZones.Visibility = System.Windows.Visibility.Hidden;

            label1.Visibility = System.Windows.Visibility.Visible;
            label3.Visibility = System.Windows.Visibility.Visible;
            label4.Visibility = System.Windows.Visibility.Visible;
            label5.Visibility = System.Windows.Visibility.Visible;
            label6.Visibility = System.Windows.Visibility.Visible;
            label7.Visibility = System.Windows.Visibility.Visible;
            label8.Visibility = System.Windows.Visibility.Visible;
            label9.Visibility = System.Windows.Visibility.Visible;
            cbDetText.Visibility = System.Windows.Visibility.Visible;
            cbDLight.Visibility = System.Windows.Visibility.Visible;
            cbGamma.Visibility = System.Windows.Visibility.Visible;
            cbGlowEffect.Visibility = System.Windows.Visibility.Visible;
            cbGraphAd.Visibility = System.Windows.Visibility.Visible;
            cbMouseAccel.Visibility = System.Windows.Visibility.Visible;
            cbResolution.Visibility = System.Windows.Visibility.Visible;
            cbWMResolution.Visibility = System.Windows.Visibility.Visible;
            cbShadow.Visibility = System.Windows.Visibility.Visible;
            cbTexture.Visibility = System.Windows.Visibility.Visible;
            btCancelSetup.Visibility = System.Windows.Visibility.Visible;
            btSaveSetup.Visibility = System.Windows.Visibility.Visible;
            //btRepair.Visibility = System.Windows.Visibility.Visible;
            lbPatchVersion.Visibility = System.Windows.Visibility.Visible;

            cbDetText.IsChecked = engineSettings.DetailedTextures;

            cbDLight.Items.Clear();
            cbDLight.Items.Add("None");
            cbDLight.Items.Add("1");
            cbDLight.Items.Add("4");
            cbDLight.Items.Add("Unlimited");
            cbDLight.SelectedIndex = engineSettings.DynamicLight;

            cbGamma.Items.Clear();
            cbGamma.Items.Add("0.8");
            cbGamma.Items.Add("1.0");
            cbGamma.Items.Add("1.2");
            cbGamma.Items.Add("1.4");
            cbGamma.Items.Add("1.6");
            cbGamma.Items.Add("1.8");
            cbGamma.SelectedIndex = (engineSettings.Gamma == (decimal)0.8 ? 0 : engineSettings.Gamma == (decimal)1.2 ? 2 : engineSettings.Gamma == (decimal)1.4 ? 3 : engineSettings.Gamma == (decimal)1.6 ? 4 : engineSettings.Gamma == (decimal)1.8 ? 5 : 1);

            cbGlowEffect.Items.Clear();
            cbGlowEffect.Items.Add("None");
            cbGlowEffect.Items.Add("Low");
            cbGlowEffect.Items.Add("High");
            cbGlowEffect.SelectedIndex = engineSettings.GlowEffect;

            cbMouseAccel.IsChecked = engineSettings.MouseAcceleration;

            cbGraphAd.Items.Clear();

            ManagementObjectSearcher search = new ManagementObjectSearcher("select * from Win32_VideoController");
            foreach (ManagementObject mo in search.Get())
            {
                cbGraphAd.Items.Add(mo.GetPropertyValue("Name").ToString());
            }
            cbGraphAd.SelectedIndex = (cbGraphAd.Items.IndexOf(engineSettings.GraphicsAdapter) > -1 ? cbGraphAd.Items.IndexOf(engineSettings.GraphicsAdapter) : 0);
           
            
            cbShadow.Items.Clear();
            cbShadow.Items.Add("Default");
            cbShadow.Items.Add("+1");
            cbShadow.Items.Add("+10");
            cbShadow.Items.Add("Unlimited");
            cbShadow.SelectedIndex = engineSettings.ShadowDetail;

            cbTexture.Items.Clear();
            cbTexture.Items.Add("Low");
            cbTexture.Items.Add("Default");
            cbTexture.Items.Add("High");
            cbTexture.Items.Add("Very High");
            cbTexture.SelectedIndex = engineSettings.TextureDetail;

           
            SetupActive = true;
        }

        private void btSaveSetup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                engineSettings.DetailedTextures = (bool)cbDetText.IsChecked;
                engineSettings.DynamicLight = cbDLight.SelectedIndex;
                try
                { 
                    decimal tmpgamma = Convert.ToDecimal(cbGamma.SelectedItem, CultureInfo.InvariantCulture);
                    while (tmpgamma > 6)
                    {
                        // this is to avoid confusion with different culture format providers (".", ",", ...)
                        tmpgamma = tmpgamma / 10;
                    }
                    engineSettings.Gamma = tmpgamma;
                }
                catch 
                {
                }
                
                engineSettings.GlowEffect = cbGlowEffect.SelectedIndex;
                engineSettings.MouseAcceleration = (bool)cbMouseAccel.IsChecked;
                engineSettings.Resolution = ((bool)cbWindowMode.IsChecked ? cbWMResolution.SelectedItem.ToString() : cbResolution.SelectedItem.ToString());
                engineSettings.ShadowDetail = cbShadow.SelectedIndex;
                engineSettings.TextureDetail = cbTexture.SelectedIndex;
                engineSettings.GraphicsAdapter = cbGraphAd.SelectedItem.ToString();
                engineSettings.SaveSettings();
                Properties.Settings.Default.LastFSRes = cbResolution.SelectedItem.ToString();
                Properties.Settings.Default.LastWMRes = cbWMResolution.SelectedItem.ToString();
                Properties.Settings.Default.Save();
                hideSetup();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Writing exception.log", "Save error", MessageBoxButton.OK, MessageBoxImage.Error);
                StreamWriter of = new StreamWriter("exception.log");
                of.WriteLine(ex.Message);
                foreach(Key k in ex.Data.Keys)
                {
                    of.WriteLine(k.ToString() + " : " + ex.Data[k].ToString());
                }
            }
        }

        private void hideSetup()
        {
            SetupActive = false;
            
            btSetup.Visibility = System.Windows.Visibility.Visible;
            bLogin.Visibility = System.Windows.Visibility.Visible;
            btSetup.Visibility = System.Windows.Visibility.Visible;
            imLoginHover.Visibility = System.Windows.Visibility.Visible;
            imLoginNormal.Visibility = System.Windows.Visibility.Visible;
            imSetupHover.Visibility = System.Windows.Visibility.Visible;
            imSetupNormal.Visibility = System.Windows.Visibility.Visible;
            cbWindowMode.Visibility = System.Windows.Visibility.Visible;
            //lbUpdate.Visibility = System.Windows.Visibility.Visible;
            btVolume.IsEnabled = true;
           /* if (mediaBgm.Volume > 0.0)
            {
                imSoundOn.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                imSoundOff.Visibility = System.Windows.Visibility.Visible;
            }
            */
            if (!gotZones)
            {
                tbPassword.Visibility = System.Windows.Visibility.Visible;
                tbUser.Visibility = System.Windows.Visibility.Visible;
                lbLoginDetails.Visibility = System.Windows.Visibility.Visible;
                imTextUser.Visibility = System.Windows.Visibility.Visible;
                imTextPass.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                lbSelectZone.Visibility = System.Windows.Visibility.Visible;
                cbZones.Visibility = System.Windows.Visibility.Visible;
            }
            label1.Visibility = System.Windows.Visibility.Hidden;
            label3.Visibility = System.Windows.Visibility.Hidden;
            label4.Visibility = System.Windows.Visibility.Hidden;
            label5.Visibility = System.Windows.Visibility.Hidden;
            label6.Visibility = System.Windows.Visibility.Hidden;
            label7.Visibility = System.Windows.Visibility.Hidden;
            label8.Visibility = System.Windows.Visibility.Hidden;
            label9.Visibility = System.Windows.Visibility.Hidden;
            cbDetText.Visibility = System.Windows.Visibility.Hidden;
            cbDLight.Visibility = System.Windows.Visibility.Hidden;
            cbGamma.Visibility = System.Windows.Visibility.Hidden;
            cbGlowEffect.Visibility = System.Windows.Visibility.Hidden;
            cbGraphAd.Visibility = System.Windows.Visibility.Hidden;
            cbMouseAccel.Visibility = System.Windows.Visibility.Hidden;
            cbResolution.Visibility = System.Windows.Visibility.Hidden;
            cbWMResolution.Visibility = System.Windows.Visibility.Hidden;
            cbShadow.Visibility = System.Windows.Visibility.Hidden;
            cbTexture.Visibility = System.Windows.Visibility.Hidden;
            btCancelSetup.Visibility = System.Windows.Visibility.Hidden;
            btSaveSetup.Visibility = System.Windows.Visibility.Hidden;
            //btRepair.Visibility = System.Windows.Visibility.Hidden;
            lbPatchVersion.Visibility = System.Windows.Visibility.Hidden;
        }

        private void btCancelSetup_Click(object sender, RoutedEventArgs e)
        {
            hideSetup();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            string[] args = App.mArgs;
            if (args.Length > 0)
            {
                foreach (string arg in args)
                {
                    if (arg.StartsWith("--version="))
                    {
                        int argversion = Convert.ToInt32(arg.Substring(arg.IndexOf('=') + 1));
                        Properties.Settings.Default.Version = argversion;
                        Properties.Settings.Default.Save();
                    }
                    if (arg.StartsWith("--nopatchplease"))
                    {
                        pdnu = true;
                    }
                    if (arg.StartsWith("--ip="))
                    {
                        this.IP = arg.Substring(arg.IndexOf('=') + 1);
                    }
                }
            }
            // DEBUG MODE ON
            // pdnu = true;
            //MessageBox.Show("This is a debugging launcher!", "Debug mode", MessageBoxButton.OK, MessageBoxImage.Stop);  
            // DEBUG MODE OFF

            /* if (File.Exists("Snd\\Bgm\\Launcher_01.mp3"))
            {
                mediaBgm.Visibility = System.Windows.Visibility.Visible;
                mediaBgm.Source = new Uri("Snd\\Bgm\\Launcher_01.mp3", UriKind.Relative);
                mediaBgm.LoadedBehavior = MediaState.Manual;
                mediaBgm.Volume = (Properties.Settings.Default.VolumeOn ? 0.5 : 0.0);
                mediaBgm.Play();
                this.btVolume.IsEnabled = true;
                imSoundOn.Visibility = (Properties.Settings.Default.VolumeOn ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden);
                imSoundOff.Visibility = (Properties.Settings.Default.VolumeOn ? System.Windows.Visibility.Hidden : System.Windows.Visibility.Visible);
            } */
            bLogin.IsEnabled = true;
            lbPatchVersion.Content += Properties.Settings.Default.Version.ToString();
            if (!pdnu)
            {
                if (UpdateAvailable())
                {
                    bLogin.IsEnabled = false;
                    btUpdate_Click(this, new RoutedEventArgs());
                }
            }
            if (tbUser.Text != "Username")
            {
                tbPassword.Focus();
            }

            double maxX = 2560;
            double maxY = 1440;
            double[][] val = new double[][] { new double[] { 800, 600 }, new double[] { 1024, 768 }, new double[] { 1152, 864 }, new double[] { 1280, 720 }, new double[] { 1280, 800 }, new double[] { 1280, 960 }, new double[] { 1280, 1024 }, new double[] { 1360, 768 }, new double[] { 1366, 768 }, new double[] { 1440, 900 }, new double[] { 1600, 900 }, new double[] { 1600, 1200 }, new double[] { 1680, 1050 }, new double[] { 1920, 1080 }, new double[] {1920, 1200 }, new double[] { maxX, maxY } };
            cbWMResolution.Items.Clear();
            cbResolution.Items.Clear();
            for (int i = 0; i < val.Length; i++)
            {
                if (maxX >= val[i][0] && maxY >= val[i][1])
                {
                    cbResolution.Items.Add(val[i][0].ToString() + "x" + val[i][1].ToString());
                    cbWMResolution.Items.Add(val[i][0].ToString() + "x" + val[i][1].ToString());
                }
            }

            cbResolution.SelectedItem = (Properties.Settings.Default.LastFSRes.Length > 1 ? Properties.Settings.Default.LastFSRes : engineSettings.Resolution);
            cbWMResolution.SelectedItem = (Properties.Settings.Default.LastWMRes.Length > 1 ? Properties.Settings.Default.LastWMRes : engineSettings.Resolution);

            if (cbResolution.SelectedIndex < 0)
            {
                cbResolution.SelectedIndex = 0;
            }
            if (cbWMResolution.SelectedIndex < 0)
            {
                cbWMResolution.SelectedIndex = 0;
            }
        }

        private bool UpdateAvailable()
        {
            bool ret = false;
            WebClient updateServerLoader = new WebClient();
            try
            {
                //TODO ULR ausbessern
                string versionstring = updateServerLoader.DownloadString("http://update1.rfstasis.com/index.html");
                baseVersionS = Convert.ToInt32(versionstring);
                if (baseVersionS > Properties.Settings.Default.Version)
                {
                    ret = true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Update check error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return ret;
        }


        //private void btGPlus_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Topmost = false;
        //    this.ShowInTaskbar = true;
        //    System.Diagnostics.Process gplus = new System.Diagnostics.Process();
        //    gplus.StartInfo = new System.Diagnostics.ProcessStartInfo("#");
        //    gplus.Start();
        //}

        //private void btFB_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Topmost = false;
        //    this.ShowInTaskbar = true;
        //    System.Diagnostics.Process face = new System.Diagnostics.Process();
        //    face.StartInfo = new System.Diagnostics.ProcessStartInfo("https://www.facebook.com/RF-Stasis-International-107974847366873/");
        //    face.Start();
        //}

        //private void btTwitter_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Topmost = false;
        //    this.ShowInTaskbar = true;
        //    System.Diagnostics.Process tweet = new System.Diagnostics.Process();
        //    tweet.StartInfo = new System.Diagnostics.ProcessStartInfo("#");
        //    tweet.Start();
        //}

        //private void button1_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Topmost = false;
        //    this.ShowInTaskbar = true;
        //    System.Diagnostics.Process urli = new System.Diagnostics.Process();
        //    urli.StartInfo = new System.Diagnostics.ProcessStartInfo("http://www.rfstasis.com");
        //    urli.Start();
        //}

        private void btGPlus_MouseEnter(object sender, MouseEventArgs e)
        {
            tmpCursor = this.Cursor;
            this.Cursor = Cursors.Hand;
        }

        private void btGPlus_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Cursor = tmpCursor;
        }

        /* private void btVolume_Click(object sender, RoutedEventArgs e)
        {
            if (this.mediaBgm.Volume > 0.0)
            {
                this.mediaBgm.Volume = 0.0;
                imSoundOn.Visibility = System.Windows.Visibility.Hidden;
                imSoundOff.Visibility = System.Windows.Visibility.Visible;
                Properties.Settings.Default.VolumeOn = false;
                Properties.Settings.Default.Save();
            }
            else
            {
                this.mediaBgm.Volume = 0.5;
                imSoundOff.Visibility = System.Windows.Visibility.Hidden;
                imSoundOn.Visibility = System.Windows.Visibility.Visible;
                Properties.Settings.Default.VolumeOn = true;
                Properties.Settings.Default.Save();
            }
        } */

   /*     private void btRepair_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("You are about to repair the Client files. This may take a long time, cause high CPU load, and use your Internet connection at high bandwith.\n\n" +
                "Do you want to continue?", "Repair Files?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                mediaBgm.Stop();
                mediaBgm.Close();

                hideSetup();
                bLogin.IsEnabled = false;
                //btRepair.IsEnabled = false;

               /* lbUpdate.Visibility = System.Windows.Visibility.Visible;
                lbUpdtPerc.Visibility = System.Windows.Visibility.Visible;
              // * /
                pbUpdtTotal.Visibility = System.Windows.Visibility.Visible;
                pbUpdt.Visibility = System.Windows.Visibility.Visible;
                lbUpdate.Visibility = System.Windows.Visibility.Visible;
                lbUpdate.Content = "Repair:";
                updater.CheckRepair();
            }
        }
*/
        private void cbZones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbZones.Foreground = brh[cbZones.SelectedIndex];
        }

        private void btUpdate_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process updtproc = new System.Diagnostics.Process();
            try
            {
                updtproc.StartInfo.FileName = "update.exe";
                updtproc.StartInfo.UseShellExecute = false;
                updtproc.Start();
                System.Threading.Thread.Sleep(450); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Process Spawn error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            try
            {
                if (updtproc.Id != 0 && !updtproc.HasExited)
                {
                    //RFLogin.DisposeSocket();
                    Application.Current.Shutdown(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Process termination error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void tbUser_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text == "Username")
            {
                tb.Text = "";
            }
            tb.Foreground = Brushes.Black;
        }

        private void tbPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox pb = (PasswordBox)sender;
            if (pb.Password == "Password")
            {
                pb.Password = "";
            }
            pb.Foreground = Brushes.Black;
        }

        private void btRegister_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://gamecp.rfstasis.com");
        }
    }
}
