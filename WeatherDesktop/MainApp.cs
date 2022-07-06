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
using WeatherDesktop.Shared.Handlers;

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
                new MenuItem(Properties.Menu.Settings, GetSettings().ToArray()),
                new MenuItem(Properties.Menu.About, MenuAboutClick),
                new MenuItem(Properties.Menu.Exit, MenuExitClick)
            };

            return menu;
        }

        private IEnumerable<MenuItem> GetSettings()
        {
            yield return new MenuItem(Properties.Menu.Global, GlobalMenuSettings());
            yield return new MenuItem(Properties.Menu.Images, GetWeatherMenuItems().ToArray());
            yield return new MenuItem(g_SunRiseSet.GetType().Name, g_SunRiseSet.SettingsItems());
            yield return new MenuItem(g_Weather.GetType().Name, g_Weather.SettingsItems());
            yield return new MenuItem(Properties.Menu.Themes, Themes.SettingsItems());
        }

        private MenuItem[] GlobalMenuSettings()
        {
            var SelectedItem = AppSetttingsHandler.Weather;
            var SelectedSRS = AppSetttingsHandler.SunRiseSet;

            return new List<MenuItem>
            {
                new MenuItem(Properties.Menu.PluginFolder, PluginFolder_Event),
                new MenuItem(Properties.Menu.DeniedItems,new MenuItem[]
                {
                    new MenuItem(Properties.Menu.DeniedHours, DenyedtHours_Event),
                    new MenuItem(Properties.Menu.DeniedDays, DenyedDays_event)
                } 
                ),
                new MenuItem(Properties.Menu.Weather, 
                WeatherObjects.Select(x => 
                new MenuItem(x.Metadata.ClassName, UpdateGlobalObjecttype)
                {Checked = x.Metadata.ClassName == SelectedItem}
                ).ToArray()),
                new MenuItem(Properties.Menu.SunRiseSet,
                SRSObjects.Select(x => 
                new MenuItem(x.Metadata.ClassName, UpdateGlobalObjecttype)
                { Checked = x.Metadata.ClassName == SelectedSRS}).ToArray())
            }.ToArray();
        }
        private IEnumerable<MenuItem> GetWeatherMenuItems()
        {
            foreach (var element in Enum.GetValues(typeof(SharedObjects.WeatherTypes)))
            {
                var ElementName = Enum.GetName(typeof(SharedObjects.WeatherTypes), element);
                
                var DayName = cDay + ElementName;
                yield return new MenuItem(DayName, MenuItemClick)
                { 
                Checked = g_ImageDictionary.ContainsKey(DayName)
                };

                var NightName = cNight + ElementName;
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
                        var notificationIcon = new MainApp();
                        notificationIcon.notifyIcon.Visible = true;
                        GC.Collect();
                        Application.Run();
                        notificationIcon.notifyIcon.Dispose();
                    }
                    catch (Exception x)
                    {
                        MessageBox.Show("Error: " + x.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        ErrorHandler.Send(x);
                    }
                    mtx.ReleaseMutex();
                }
                else
                {
                    GC.Collect();
                    MessageBox.Show(Properties.Warnings.AppAlreadyRunning);
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

            var Name = ((MenuItem)sender).Text;//.Replace("*", string.Empty);
            var openFileDialog1 = new OpenFileDialog
            {
                Filter = "jpeg (*.jpg)|*.jpg|Portable Network Graphics (*.png)|*.png",
                FilterIndex = 1,
                RestoreDirectory = true,
                Title = Name
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                AppSetttingsHandler.Write(Name, openFileDialog1.FileName);
                if (g_ImageDictionary.ContainsKey(Name)) { g_ImageDictionary[Name] = openFileDialog1.FileName; }
                else { g_ImageDictionary.Add(Name, openFileDialog1.FileName); }
                UpdateScreen(true);
                notificationMenu = new ContextMenu(InitializeMenu());
                notifyIcon.ContextMenu = notificationMenu;
            }
        }

        private void MenuAboutClick(object sender, EventArgs e)
        {
            var Name = this.GetType().Assembly.GetName().Name;

            var message = new System.Text.StringBuilder();
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
            var values = new List<int>();
            for (int i = 0; i < 24; i++)
            {
                if (DenyedHours[i]) { values.Add(i); }
            }


            var ValuesCSV = InputHandler.InputBox(Properties.Prompts.CSVForHours, Properties.Titles.EnterHours, string.Join(",", values.ToArray()));
            values = new List<int>();
            foreach (string item in ValuesCSV.Split(',')) { values.Add(int.Parse(item.Replace(",", string.Empty))); }
            for (int i = 0; i < 24; i++) { DenyedHours[i] = values.Contains(i); }
            AppSetttingsHandler.DeniedHours = DenyedHours.ToInt().ToString();
        }

        private void DenyedDays_event(object sender, EventArgs e)
        {
            var ValuesCSV = InputHandler.InputBox(Properties.Prompts.CSVForDays, Properties.Titles.EnterDays);
            var values = new List<int>();
            foreach (string item in ValuesCSV.Split(',')) { values.Add(int.Parse(item.Replace(",", string.Empty))); }
            for (int i = 0; i < 7; i++) { DenyedDays[i] = values.Contains(i); }
            AppSetttingsHandler.DeniedDays = DenyedDays.ToInt().ToString();
        }

        private void UpdateGlobalObjecttype(object sender, EventArgs e)
        {
            var Current = (MenuItem)sender;
            var Name = Current.Text;
            if (((MenuItem)Current.Parent).Text == "Weather")
            {
                AppSetttingsHandler.Weather = Name;
                g_Weather = GetByName(WeatherObjects, Name);
                g_Weather.Load();
            }
            else if (((MenuItem)Current.Parent).Text == "SunRiseSet")
            {
                AppSetttingsHandler.SunRiseSet = Name;
                
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
            var Name = Item.GetType().Assembly.GetName().Name + " " + Item.ToString();
            var CustomInfoCopyrightCall = g_Weather.GetType().Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);
            var sb = new System.Text.StringBuilder();
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
                
                var currentTime = DateTime.Now.TimeOfDay.Between(sunriseSet.SunRise.TimeOfDay, sunriseSet.SunSet.TimeOfDay) ? cDay : cNight;
                

                var weatherType = Enum.GetName(typeof(SharedObjects.WeatherTypes), weather.WType);
                notifyIcon.Text = weatherType + " " + weather.Temp.ToString();
                var currentWeatherType = currentTime + weatherType;
                if (string.IsNullOrWhiteSpace(g_CurrentWeatherType) || currentWeatherType != g_CurrentWeatherType || overrideImage)
                {
                    g_CurrentWeatherType = currentWeatherType;
                    notifyIcon.Icon = Share.Wallpaper.GetWeatherIcon(weather.WType, (currentTime == cDay));
                    if (g_ImageDictionary.ContainsKey(currentWeatherType))
                    {
                        try { Wallpaper.Set(g_ImageDictionary[currentWeatherType], Wallpaper.Style.Stretched);

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
                if (!LatLongHandler.HasRecord())
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
                                    LatLongHandler.Set(lat.Latitude(), lat.Longitude()); break;
                                }
                            }
                            catch (Exception x){ ErrorHandler.Send(x); }
                        }
                    }
                }

                //get weather type
                var weatherType = AppSetttingsHandler.Weather;

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
                var srs = AppSetttingsHandler.SunRiseSet;
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
            int.TryParse(AppSetttingsHandler.DeniedHours, out int intDenyedHours);
            int.TryParse(AppSetttingsHandler.DeniedDays, out int intDenyedDays);
            DenyedHours = intDenyedHours.ToBitArray();
            DenyedDays = intDenyedDays.ToBitArray();

        }

        private void CreateTimer()
        {
            var t = new System.Windows.Forms.Timer
            {
                Interval = 3000 // specify interval time as you want
            };
            t.Tick += new EventHandler(OnTimedEvent);
            t.Start();
        }

        private void UpdateImageCache()
        {
            foreach (var element in Enum.GetValues(typeof(SharedObjects.WeatherTypes)))
            {
                var ElementName = Enum.GetName(typeof(SharedObjects.WeatherTypes), element);
                var daykey = cDay + ElementName;
                var nightKey = cNight + ElementName;
                var dayimageCache = AppSetttingsHandler.Read(cDay + ElementName);
                var nightimagecache = AppSetttingsHandler.Read(cNight + ElementName);
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
                ErrorHandler.Send(compositionException);
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
