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
        const string cWeather = "gWeatherapp";
        const string cSRS = "gsunRiseSet";
        #endregion

        #region global Objects
        private NotifyIcon notifyIcon;
        private ContextMenu notificationMenu;
        private Interface.ISharedWeatherinterface g_Weather;
        private Interface.IsharedSunRiseSetInterface g_SunRiseSet;
        private Dictionary<string, string> g_ImageDictionary = new Dictionary<string, string>();
        private string g_CurrentWeatherType;

        List<Byte> HoursBlackLsited = new List<byte>();
        System.Collections.BitArray BlackListHours = new System.Collections.BitArray(24);
        System.Collections.BitArray BlackListDays = new System.Collections.BitArray(7);

        Type[] WeatherTypes = new Type[] { typeof(Interface.MSWeather), typeof(Interface.Mock_Weather), typeof(Interface.OpenWeatherMap) };
        Type[] SunRiseSetTypes = new Type[] { typeof(Interface.SunRiseSet), typeof(Interface.Mock_SunRiseSet) };
        #endregion

        #region Initialize icon and menu
        public NotificationIcon()
        {
            DeclareGlobals();
            notifyIcon = new NotifyIcon();
            notificationMenu = new ContextMenu(InitializeMenu());
            notifyIcon.DoubleClick += IconDoubleClick;
            ComponentResourceManager resources = new ComponentResourceManager(typeof(NotificationIcon));
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.ContextMenu = notificationMenu;
        }

        private MenuItem[] InitializeMenu()
        {


            MenuItem[] menu = new MenuItem[] {
                new MenuItem("Settings", GetSettings()),
                new MenuItem("About", MenuAboutClick),
                new MenuItem("Exit", MenuExitClick)
            };

            return menu;
        }

        private MenuItem[] GetSettings()
        {
            List<MenuItem> Items = new List<MenuItem>();
            Items.Add(new MenuItem("Global", GlobalMenuSettings()));
            Items.Add(new MenuItem("images", GetWeatherMenuItems()));
            Items.Add(new MenuItem(g_SunRiseSet.GetType().Name, g_SunRiseSet.SettingsItems()));
            Items.Add(new MenuItem(g_Weather.GetType().Name, g_Weather.SettingsItems()));
            return Items.ToArray();
        }

        private MenuItem[] GlobalMenuSettings()
        {
            List<MenuItem> Items = new List<MenuItem>();
            List<MenuItem> BlackLists = new List<MenuItem>();
            BlackLists.Add(new MenuItem("BlackList Hours", BlackListHours_Event));
            BlackLists.Add(new MenuItem("BlackList Days", BlackListDays_event));
            Items.Add(new MenuItem("BlackListing", BlackLists.ToArray()));

            List<MenuItem> WeatherItems = new List<MenuItem>();
            List<MenuItem> SunRiseSetItems = new List<MenuItem>();
            string SelectedItem = Interface.Shared.ReadSetting(cWeather);
            string SelectedSRS = Interface.Shared.ReadSetting(cSRS);

            foreach (var item in WeatherTypes)
            {
                MenuItem ItemToAdd = new MenuItem(item.FullName, UpdateGlobalObjecttype);
                if (item.FullName == SelectedItem) { ItemToAdd.Checked = true; }
                WeatherItems.Add(ItemToAdd);
            }
            foreach (var item in SunRiseSetTypes)
            {
                MenuItem ItemToAdd = new MenuItem(item.FullName, UpdateGlobalObjecttype);
                if (item.FullName == SelectedSRS) { ItemToAdd.Checked = true; }
                SunRiseSetItems.Add(ItemToAdd);
            }
            Items.Add(new MenuItem("Weather", WeatherItems.ToArray()));
            Items.Add(new MenuItem("SunRiseSet", SunRiseSetItems.ToArray()));

            return Items.ToArray();

        }
        private MenuItem[] GetWeatherMenuItems()
        {
            System.Collections.Generic.List<MenuItem> items = new System.Collections.Generic.List<MenuItem>();
            foreach (var element in System.Enum.GetValues(typeof(Interface.Shared.WeatherTypes)))
            {
                string ElementName = Enum.GetName(typeof(Interface.Shared.WeatherTypes), element);
                string DayName = cDay + ElementName;
                string NightName = cNight + ElementName;

                MenuItem dayItem = new MenuItem(DayName, MenuItemClick);
                MenuItem Nightitem = new MenuItem(NightName, MenuItemClick);

                dayItem.Checked = (g_ImageDictionary.ContainsKey(DayName));
                Nightitem.Checked = (g_ImageDictionary.ContainsKey(NightName));
                items.Add(dayItem);
                items.Add(Nightitem);

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

            bool isFirstInstance = false;
            // Please use a unique name for the mutex to prevent conflicts with other programs

            using (Mutex mtx = new Mutex(true, "WeatherDesktop", out isFirstInstance))
            {
                if (isFirstInstance)
                {
                    try
                    {
                        NotificationIcon notificationIcon = new NotificationIcon();
                        notificationIcon.notifyIcon.Visible = true;
                        GC.Collect();
                        Application.Run();
                        notificationIcon.notifyIcon.Dispose();
                    }
                    catch(Exception x)
                    { MessageBox.Show("Error: " + x.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    mtx.ReleaseMutex();
                }
                else
                {
                    GC.Collect();
                    MessageBox.Show("App appears to be running. if not, you may have to restart your machine to get it to work.");
                }
            }



        }
        #endregion

        #region Event Handlers

        private void MenuItemClick(object sender, EventArgs e)
        {

            string Name = ((MenuItem)sender).Text;//.Replace("*", string.Empty);
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "jpeg (*.jpg)|*.jpg|Portable Network Graphics (*.png)|*.png",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Interface.Shared.AddUpdateAppSettings(Name, openFileDialog1.FileName);
                if (g_ImageDictionary.ContainsKey(Name)) { g_ImageDictionary[Name] = openFileDialog1.FileName; }
                else { g_ImageDictionary.Add(Name, openFileDialog1.FileName); }
                UpdateScreen(true);
                notificationMenu = new ContextMenu(InitializeMenu());
                notifyIcon.ContextMenu = notificationMenu;
            }
        }

        private void MenuAboutClick(object sender, EventArgs e)
        {
            Dictionary<string, string> debugValues = new Dictionary<string, string>();
            debugValues.Add("Weather Notifcation Type", g_CurrentWeatherType);

            MessageBox.Show("weather desktop, by Luke Liukonen, 2017" + Environment.NewLine + Interface.Shared.CompileDebug("Main Values", debugValues) + g_Weather.Debug() + g_SunRiseSet.Debug());
        }

        private void MenuExitClick(object sender, EventArgs e) { Application.Exit(); }

        private void IconDoubleClick(object sender, EventArgs e) { MessageBox.Show(((Interface.WeatherResponse)(g_Weather).Invoke()).ForcastDescription); }

        private void OnTimedEvent(object sender, EventArgs e) { UpdateScreen(false); }

        private void BlackListHours_Event(object sender, EventArgs e)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            List<int> values = new List<int>();
            for (int i = 0; i < 24; i++)
            {
                if (BlackListHours[i]) { values.Add(i); }
            }


            string ValuesCSV = Microsoft.VisualBasic.Interaction.InputBox("Enter days in comma seperated values, Military time", "Enter Blacklisted Hours", string.Join(",", values.ToArray()));
            values = new List<int>();
            foreach (string item in ValuesCSV.Split(',')) { values.Add(int.Parse(item.Replace(",", string.Empty))); }
            for (int i = 0; i < 24; i++) { BlackListHours[i] = values.Contains(i); }
            Interface.Shared.AddUpdateAppSettings("BlackListHours", Interface.Shared.ConvertBitarrayToInt(BlackListHours).ToString());
        }

        private void BlackListDays_event(object sender, EventArgs e)
        {
            string ValuesCSV = Microsoft.VisualBasic.Interaction.InputBox("Enter days in comma seperated values, with Sunday = 0 and Saturday = 6, example '0,1,2' = Sunday Monday Tuesday", "Enter Blacklisted Days");
            List<int> values = new List<int>();
            foreach (string item in ValuesCSV.Split(',')) { values.Add(int.Parse(item.Replace(",", string.Empty))); }
            for (int i = 0; i < 7; i++) { BlackListDays[i] = values.Contains(i); }
            Interface.Shared.AddUpdateAppSettings("BlackListDays", Interface.Shared.ConvertBitarrayToInt(BlackListDays).ToString());
        }

        private void UpdateGlobalObjecttype(object sender, EventArgs e)
        {
            MenuItem Current = (MenuItem)sender;
            string Name = Current.Text;
            if (((MenuItem)Current.Parent).Text == "Weather")
            {
                Interface.Shared.AddUpdateAppSettings(cWeather, Name);
                Type S = Type.GetType(Name);
                g_Weather = (Interface.ISharedWeatherinterface)Activator.CreateInstance(S);

            }
            else if (((MenuItem)Current.Parent).Text == "SunRiseSet")
            {
                Interface.Shared.AddUpdateAppSettings(cSRS, Name);
                Type S = Type.GetType(Name);
                g_SunRiseSet = (Interface.IsharedSunRiseSetInterface)Activator.CreateInstance(S);
            }
            notificationMenu = new ContextMenu(InitializeMenu());
            notifyIcon.ContextMenu = notificationMenu;
        }

        #endregion

        #region Private Functions

        private void UpdateScreen(Boolean overrideImage)
        {
            if (!(BlackListDays[(int)DateTime.Now.DayOfWeek] || BlackListHours[DateTime.Now.Hour]))
            {
                var weather = (Interface.WeatherResponse)g_Weather.Invoke();
                var sunriseSet = g_SunRiseSet.Invoke();
                string currentTime;

                if (Interface.Shared.BetweenTimespans(DateTime.Now.TimeOfDay, ((Interface.SunRiseSetResponse)sunriseSet).SunRise.TimeOfDay, ((Interface.SunRiseSetResponse)sunriseSet).SunSet.TimeOfDay)) { currentTime = cDay; } else { currentTime = cNight; }


                string weatherType = Enum.GetName(typeof(Interface.Shared.WeatherTypes), weather.WType);
                notifyIcon.Text = weatherType + " " + weather.Temp.ToString();
                string currentWeatherType = currentTime + weatherType;
                if (string.IsNullOrWhiteSpace(g_CurrentWeatherType) || currentWeatherType != g_CurrentWeatherType || overrideImage)
                {
                    g_CurrentWeatherType = currentWeatherType;
                    if (g_ImageDictionary.ContainsKey(currentWeatherType))
                    {
                        try { Shared.Wallpaper.Set(g_ImageDictionary[currentWeatherType], Shared.Wallpaper.Style.Stretched); }
                        catch (Exception x) { MessageBox.Show(x.ToString()); }
                    }
                }
            }
        }

        private void DeclareGlobals()
        {
            try {
                Type T = Type.GetType(Interface.Shared.ReadSetting(cWeather));
                g_Weather = (Interface.ISharedWeatherinterface)Activator.CreateInstance(T);
                Type S = Type.GetType(Interface.Shared.ReadSetting(cSRS));
                g_SunRiseSet = (Interface.IsharedSunRiseSetInterface)Activator.CreateInstance(S);

            }

            catch {
                g_Weather = new Interface.Mock_Weather();
                g_SunRiseSet = new Interface.Mock_SunRiseSet();
                Interface.Shared.AddUpdateAppSettings(cWeather, g_Weather.GetType().FullName);
                Interface.Shared.AddUpdateAppSettings(cSRS, g_SunRiseSet.GetType().FullName);
            }

            UpdateImageCache();
            UpdateBlackLists();
            CreateTimer();
        }

        #region Support functions to reduce complexity

         private void UpdateBlackLists()
        {
            int intBlackListdays = 0;
            int intblackListHours = 0;
            int.TryParse(Interface.Shared.ReadSetting("BlackListHours"), out intblackListHours);
            int.TryParse(Interface.Shared.ReadSetting("BlackListDays"), out intBlackListdays);
            BlackListHours = Interface.Shared.ConverTIntToBitArray(intblackListHours);
            BlackListDays = Interface.Shared.ConverTIntToBitArray(intBlackListdays);

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
            foreach (var element in System.Enum.GetValues(typeof(Interface.Shared.WeatherTypes)))
            {
                string ElementName = Enum.GetName(typeof(Interface.Shared.WeatherTypes), element);
                string daykey = cDay + ElementName;
                string nightKey = cNight + ElementName;
                string dayimageCache = Interface.Shared.ReadSetting(cDay + ElementName);
                string nightimagecache = Interface.Shared.ReadSetting(cNight + ElementName);
                if (!string.IsNullOrEmpty(dayimageCache)) { g_ImageDictionary.Add(daykey, dayimageCache); }
                if (!string.IsNullOrEmpty(nightimagecache)) { g_ImageDictionary.Add(nightKey, nightimagecache); }
            }
        }
        
        #endregion

        #endregion

    }
}
