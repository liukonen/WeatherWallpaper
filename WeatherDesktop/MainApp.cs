using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using WeatherDesktop.Share;

namespace WeatherDesktop
{
    public sealed class MainApp : IDisposable
    {
        #region Lazy Load objects
#pragma warning disable 0649
        [ImportMany]
        IEnumerable<Lazy<Interface.ISharedWeatherinterface, Interface.IClassName>> WeatherObjects;

        [ImportMany]
        IEnumerable<Lazy<Interface.IsharedSunRiseSetInterface, Interface.IClassName>> SRSObjects;

        [ImportMany]
        IEnumerable<Lazy<Interface.ILatLongInterface, Interface.IClassName>> LatLongObjects;
#pragma warning restore 0649
        #endregion

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
        private Share.ThemeHandler Themes = new Share.ThemeHandler();
        private Dictionary<string, string> g_ImageDictionary = new Dictionary<string, string>();
        private string g_CurrentWeatherType;
        private CompositionContainer _container;

        System.Collections.BitArray BlackListHours = new System.Collections.BitArray(24);
        System.Collections.BitArray BlackListDays = new System.Collections.BitArray(7);
        string PluginPaths = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + "Plugins";

        #endregion

        #region Initialize icon and menu
        public MainApp()
        {
            LazyLoader();
            DeclareGlobals();
            notifyIcon = new NotifyIcon();
            notificationMenu = new ContextMenu(InitializeMenu());
            notifyIcon.DoubleClick += IconDoubleClick;
            ComponentResourceManager resources = new ComponentResourceManager(typeof(MainApp));
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.ContextMenu = notificationMenu;
            CreateTimer();
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
            List<MenuItem> Items = new List<MenuItem>
            {
                new MenuItem("Global", GlobalMenuSettings()),
                new MenuItem("images", GetWeatherMenuItems()),
                new MenuItem(g_SunRiseSet.GetType().Name, g_SunRiseSet.SettingsItems()),
                new MenuItem(g_Weather.GetType().Name, g_Weather.SettingsItems()),
                new MenuItem("Themes", Themes.SettingsItems())
            };
            return Items.ToArray();
        }

        private MenuItem[] GlobalMenuSettings()
        {
            List<MenuItem> Items = new List<MenuItem>{new MenuItem("Plugin Folder", PluginFolder_Event)};
            List<MenuItem> BlackLists = new List<MenuItem> {new MenuItem("BlackList Hours", BlackListHours_Event), new MenuItem("BlackList Days", BlackListDays_event)};
            Items.Add(new MenuItem("BlackListing", BlackLists.ToArray()));
            
            List<MenuItem> WeatherItems = new List<MenuItem>();
            List<MenuItem> SunRiseSetItems = new List<MenuItem>();
            string SelectedItem = SharedObjects.AppSettings.ReadSetting(cWeather);
            string SelectedSRS = SharedObjects.AppSettings.ReadSetting(cSRS);



            foreach (var item in WeatherObjects)
            {
                MenuItem ItemToAdd = new MenuItem(item.Metadata.ClassName, UpdateGlobalObjecttype);
                if (item.Metadata.ClassName == SelectedItem) { ItemToAdd.Checked = true; }
                WeatherItems.Add(ItemToAdd);
            }
            foreach (var item in SRSObjects)
            {
                MenuItem ItemToAdd = new MenuItem(item.Metadata.ClassName, UpdateGlobalObjecttype);
                if (item.Metadata.ClassName == SelectedSRS) { ItemToAdd.Checked = true; }
                SunRiseSetItems.Add(ItemToAdd);
            }
            Items.Add(new MenuItem("Weather", WeatherItems.ToArray()));
            Items.Add(new MenuItem("SunRiseSet", SunRiseSetItems.ToArray()));

            return Items.ToArray();

        }
        private MenuItem[] GetWeatherMenuItems()
        {
            System.Collections.Generic.List<MenuItem> items = new System.Collections.Generic.List<MenuItem>();
            foreach (var element in System.Enum.GetValues(typeof(SharedObjects.WeatherTypes)))
            {
                string ElementName = Enum.GetName(typeof(SharedObjects.WeatherTypes), element);
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
        public static void Main()
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
                        MainApp notificationIcon = new MainApp();
                        notificationIcon.notifyIcon.Visible = true;
                        GC.Collect();
                        Application.Run();
                        notificationIcon.notifyIcon.Dispose();
                    }
                    catch(Exception x)
                    { MessageBox.Show("Error: " + x.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        WeatherDesktop.Share.ErrorHandler.Send(x);
                    }
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

        private void PluginFolder_Event(object sender, EventArgs e)
        {
            if (System.IO.Directory.Exists(PluginPaths)) {System.Diagnostics.Process.Start(PluginPaths); }
        }
        private void MenuItemClick(object sender, EventArgs e)
        {

            string Name = ((MenuItem)sender).Text;//.Replace("*", string.Empty);
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "jpeg (*.jpg)|*.jpg|Portable Network Graphics (*.png)|*.png",
                FilterIndex = 1,
                RestoreDirectory = true,
                Title = Name
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SharedObjects.AppSettings.AddUpdateAppSettings(Name, openFileDialog1.FileName);
                if (g_ImageDictionary.ContainsKey(Name)) { g_ImageDictionary[Name] = openFileDialog1.FileName; }
                else { g_ImageDictionary.Add(Name, openFileDialog1.FileName); }
                UpdateScreen(true);
                notificationMenu = new ContextMenu(InitializeMenu());
                notifyIcon.ContextMenu = notificationMenu;
            }
        }

        private void MenuAboutClick(object sender, EventArgs e)
        {
            string Name = this.GetType().Assembly.GetName().Name;

            System.Text.StringBuilder message = new System.Text.StringBuilder();
            //Description
            //
            //Version: XXXX
            //Copyright:XXXXX
            //
            //Others
            var CustomDescriptionAttributes = this.GetType().Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false);
            if (CustomDescriptionAttributes.Length > 0) { message.Append(((System.Reflection.AssemblyDescriptionAttribute)CustomDescriptionAttributes[0]).Description).Append(Environment.NewLine); }
            message.Append(Environment.NewLine);
            message.Append("Version: ").Append(this.GetType().Assembly.GetName().Version.ToString()).Append(Environment.NewLine);
            var CustomInfoCopyrightCall = this.GetType().Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);
            if (CustomInfoCopyrightCall.Length > 0) { message.Append("Copyright: ").Append(((System.Reflection.AssemblyCopyrightAttribute)CustomInfoCopyrightCall[0]).Copyright).Append(Environment.NewLine); }
            message.Append(Environment.NewLine);
            message.Append(ExtractInfo(g_Weather));
            message.Append(ExtractInfo(g_SunRiseSet));
            MessageBox.Show(message.ToString(), Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MenuExitClick(object sender, EventArgs e) { Application.Exit(); }

        private void IconDoubleClick(object sender, EventArgs e) { MessageBox.Show(((Interface.WeatherResponse)(g_Weather).Invoke()).ForcastDescription); }

        private void OnTimedEvent(object sender, EventArgs e) { UpdateScreen(false); }

        private void BlackListHours_Event(object sender, EventArgs e)
        {
            List<int> values = new List<int>();
            for (int i = 0; i < 24; i++)
            {
                if (BlackListHours[i]) { values.Add(i); }
            }


            string ValuesCSV = Microsoft.VisualBasic.Interaction.InputBox("Enter days in comma seperated values, Military time", "Enter Blacklisted Hours", string.Join(",", values.ToArray()));
            values = new List<int>();
            foreach (string item in ValuesCSV.Split(',')) { values.Add(int.Parse(item.Replace(",", string.Empty))); }
            for (int i = 0; i < 24; i++) { BlackListHours[i] = values.Contains(i); }
            SharedObjects.AppSettings.AddUpdateAppSettings("BlackListHours", SharedObjects.ConvertBitarrayToInt(BlackListHours).ToString());
        }

        private void BlackListDays_event(object sender, EventArgs e)
        {
            string ValuesCSV = Microsoft.VisualBasic.Interaction.InputBox("Enter days in comma seperated values, with Sunday = 0 and Saturday = 6, example '0,1,2' = Sunday Monday Tuesday", "Enter Blacklisted Days");
            List<int> values = new List<int>();
            foreach (string item in ValuesCSV.Split(',')) { values.Add(int.Parse(item.Replace(",", string.Empty))); }
            for (int i = 0; i < 7; i++) { BlackListDays[i] = values.Contains(i); }
            SharedObjects.AppSettings.AddUpdateAppSettings("BlackListDays", SharedObjects.ConvertBitarrayToInt(BlackListDays).ToString());
        }

        private void UpdateGlobalObjecttype(object sender, EventArgs e)
        {
            MenuItem Current = (MenuItem)sender;
            string Name = Current.Text;
            if (((MenuItem)Current.Parent).Text == "Weather")
            {
                SharedObjects.AppSettings.AddUpdateAppSettings(cWeather, Name);
                g_Weather = GetWeatherByName(Name);
                g_Weather.Load();
            }
            else if (((MenuItem)Current.Parent).Text == "SunRiseSet")
            {
                SharedObjects.AppSettings.AddUpdateAppSettings(cSRS, Name);
                
                g_SunRiseSet = GetSRSByName(Name);
                g_SunRiseSet.Load();
            }
            notificationMenu = new ContextMenu(InitializeMenu());
            notifyIcon.ContextMenu = notificationMenu;
        }

        private Interface.IsharedSunRiseSetInterface GetSRSByName(string name)
        {
            foreach (var item in SRSObjects)
            {
                if (item.Metadata.ClassName == name) { return item.Value; }
          }
            return null;
        }
        private Interface.ISharedWeatherinterface GetWeatherByName(string name)
        {
            
            foreach (var item in WeatherObjects)
            {
                if (item.Metadata.ClassName == name) { return item.Value; }
            }
            return null;
        }

        #endregion

        #region Private Functions

        private string ExtractInfo(WeatherDesktop.Interface.ISharedInterface Item)
        {
            string Name = Item.GetType().Assembly.GetName().Name + " " + Item.ToString();
            var CustomInfoCopyrightCall = g_Weather.GetType().Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append(Name).Append(Environment.NewLine);
            sb.Append('_',Name.Length).Append(Environment.NewLine);
            if (CustomInfoCopyrightCall.Length > 0) { sb.Append("Copyright: ").Append(((System.Reflection.AssemblyCopyrightAttribute)CustomInfoCopyrightCall[0]).Copyright).Append(Environment.NewLine);}
            sb.Append("Version: ").Append(g_Weather.GetType().Assembly.GetName().Version.ToString()).Append(Environment.NewLine);
            sb.Append("Debug Info: ").Append(Item.Debug()).Append(Environment.NewLine).Append(Environment.NewLine);
            return sb.ToString();
        }

        private void UpdateScreen(Boolean overrideImage)
        {
            if (!(BlackListDays[(int)DateTime.Now.DayOfWeek] || BlackListHours[DateTime.Now.Hour]))
            {
                var weather = (Interface.WeatherResponse)g_Weather.Invoke();
                var sunriseSet = g_SunRiseSet.Invoke();
                string currentTime;

                if (SharedObjects.BetweenTimespans(DateTime.Now.TimeOfDay, ((Interface.SunRiseSetResponse)sunriseSet).SunRise.TimeOfDay, ((Interface.SunRiseSetResponse)sunriseSet).SunSet.TimeOfDay)) { currentTime = cDay; } else { currentTime = cNight; }


                string weatherType = Enum.GetName(typeof(SharedObjects.WeatherTypes), weather.WType);
                notifyIcon.Text = weatherType + " " + weather.Temp.ToString();
                string currentWeatherType = currentTime + weatherType;
                if (string.IsNullOrWhiteSpace(g_CurrentWeatherType) || currentWeatherType != g_CurrentWeatherType || overrideImage)
                {
                    g_CurrentWeatherType = currentWeatherType;
                    notifyIcon.Icon = Share.Wallpaper.GetWeatherIcon(weather.WType, (currentTime == cDay));
                    if (g_ImageDictionary.ContainsKey(currentWeatherType))
                    {
                        try { Share.Wallpaper.Set(g_ImageDictionary[currentWeatherType], Share.Wallpaper.Style.Stretched);

                        }
                        catch (Exception x) { MessageBox.Show(x.ToString()); }
                    }
                }
            }
            if (Themes.RefreshMenu)
            {
                notificationMenu = new ContextMenu(InitializeMenu());
                notifyIcon.ContextMenu = notificationMenu;
            }
        }

        private void DeclareGlobals()
        {
            try
            {

                //try get latlong if you can
                if (!SharedObjects.LatLong.HasRecord())
                {
                    if (LatLongObjects != null)
                    {
                        var i = LatLongObjects.GetEnumerator();
                        while (i.MoveNext())
                        {
                            try
                            {
                                var lat = i.Current.Value;
                                if (lat.worked())
                                {
                                    SharedObjects.LatLong.Set(lat.Latitude(), lat.Longitude());
                                    break;
                                }
                            }
                            catch { }

                        }
                    }
                }


                //get weather type
                string weatherType = SharedObjects.AppSettings.ReadSetting(cWeather);
                if (!string.IsNullOrWhiteSpace(weatherType)) { GetWeatherByName(weatherType); }
                if (g_Weather == null)
                {
                    var I = WeatherObjects.GetEnumerator();
                    while (I.MoveNext())
                    {
                        var current = I.Current;
                        if (!current.Metadata.ClassName.StartsWith("Mock"))
                        {
                            g_Weather = current.Value;
                            break;
                        }

                    }
                }
                if (g_Weather == null) { g_Weather = GetWeatherByName("Mock_Weather"); }
                g_Weather.Load();

                //get SRS
                string srs = SharedObjects.AppSettings.ReadSetting(cSRS);


                if (!string.IsNullOrWhiteSpace(srs)) { g_SunRiseSet = GetSRSByName(srs); }
                if (g_SunRiseSet == null)
                {
                    var i = SRSObjects.GetEnumerator();
                    while (i.MoveNext())
                    {
                        var current = i.Current;
                        if (!current.Metadata.ClassName.StartsWith("Mock"))
                        {
                            g_SunRiseSet = current.Value; break;
                        }
                    }
                }
                if (g_SunRiseSet == null) { GetSRSByName("Mock_SunRiseSet"); }
                g_SunRiseSet.Load();
            }
            catch (Exception x)
            {
                Share.ErrorHandler.Send(x);
            }

            UpdateImageCache();
            UpdateBlackLists();

        }

        #region Support functions to reduce complexity

         private void UpdateBlackLists()
        {
            int.TryParse(SharedObjects.AppSettings.ReadSetting("BlackListHours"), out int intblackListHours);
            int.TryParse(SharedObjects.AppSettings.ReadSetting("BlackListDays"), out int intBlackListdays);
            BlackListHours = SharedObjects.ConverTIntToBitArray(intblackListHours);
            BlackListDays = SharedObjects.ConverTIntToBitArray(intBlackListdays);

        }

        private void CreateTimer()
        {
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer
            {
                Interval = 1000 // specify interval time as you want
            };
            t.Tick += new EventHandler(OnTimedEvent);
            t.Start();
        }

        private void UpdateImageCache()
        {
            foreach (var element in System.Enum.GetValues(typeof(SharedObjects.WeatherTypes)))
            {
                string ElementName = Enum.GetName(typeof(SharedObjects.WeatherTypes), element);
                string daykey = cDay + ElementName;
                string nightKey = cNight + ElementName;
                string dayimageCache = SharedObjects.AppSettings.ReadSetting(cDay + ElementName);
                string nightimagecache = SharedObjects.AppSettings.ReadSetting(cNight + ElementName);
                if (!string.IsNullOrEmpty(dayimageCache)) { g_ImageDictionary.Add(daykey, dayimageCache); }
                if (!string.IsNullOrEmpty(nightimagecache)) { g_ImageDictionary.Add(nightKey, nightimagecache); }
            }
        }

        private void LazyLoader()
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(WeatherDesktop.MainApp).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(Environment.CurrentDirectory));

            if (System.IO.Directory.Exists(PluginPaths)) {
                catalog.Catalogs.Add(new DirectoryCatalog(PluginPaths));
                foreach (var item in System.IO.Directory.EnumerateDirectories(PluginPaths))
                {
                    catalog.Catalogs.Add(new DirectoryCatalog(item));
                }
            }
            //Create the CompositionContainer with the parts in the catalog
            _container = new CompositionContainer(catalog);

            //Fill the imports of this object
            try
            {
                this._container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Share.ErrorHandler.Send(compositionException);
                // Console.WriteLine(compositionException.ToString());
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing){notifyIcon.Dispose(); notificationMenu.Dispose(); _container.Dispose();}

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose(){Dispose(true);}
        #endregion

        #endregion

        #endregion

    }
}
