using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using WeatherDesktop.Share;
using System.Linq;
using WeatherDesktop.Shared.Extentions;

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
        private ThemeHandler Themes = new ThemeHandler();
        private Dictionary<string, string> g_ImageDictionary = new Dictionary<string, string>();
        private string g_CurrentWeatherType;
        private CompositionContainer _container;

        System.Collections.BitArray DenyedHours = new System.Collections.BitArray(24);
        System.Collections.BitArray DenyedDays = new System.Collections.BitArray(7);
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
            //ComponentResourceManager resources = new ComponentResourceManager(typeof(MainApp));
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.ContextMenu = notificationMenu;
            UpdateScreen(false);
            CreateTimer();
        }

        private MenuItem[] InitializeMenu()
        {


            MenuItem[] menu = new MenuItem[] {
                new MenuItem("Settings", GetSettings().ToArray()),
                new MenuItem("About", MenuAboutClick),
                new MenuItem("Exit", MenuExitClick)
            };

            return menu;
        }

        private IEnumerable<MenuItem> GetSettings()
        {
            yield return new MenuItem("Global", GlobalMenuSettings());
            yield return new MenuItem("images", GetWeatherMenuItems().ToArray());
            yield return new MenuItem(g_SunRiseSet.GetType().Name, g_SunRiseSet.SettingsItems());
            yield return new MenuItem(g_Weather.GetType().Name, g_Weather.SettingsItems());
            yield return new MenuItem("Themes", Themes.SettingsItems());
        }

        private MenuItem[] GlobalMenuSettings()
        {
            List<MenuItem> Items = new List<MenuItem>{new MenuItem("Plugin Folder", PluginFolder_Event)};
            List<MenuItem> DenyedList = new List<MenuItem> {new MenuItem("Denyed Hours", DenyedtHours_Event), new MenuItem("Denyed Days", DenyedDays_event)};
            Items.Add(new MenuItem("DenyedListing", DenyedList.ToArray()));
            
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
        private IEnumerable<MenuItem> GetWeatherMenuItems()
        {
            foreach (var element in Enum.GetValues(typeof(SharedObjects.WeatherTypes)))
            {
                string ElementName = Enum.GetName(typeof(SharedObjects.WeatherTypes), element);
                
                string DayName = cDay + ElementName;
                yield return new MenuItem(DayName, MenuItemClick)
                { 
                Checked = g_ImageDictionary.ContainsKey(DayName)
                };

                string NightName = cNight + ElementName;
                yield return new MenuItem(NightName, MenuItemClick)
                {
                    Checked = (g_ImageDictionary.ContainsKey(NightName))
                };
            }
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

            // Please use a unique name for the mutex to prevent conflicts with other programs

            using (Mutex mtx = new Mutex(true, "WeatherDesktop", out bool isFirstInstance))
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
                    catch (Exception x)
                    {
                        MessageBox.Show("Error: " + x.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void DenyedtHours_Event(object sender, EventArgs e)
        {
            List<int> values = new List<int>();
            for (int i = 0; i < 24; i++)
            {
                if (DenyedHours[i]) { values.Add(i); }
            }


            string ValuesCSV = Microsoft.VisualBasic.Interaction.InputBox("Enter days in comma seperated values, Military time", "Enter Denyed Hours", string.Join(",", values.ToArray()));
            values = new List<int>();
            foreach (string item in ValuesCSV.Split(',')) { values.Add(int.Parse(item.Replace(",", string.Empty))); }
            for (int i = 0; i < 24; i++) { DenyedHours[i] = values.Contains(i); }
            SharedObjects.AppSettings.AddUpdateAppSettings("DenyedHours", SharedObjects.ConvertBitarrayToInt(DenyedHours).ToString());
        }

        private void DenyedDays_event(object sender, EventArgs e)
        {
            string ValuesCSV = Microsoft.VisualBasic.Interaction.InputBox("Enter days in comma seperated values, with Sunday = 0 and Saturday = 6, example '0,1,2' = Sunday Monday Tuesday", "Enter Denyed Days");
            List<int> values = new List<int>();
            foreach (string item in ValuesCSV.Split(',')) { values.Add(int.Parse(item.Replace(",", string.Empty))); }
            for (int i = 0; i < 7; i++) { DenyedDays[i] = values.Contains(i); }
            SharedObjects.AppSettings.AddUpdateAppSettings("DenyedDays", SharedObjects.ConvertBitarrayToInt(DenyedDays).ToString());
        }

        private void UpdateGlobalObjecttype(object sender, EventArgs e)
        {
            MenuItem Current = (MenuItem)sender;
            string Name = Current.Text;
            if (((MenuItem)Current.Parent).Text == "Weather")
            {
                SharedObjects.AppSettings.AddUpdateAppSettings(cWeather, Name);
                g_Weather = GetByName(WeatherObjects, Name);
                g_Weather.Load();
            }
            else if (((MenuItem)Current.Parent).Text == "SunRiseSet")
            {
                SharedObjects.AppSettings.AddUpdateAppSettings(cSRS, Name);
                
                g_SunRiseSet = GetByName(SRSObjects, Name);
                g_SunRiseSet.Load();
            }
            notificationMenu = new ContextMenu(InitializeMenu());
            notifyIcon.ContextMenu = notificationMenu;
        }

        private static T GetByName<T>(IEnumerable<Lazy<T, Interface.IClassName>> collection, string name) 
            => collection.Where(x => x.Metadata.ClassName == name).Select(x => x.Value).FirstOrDefault();
        



        #endregion

        #region Private Functions

        private string ExtractInfo(Interface.ISharedInterface Item)
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
            if (!(DenyedDays[(int)DateTime.Now.DayOfWeek] || DenyedHours[DateTime.Now.Hour]))
            {
                var weather = (Interface.WeatherResponse)g_Weather.Invoke();
                var sunriseSet = (Interface.SunRiseSetResponse)g_SunRiseSet.Invoke();
                string currentTime;

                currentTime = DateTime.Now.TimeOfDay.Between(sunriseSet.SunRise.TimeOfDay, sunriseSet.SunSet.TimeOfDay) ? cDay : cNight;
                

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

                        var ordered = LatLongObjects.OrderBy(d => {
                            return (d.Metadata.ClassName == "OpenDataFlatFileLookup") ? 1 : 2;
                        });
                        foreach (var obj in ordered)
                        {
                            try
                            {
                                var lat = obj.Value;
                                if (lat.worked())
                                {
                                    SharedObjects.LatLong.Set(lat.Latitude(), lat.Longitude()); break;
                                }
                            }
                            catch (Exception x){ ErrorHandler.Send(x); }
                        }
                    }
                }

                //get weather type
                string weatherType = SharedObjects.AppSettings.ReadSetting(cWeather);

                foreach (var item in PullByPrefered(WeatherObjects, weatherType, "GovWeather3"))
                {
                    try
                    {
                        g_Weather = item;
                        g_Weather.Load();
                        break;
                    }
                    catch (Exception x){ ErrorHandler.Send(x); }
                }


                //get SRS
                string srs = SharedObjects.AppSettings.ReadSetting(cSRS);
                foreach (var item in PullByPrefered(SRSObjects, srs, "InternalSunRiseSet"))
                {
                    try
                    {
                        g_SunRiseSet = item;
                        g_SunRiseSet.Load();
                        break;
                    }
                    catch { }
                }
            }
            catch (Exception x)
            {
                ErrorHandler.Send(x);
            }

            UpdateImageCache();
            UpdateDenyedList();
        }

        private IEnumerable<T> PullByPrefered<T>(IEnumerable<Lazy<T, Interface.IClassName>> collection, string prefered,string secondary)
        { 
            T response = GetByName(collection, prefered);
            if (response == null)
            {

                var fixedOrder = new List<string> { secondary };

                var ordered = collection.OrderBy(d => {
                    var index = fixedOrder.IndexOf(d.Metadata.ClassName);
                    if (d.Metadata.ClassName.StartsWith("Mock")) return int.MaxValue;
                    return (index == -1) ? int.MaxValue - 1: index;
                });
                var TT = ordered.ToArray();
                foreach (var item in ordered)
                {
                    yield return item.Value;
                    
                }           
            }
        }


        #region Support functions to reduce complexity

        private void UpdateDenyedList()
        {
            int.TryParse(SharedObjects.AppSettings.ReadSetting("DenyedHours"), out int intDenyedHours);
            int.TryParse(SharedObjects.AppSettings.ReadSetting("DenyedDays"), out int intDenyedDays);
            DenyedHours = SharedObjects.ConverTIntToBitArray(intDenyedHours);
            DenyedDays = SharedObjects.ConverTIntToBitArray(intDenyedDays);

        }

        private void CreateTimer()
        {
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer
            {
                Interval = 30000 // specify interval time as you want
            };
            t.Tick += new EventHandler(OnTimedEvent);
            t.Start();
        }

        private void UpdateImageCache()
        {
            foreach (var element in System.Enum.GetValues(typeof(SharedObjects.WeatherTypes)))
            {
                var ElementName = Enum.GetName(typeof(SharedObjects.WeatherTypes), element);
                var daykey = cDay + ElementName;
                var nightKey = cNight + ElementName;
                var dayimageCache = SharedObjects.AppSettings.ReadSetting(cDay + ElementName);
                var nightimagecache = SharedObjects.AppSettings.ReadSetting(cNight + ElementName);
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
                disposedValue = true;
            }
        }

        void IDisposable.Dispose() => Dispose(true);
        #endregion

        #endregion

        #endregion

    }
}
