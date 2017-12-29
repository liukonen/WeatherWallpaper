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
        private Interface.ISharedWeatherinterface GlobalWeather;
        private Interface.IsharedSunRiseSetInterface GlobalSRS;
        private Share.ThemeHandler Themes = new Share.ThemeHandler();
        private Dictionary<string, string> GlobalImageDirectory = new Dictionary<string, string>();
        private string GlobalCurrentWeatherType;
        private CompositionContainer _container;

        System.Collections.BitArray BlackListHours = new System.Collections.BitArray(24);
        System.Collections.BitArray BlackListDays = new System.Collections.BitArray(7);
        string PluginPaths = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + "Plugins";
        private Shared.Translation Translate = new Shared.Translation();
        #endregion

        #region Initialize icon and menu
        /// <summary>
        /// VB terms, Public Sub New
        /// </summary>
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

        #endregion
        
        #region Menu Items

        private MenuItem[] InitializeMenu()
        {
            return new MenuItem[] {
                new MenuItem(Translate.Settings, GetSettings()),
                new MenuItem(Translate.About, MenuAboutClick),
                new MenuItem(Translate.Exit, MenuExitClick)
            };
        }

        private MenuItem[] GetSettings()
        {
            return new MenuItem[]
            {
                new MenuItem(Translate.Global, GlobalMenuSettings()),
                new MenuItem(Translate.Images, GetWeatherMenuItems()),
                new MenuItem(GlobalSRS.GetType().Name, GlobalSRS.SettingsItems()),
                new MenuItem(GlobalWeather.GetType().Name, GlobalWeather.SettingsItems()),
                new MenuItem(Translate.Themes, Themes.SettingsItems())
            };
        }

        private MenuItem[] GlobalMenuSettings()
        {
            List<MenuItem> Items = new List<MenuItem>{new MenuItem(Translate.PluginFolder, PluginFolder_Event)};
            List<MenuItem> BlackLists = new List<MenuItem> {new MenuItem(Translate.BlackListHours, BlackListHours_Event), new MenuItem(Translate.BlackListDays, BlackListDays_event)};
            Items.Add(new MenuItem(Translate.BlackList, BlackLists.ToArray()));
            
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
            Items.Add(new MenuItem(Translate.Weather, WeatherItems.ToArray()));
            Items.Add(new MenuItem(Translate.SunRiseSet, SunRiseSetItems.ToArray()));
            Items.Add(new MenuItem(Translate.Language, LanguagesMenuItems()));
            return Items.ToArray();

        }

        private MenuItem[] LanguagesMenuItems()
        {
            List<MenuItem> Items = new List<MenuItem>();
            foreach (string key in QuickTranslation.SupportedLanguages.Keys)
            {
                Items.Add(new MenuItem(key, MenuChangeLanguage));
            }
            return Items.ToArray();
        }

        private MenuItem[] GetWeatherMenuItems()
        {
            List<MenuItem> items = new List<MenuItem>();
            foreach (var element in System.Enum.GetValues(typeof(SharedObjects.WeatherTypes)))
            {
                string ElementName =  Enum.GetName(typeof(SharedObjects.WeatherTypes), element);
                string DayName = cDay + ElementName;
                string NightName = cNight + ElementName;

                string TranslatedName = QuickTranslation.TranslatedWeatherType((SharedObjects.WeatherTypes)element);

                MenuItem dayItem = new MenuItem(Translate.Day + "-" + TranslatedName, MenuItemClick);
                MenuItem Nightitem = new MenuItem(Translate.Night + "-" + TranslatedName, MenuItemClick);

                dayItem.Checked = (GlobalImageDirectory.ContainsKey(DayName));
                Nightitem.Checked = (GlobalImageDirectory.ContainsKey(NightName));
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

        /// <summary>
        /// Handles Event of Menu / Settings / GlobalMenuSettings / Plugin Folder
        /// </summary>
        /// <param name="sender">Plugin Folder MenuItem</param>
        /// <param name="e"></param>
        private void PluginFolder_Event(object sender, EventArgs e)
        {
            if (System.IO.Directory.Exists(PluginPaths)) {System.Diagnostics.Process.Start(PluginPaths); }
        }

        /// <summary>
        /// Handles Events for Menu / Images / * (Both Night and Day images)
        /// </summary>
        /// <param name="sender">Image Menu Item</param>
        /// <param name="e"></param>
        private void MenuItemClick(object sender, EventArgs e)
        {

            string Name = ((MenuItem)sender).Text;
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Filter = "jpeg (*.jpg)|*.jpg|Portable Network Graphics (*.png)|*.png",
                FilterIndex = 1,
                RestoreDirectory = true,
                Title = Name
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string[] NameParse = Name.Split('-');
                string DN = NameParse[0].Replace("-", string.Empty);
                string WT = NameParse[1].Replace("-", string.Empty);
                string TranslatedName = QuickTranslation.ReverseTranlateDayNight(DN) + "-" + QuickTranslation.ReverseTranslateWeather(WT);
                SharedObjects.AppSettings.AddUpdateAppSettings(TranslatedName, openFileDialog1.FileName);
                if (GlobalImageDirectory.ContainsKey(TranslatedName)) { GlobalImageDirectory[TranslatedName] = openFileDialog1.FileName; }
                else { GlobalImageDirectory.Add(TranslatedName, openFileDialog1.FileName); }
                UpdateScreen(true);
                notificationMenu = new ContextMenu(InitializeMenu());
                notifyIcon.ContextMenu = notificationMenu;
            }
        }

        /// <summary>
        /// Handles Event for Menu / About
        /// </summary>
        /// <param name="sender">About Menu Item</param>
        /// <param name="e"></param>
        private void MenuAboutClick(object sender, EventArgs e)
        {
            string Name = this.GetType().Assembly.GetName().Name;

            System.Text.StringBuilder message = new System.Text.StringBuilder();
            var CustomDescriptionAttributes = this.GetType().Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyDescriptionAttribute), false);
            if (CustomDescriptionAttributes.Length > 0) { message.Append(((System.Reflection.AssemblyDescriptionAttribute)CustomDescriptionAttributes[0]).Description).Append(Environment.NewLine); }
            message.Append(Environment.NewLine);
            message.Append(Translate.Version).Append(": ").Append(this.GetType().Assembly.GetName().Version.ToString()).Append(Environment.NewLine);
            var CustomInfoCopyrightCall = this.GetType().Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);
            if (CustomInfoCopyrightCall.Length > 0) { message.Append(Translate.Copyright).Append(": ").Append(((System.Reflection.AssemblyCopyrightAttribute)CustomInfoCopyrightCall[0]).Copyright).Append(Environment.NewLine); }
            message.Append(Environment.NewLine);
            message.Append(ExtractInfo(GlobalWeather));
            message.Append(ExtractInfo(GlobalSRS));
            MessageBox.Show(message.ToString(), Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Handles Event of Menu / Exit
        /// </summary>
        /// <param name="sender">Exit Menu Item</param>
        /// <param name="e"></param>
        private void MenuExitClick(object sender, EventArgs e) { Application.Exit(); }

        /// <summary>
        /// Handles Event of Menu - Double Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IconDoubleClick(object sender, EventArgs e) { MessageBox.Show(((Interface.WeatherResponse)(GlobalWeather).Invoke()).ForcastDescription); }

        /// <summary>
        /// Handles Event of t (timer) .Tick Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimedEvent(object sender, EventArgs e) { UpdateScreen(false); }

        /// <summary>
        /// Handles Event of Menu / Settings / GlobalMenutSettings / BlacklistHours Click
        /// </summary>
        /// <param name="sender">BlackListHours Menu Item</param>
        /// <param name="e"></param>
        private void BlackListHours_Event(object sender, EventArgs e)
        {
            List<int> values = new List<int>();
            for (int i = 0; i < 24; i++)
            {
                if (BlackListHours[i]) { values.Add(i); }
            }


            string ValuesCSV = Microsoft.VisualBasic.Interaction.InputBox(Translate.BlackListHoursInputMessage, Translate.BlackListHoursHeader, string.Join(",", values.ToArray()));
            values = new List<int>();
            foreach (string item in ValuesCSV.Split(',')) { values.Add(int.Parse(item.Replace(",", string.Empty))); }
            for (int i = 0; i < 24; i++) { BlackListHours[i] = values.Contains(i); }
            SharedObjects.AppSettings.AddUpdateAppSettings("BlackListHours", SharedObjects.ConvertBitarrayToInt(BlackListHours).ToString());
        }

        private void BlackListDays_event(object sender, EventArgs e)
        {
            string ValuesCSV = Microsoft.VisualBasic.Interaction.InputBox(Translate.BlackListDaysInputMessage, Translate.BlackListDaysHeader);
            List<int> values = new List<int>();
            foreach (string item in ValuesCSV.Split(',')) { values.Add(int.Parse(item.Replace(",", string.Empty))); }
            for (int i = 0; i < 7; i++) { BlackListDays[i] = values.Contains(i); }
            SharedObjects.AppSettings.AddUpdateAppSettings("BlackListDays", SharedObjects.ConvertBitarrayToInt(BlackListDays).ToString());
        }

        private void UpdateGlobalObjecttype(object sender, EventArgs e)
        {
            MenuItem Current = (MenuItem)sender;
            string Name = Current.Text;
            if (((MenuItem)Current.Parent).Text == Translate.Weather)
            {
                SharedObjects.AppSettings.AddUpdateAppSettings(cWeather, Name);
                GlobalWeather = GetWeatherByName(Name);
                GlobalWeather.Load();
            }
            else if (((MenuItem)Current.Parent).Text == Translate.SunRiseSet)
            {
                SharedObjects.AppSettings.AddUpdateAppSettings(cSRS, Name);
                
                GlobalSRS = GetSRSByName(Name);
                GlobalSRS.Load();
            }
            notificationMenu = new ContextMenu(InitializeMenu());
            notifyIcon.ContextMenu = notificationMenu;
        }
        
        private void MenuChangeLanguage(object sender, EventArgs e)
        {
            string Name = ((MenuItem)sender).Text;
            string twoCharCode = QuickTranslation.SupportedLanguages[Name];
            QuickTranslation.ChangeLanguage(twoCharCode);
            UpdateScreen(true);
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
            var CustomInfoCopyrightCall = GlobalWeather.GetType().Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyCopyrightAttribute), false);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append(Name).Append(Environment.NewLine);
            sb.Append('_',Name.Length).Append(Environment.NewLine);
            if (CustomInfoCopyrightCall.Length > 0) { sb.Append(Translate.Copyright).Append(": ").Append(((System.Reflection.AssemblyCopyrightAttribute)CustomInfoCopyrightCall[0]).Copyright).Append(Environment.NewLine);}
            sb.Append(Translate.Version).Append(": ").Append(GlobalWeather.GetType().Assembly.GetName().Version.ToString()).Append(Environment.NewLine);
            sb.Append(Translate.DebugInfo).Append(": ").Append(Item.Debug()).Append(Environment.NewLine).Append(Environment.NewLine);
            return sb.ToString();
        }

        private void UpdateScreen(Boolean overrideImage)
        {
            if (!(BlackListDays[(int)DateTime.Now.DayOfWeek] || BlackListHours[DateTime.Now.Hour]))
            {
                var weather = (Interface.WeatherResponse)GlobalWeather.Invoke();
                var sunriseSet = GlobalSRS.Invoke();
                string currentTime;

                if (SharedObjects.BetweenTimespans(DateTime.Now.TimeOfDay, ((Interface.SunRiseSetResponse)sunriseSet).SunRise.TimeOfDay, ((Interface.SunRiseSetResponse)sunriseSet).SunSet.TimeOfDay)) { currentTime = cDay; } else { currentTime = cNight; }


                string weatherType = Enum.GetName(typeof(SharedObjects.WeatherTypes), weather.WType);
                notifyIcon.Text = QuickTranslation.TranslatedWeatherType(weather.WType) + " " + weather.Temp.ToString();
                string currentWeatherType = currentTime + weatherType;
                if (string.IsNullOrWhiteSpace(GlobalCurrentWeatherType) || currentWeatherType != GlobalCurrentWeatherType || overrideImage)
                {
                    GlobalCurrentWeatherType = currentWeatherType;
                    notifyIcon.Icon = Share.Wallpaper.GetWeatherIcon(weather.WType, (currentTime == cDay));
                    if (GlobalImageDirectory.ContainsKey(currentWeatherType))
                    {
                        try { Share.Wallpaper.Set(GlobalImageDirectory[currentWeatherType], Share.Wallpaper.Style.Stretched);

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
               //get weather type
                string weatherType = SharedObjects.AppSettings.ReadSetting(cWeather);
                if (!string.IsNullOrWhiteSpace(weatherType)) { GetWeatherByName(weatherType); }
                if (GlobalWeather == null)
                {
                    var I = WeatherObjects.GetEnumerator();
                    while (I.MoveNext())
                    {
                        var current = I.Current;
                        if (!current.Metadata.ClassName.StartsWith("Mock"))
                        {
                            GlobalWeather = current.Value;
                            break;
                        }

                    }
                }
                if (GlobalWeather == null) { GlobalWeather = GetWeatherByName("Mock_Weather"); }
                GlobalWeather.Load();

                //try get latlong if you can
                if (!SharedObjects.LatLong.HasRecord())
                {
                    if (LatLongObjects != null) { 
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

                //get SRS
                string srs = SharedObjects.AppSettings.ReadSetting(cSRS);


                if (!string.IsNullOrWhiteSpace(srs)) { GlobalSRS = GetSRSByName(srs); }
                if (GlobalSRS == null)
                {
                    var i = SRSObjects.GetEnumerator();
                    while (i.MoveNext())
                    {
                        var current = i.Current;
                        if (!current.Metadata.ClassName.StartsWith("Mock"))
                        {
                            GlobalSRS = current.Value; break;
                        }
                    }
                }
                if (GlobalSRS == null) { GetSRSByName("Mock_SunRiseSet"); }
                GlobalSRS.Load();
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
                if (!string.IsNullOrEmpty(dayimageCache)) { GlobalImageDirectory.Add(daykey, dayimageCache); }
                if (!string.IsNullOrEmpty(nightimagecache)) { GlobalImageDirectory.Add(nightKey, nightimagecache); }
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
