/*
 * Created using SharpDevelop, Visual studio 2017 and Monodevelop.
 * Developer: Luke Liukonen 
 * Date: 10/11/2017
 * Time: 10:17 PM
 */
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;



namespace WeatherDesktop
{
    public sealed class NotificationIcon
    {

       #region constants
        const string cDay = "day-";
        const string cNight = "night-";
        #endregion

        #region global Objects
        private NotifyIcon notifyIcon;
        private ContextMenu notificationMenu;
        private Interfaces.MSWeather g_Weather;
        private Interfaces.SunRiseSet g_SunRiseSet;
        private Dictionary<string, string> g_ImageDictionary = new Dictionary<string, string>();
        private string g_CurrentWeatherType;
        #endregion

        #region Initialize icon and menu
        public NotificationIcon()
        {
            DeclareGlobals();
            notifyIcon = new NotifyIcon();
            notificationMenu = new ContextMenu(InitializeMenu());
            notifyIcon.DoubleClick += IconDoubleClick;
            ComponentResourceManager resources = new ComponentResourceManager(typeof(NotificationIcon));
            notifyIcon.Icon = (Icon)resources.GetObject("$this.Icon");
            notifyIcon.ContextMenu = notificationMenu;
        }

        private MenuItem[] InitializeMenu()
        {


            MenuItem[] menu = new MenuItem[] {
                new MenuItem("Images", GetWeatherMenuItems()),
                new MenuItem("About", MenuAboutClick),
                new MenuItem("Exit", MenuExitClick)
            };
            return menu;
        }

        private MenuItem[] GetWeatherMenuItems()
        {
            System.Collections.Generic.List<MenuItem> items = new System.Collections.Generic.List<MenuItem>();
            foreach (var element in System.Enum.GetValues(typeof(Interfaces.Shared.WeatherTypes)))
            {
                string ElementName = Enum.GetName(typeof(Interfaces.Shared.WeatherTypes), element);
                string DayName = cDay + ElementName;
                string NightName = cNight + ElementName;

                if (g_ImageDictionary.ContainsKey(DayName)) { DayName = "*" + DayName; }
                if (g_ImageDictionary.ContainsKey(NightName)) { NightName = "*" + NightName; }
                items.Add(new MenuItem(DayName, MenuItemClick));
                items.Add(new MenuItem(NightName, MenuItemClick));

            }
            return items.ToArray();

        }

        #endregion

        #region Main - Program entry point
        /// <summary>Program entry point.</summary>
        /// <param name="args">Command Line Arguments</param>
        [STAThread]
        public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Please use a unique name for the mutex to prevent conflicts with other programs
            using (Mutex mtx = new Mutex(true, "WeatherDesktop", out bool isFirstInstance))
            {
                if (isFirstInstance)
                {
                    NotificationIcon notificationIcon = new NotificationIcon();
                    notificationIcon.notifyIcon.Visible = true;
                    Application.Run();
                    notificationIcon.notifyIcon.Dispose();
                }
                else
                {
                    // The application is already running
                    // TODO: Display message box or change focus to existing application instance
                }
            }
        }
        #endregion

        #region Event Handlers

        private void MenuItemClick(object sender, EventArgs e)
        {

            string Name = ((MenuItem)sender).Text.Replace("*", string.Empty);
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "jpeg (*.jpg)|*.jpg|Portable Network Graphics (*.png)|*.png",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Interfaces.Shared.AddUpdateAppSettings(Name, openFileDialog1.FileName);
                if (g_ImageDictionary.ContainsKey(Name)) { g_ImageDictionary[Name] = openFileDialog1.FileName; }
                else { g_ImageDictionary.Add(Name, openFileDialog1.FileName); }
                UpdateScreen(true);
                notificationMenu = new ContextMenu(InitializeMenu());
                notifyIcon.ContextMenu = notificationMenu;
            }
        }

        private void MenuAboutClick(object sender, EventArgs e)
        {
            MessageBox.Show("weather desktop, by Luke Liukonen, 2017" + Environment.NewLine + g_Weather.Debug() + g_SunRiseSet.Debug());
        }

        private void MenuExitClick(object sender, EventArgs e){Application.Exit();}

        private void IconDoubleClick(object sender, EventArgs e){MessageBox.Show(g_Weather.Invoke().ForcastDescription);}

        private void OnTimedEvent(object sender, EventArgs e){UpdateScreen(false);}

        #endregion

        #region Private Functions

        private void UpdateScreen(Boolean overrideImage)
        {
            var weather = g_Weather.Invoke();
            var sunriseSet = g_SunRiseSet.Invoke();
            string currentTime;

            if(Interfaces.Shared.BetweenTimespans(DateTime.Now.TimeOfDay, sunriseSet.SunRise.TimeOfDay, sunriseSet.SunSet.TimeOfDay)){ currentTime = cDay; } else { currentTime = cNight; }

            string weatherType = Enum.GetName(typeof(Interfaces.Shared.WeatherTypes), weather.WType);
            notifyIcon.Text = weatherType + " " + weather.Temp.ToString();
            string currentWeatherType = currentTime + weatherType;
            if (string.IsNullOrWhiteSpace(g_CurrentWeatherType) || currentWeatherType != g_CurrentWeatherType || overrideImage)
            {
                g_CurrentWeatherType = currentWeatherType;
                if (g_ImageDictionary.ContainsKey(currentWeatherType))
                {
                    try { Interfaces.Wallpaper.Set(g_ImageDictionary[currentWeatherType], Interfaces.Wallpaper.Style.Stretched); }
                    catch (Exception x) { MessageBox.Show(x.ToString()); }
                }
            }
        }

        private void DeclareGlobals()
        {      
            g_Weather = new Interfaces.MSWeather(Getzip());
            UpdateSunRiseSetService(g_Weather.Latitude, g_Weather.Longitude);
            UpdateImageCache();
            CreateTimer();
        }

        #region Support functions to reduce complexity

        private int Getzip()
        {
            const string cZip = "zipcode";
            int zip = 0;
            string zipcode = Interfaces.Shared.ReadSetting(cZip);
            if (string.IsNullOrWhiteSpace(zipcode))
            {
                try
                {
                    zip = int.Parse(Microsoft.VisualBasic.Interaction.InputBox("Please enter your zipcode", "Zip Code"));
                    Interfaces.Shared.AddUpdateAppSettings(cZip, zip.ToString());
                    zipcode = zip.ToString();
                }
                catch (Exception x) { MessageBox.Show("an error occured. please restart..." + x.ToString()); Application.Exit(); }
            }
            else { zip = int.Parse(zipcode); }
            return zip;
        }

        private void CreateTimer()
        {
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 1000; // specify interval time as you want
            t.Tick += new EventHandler(OnTimedEvent);
            t.Start();
        }

        private void UpdateImageCache()
        {
            foreach (var element in System.Enum.GetValues(typeof(Interfaces.Shared.WeatherTypes)))
            {
                string ElementName = Enum.GetName(typeof(Interfaces.Shared.WeatherTypes), element);
                string daykey = cDay + ElementName;
                string nightKey = cNight + ElementName;
                string dayimageCache = Interfaces.Shared.ReadSetting(cDay + ElementName);
                string nightimagecache = Interfaces.Shared.ReadSetting(cNight + ElementName);
                if (!string.IsNullOrEmpty(dayimageCache)) { g_ImageDictionary.Add(daykey, dayimageCache); }
                if (!string.IsNullOrEmpty(nightimagecache)) { g_ImageDictionary.Add(nightKey, nightimagecache); }
            }
        }

        private void UpdateSunRiseSetService(double WeatherLatitude, double WeatherLongitude)
        {
            double lat = WeatherLatitude; double lng = WeatherLongitude;
            if (lat == 0 && lng == 0 && !string.IsNullOrWhiteSpace(Interfaces.Shared.ReadSetting("Latitude")))
            {
                lat = int.Parse(Interfaces.Shared.ReadSetting("Latitude"));
                lng = int.Parse(Interfaces.Shared.ReadSetting("Longitude"));
            }
            if (!string.IsNullOrWhiteSpace(Interfaces.Shared.ReadSetting("HourUpdate")) && lat != 0)
            {
                g_SunRiseSet = new Interfaces.SunRiseSet(lat, lng, int.Parse(Interfaces.Shared.ReadSetting("HourUpdate")));
            }
            else if (lat != 0) { g_SunRiseSet = new Interfaces.SunRiseSet(lat, lng); }
            else { g_SunRiseSet = new Interfaces.SunRiseSet(); }

        }
        #endregion

        #endregion

    }
}
