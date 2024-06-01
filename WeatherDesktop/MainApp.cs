using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using WeatherDesktop.Share;
using System.Linq;
using WeatherDesktop.Shared.Extentions;
using WeatherDesktop.Shared.Handlers;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Text;

namespace WeatherDesktop
{
    public sealed class MainApp : IDisposable
    {
        #region Lazy Load objects
#pragma warning disable 0649
        [ImportMany]
        readonly IEnumerable<Lazy<Interface.ISharedWeatherinterface, Interface.IClassName>> WeatherObjects;

        [ImportMany]
        readonly IEnumerable<Lazy<Interface.IsharedSunRiseSetInterface, Interface.IClassName>> SRSObjects;

        [ImportMany]
        readonly IEnumerable<Lazy<Interface.ILatLongInterface, Interface.IClassName>> LatLongObjects;
#pragma warning restore 0649
        #endregion

        #region constants
        const string cDay = "day-";
        const string cNight = "night-";
        #endregion

        #region global Objects
        private readonly NotifyIcon notifyIcon;
        private Interface.ISharedWeatherinterface g_Weather;
        private Interface.IsharedSunRiseSetInterface g_SunRiseSet;
        private readonly ThemeHandler Themes = new ThemeHandler();
        private Dictionary<string, string> g_ImageDictionary = new Dictionary<string, string>();
        private string g_CurrentWeatherType;
        private CompositionContainer _container; 
        BitArray DeniedHours = new BitArray(24);
        BitArray DeniedDays = new BitArray(7);
        readonly string PluginPaths = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Plugins";

        #endregion

        #region Initialize icon and menu
        public MainApp()
        {
            LazyLoader();
            DeclareGlobals();
            notifyIcon = new NotifyIcon();
            notifyIcon.DoubleClick += IconDoubleClick;
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.ContextMenu = new ContextMenu(InitializeMenu());
            UpdateScreen(false);
            CreateTimer();
        }

        private MenuItem[] InitializeMenu() 
            => new MenuItem[] {
                new MenuItem(Properties.Menu.Settings, GetSettings()),
                new MenuItem(Properties.Menu.About, MenuAboutClick),
                new MenuItem(Properties.Menu.Exit, MenuExitClick)
            };

        private MenuItem[] GetSettings() => new MenuItem[] {
             new MenuItem(Properties.Menu.Global, GlobalMenuSettings()),
             new MenuItem(Properties.Menu.Images, GetWeatherMenuItems().ToArray()),
             new MenuItem(Properties.Menu.IconStyle, ThemeOptions()),
             new MenuItem(g_SunRiseSet.GetType().Name, g_SunRiseSet.SettingsItems()),
             new MenuItem(g_Weather.GetType().Name, g_Weather.SettingsItems()),
             new MenuItem(Properties.Menu.Themes, Themes.SettingsItems())
        };

        public MenuItem[] ThemeOptions()
            => Enum.GetNames(typeof(IconThemeStyle))
            .Select(x => new MenuItem(x, IconEventHandler) 
            { 
                Checked = (IconThemeStyle)Enum.Parse(typeof(IconThemeStyle), x) == IconHandler.Style 
            }).ToArray();

        private MenuItem[] GlobalMenuSettings()
            => new MenuItem[]
            {
                new MenuItem(Properties.Menu.PluginFolder, PluginFolder_Event),
                new MenuItem(Properties.Menu.DeniedItems,new MenuItem[]
                {
                    new MenuItem(Properties.Menu.DeniedHours, DenyedtHours_Event),
                    new MenuItem(Properties.Menu.DeniedDays, DenyedDays_event)
                }
                ),
                new MenuItem(Properties.Menu.Weather, Construct(WeatherObjects, AppSetttingsHandler.Weather)),
                new MenuItem(Properties.Menu.SunRiseSet, Construct(SRSObjects, AppSetttingsHandler.SunRiseSet)),
            };        

        private MenuItem[] Construct<T>(IEnumerable<Lazy<T, Interface.IClassName>> Objects, string selected)
            =>  Objects.Select(x => new MenuItem(x.Metadata.ClassName, UpdateGlobalObjecttype) { Checked = x.Metadata.ClassName == selected }).ToArray();

        private IEnumerable<MenuItem> GetWeatherMenuItems()
        => new String[] {cDay, cNight }.SelectMany(
                x => Enum.GetNames(typeof(SharedObjects.WeatherTypes))
                .Select(y => 
                    new MenuItem(x + y, MenuItemClick) 
                    { Checked = g_ImageDictionary.ContainsKey(x + y) } 
                ));
        #endregion

        #region Main - Program entry point
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
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

        private void IconEventHandler(object sender, EventArgs e)
        {
            Enum.TryParse<IconThemeStyle>(((MenuItem)sender).Text, out IconThemeStyle style);
            IconHandler.Style = style;
            UpdateScreen(true);
        }

        private void PluginFolder_Event(object sender, EventArgs e)
        {
            try {if (!Directory.Exists(PluginPaths)) Directory.CreateDirectory(PluginPaths);}
            catch { }
            if (Directory.Exists(PluginPaths)) {Process.Start(PluginPaths); }
        }
        private void MenuItemClick(object sender, EventArgs e)
        {
            var Name = ((MenuItem)sender).Text;
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
                g_ImageDictionary.AddOrUpdate(Name, openFileDialog1.FileName);
                UpdateScreen(true);
                notifyIcon.ContextMenu = new ContextMenu(InitializeMenu());
            }
        }

        private void MenuAboutClick(object sender, EventArgs e)
        {
            var Name = this.GetType().Assembly.GetName().Name;
            var message = new StringBuilder();
            message.AppendLine(ExtractFromAssembly<AssemblyDescriptionAttribute>(this).Description)
                .Append(Environment.NewLine)
                .AppendLine("Version: " + this.GetType().Assembly.GetName().Version.ToString())
                .AppendLine("Copywrite: " + ExtractFromAssembly<AssemblyCopyrightAttribute>(this).Copyright)
                .Append(Environment.NewLine)
                .Append(ExtractInfo(g_Weather)).Append(ExtractInfo(g_SunRiseSet));
            MessageBox.Show(message.ToString(), Name, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MenuExitClick(object sender, EventArgs e) => Application.Exit(); 
        private void IconDoubleClick(object sender, EventArgs e) 
            => MessageBox.Show(((Interface.WeatherResponse)(g_Weather).Invoke()).ForcastDescription); 

        private void OnTimedEvent(object sender, EventArgs e) => UpdateScreen(false);

        private void DenyedtHours_Event(object sender, EventArgs e)
            => PromptBitArray(24, DeniedHours, Properties.Prompts.CSVForHours, Properties.Titles.EnterHours);
        

        private void DenyedDays_event(object sender, EventArgs e)
            => PromptBitArray(7, DeniedDays, Properties.Prompts.CSVForDays, Properties.Titles.EnterDays);
        
        private BitArray PromptBitArray(int sizeOfArray, BitArray original, string message, string title)
        {
            var ValuesCSV = InputHandler.InputBox(message, title, string.Join(",", original.SelectedIndexs()));
            var values = new List<int>();
            if (!string.IsNullOrEmpty(ValuesCSV)) values.AddRange(ValuesCSV.Split(',').Select(x => int.Parse(x)));
            return new BitArray(sizeOfArray, false).SetRange(values, true);
        }

        private void UpdateGlobalObjecttype(object sender, EventArgs e)
        {
            var Current = (MenuItem)sender;
            var Name = Current.Text;
            var Parent = ((MenuItem)Current.Parent).Text;
            if (Parent == Properties.Menu.Weather)
            {
                AppSetttingsHandler.Weather = Name;
                g_Weather = GetByName(WeatherObjects, Name);
                g_Weather.Load();
            }
            else if (Parent == Properties.Menu.SunRiseSet)
            {
                AppSetttingsHandler.SunRiseSet = Name;
                g_SunRiseSet = GetByName(SRSObjects, Name);
                g_SunRiseSet.Load();
            }
            notifyIcon.ContextMenu = new ContextMenu(InitializeMenu());
        }

        #endregion

        #region Private Functions
        
        private static T GetByName<T>(IEnumerable<Lazy<T, Interface.IClassName>> collection, string name)
             => collection.Where(x => x.Metadata.ClassName == name).Select(x => x.Value).FirstOrDefault();

        private string ExtractInfo(Interface.ISharedInterface Item)
        {
            var name = $"{Item.GetType().Assembly.GetName().Name} {Item}";
            var builder = new StringBuilder(name + Environment.NewLine);
            return builder.Append('_', name.Length).Append(Environment.NewLine)
            .AppendLine("Copyright: " + ExtractFromAssembly<AssemblyCopyrightAttribute>(g_Weather).Copyright)
            .AppendLine($"Version: {g_Weather.GetType().Assembly.GetName().Version}")
            .AppendLine("Debug Info: " + Item.Debug()).Append(Environment.NewLine).ToString();
        }

        private T ExtractFromAssembly<T>(object From) => (T)From.GetType().Assembly.GetCustomAttributes(typeof(T), false)[0];

        private void UpdateScreen(Boolean overrideImage)
        {
            if (!(DeniedDays[(int)DateTime.Now.DayOfWeek] || DeniedHours[DateTime.Now.Hour]))
            {
                var weather = (Interface.WeatherResponse)g_Weather.Invoke();
                var sunriseSet = (Interface.SunRiseSetResponse)g_SunRiseSet.Invoke();
                
                var currentTime = DateTime.Now.TimeOfDay.Between(sunriseSet.SunRise.TimeOfDay, sunriseSet.SunSet.TimeOfDay)
                    ? cDay : cNight;
     
                var weatherType = Enum.GetName(typeof(SharedObjects.WeatherTypes), weather.WType);
                notifyIcon.Text = $"{weatherType} {weather.Temp}";
                var currentWeatherType = currentTime + weatherType;
                if (string.IsNullOrWhiteSpace(g_CurrentWeatherType) || currentWeatherType != g_CurrentWeatherType || overrideImage)
                {
                    g_CurrentWeatherType = currentWeatherType;
                    var IconStyle = IconHandler.Style;
                    var isDay = (currentTime == cDay);
                    notifyIcon.Icon = Wallpaper.GetWeatherIcon(weather.WType, isDay, IconHandler.IsLightMode(IconStyle, !isDay));
                    if (g_ImageDictionary.ContainsKey(currentWeatherType))
                    {
                        try { Wallpaper.Set(g_ImageDictionary[currentWeatherType], Wallpaper.Style.Stretched);}
                        catch (Exception x) { MessageBox.Show(x.ToString()); }
                    }
                }
            }
            if (Themes.RefreshMenu)
            {
                notifyIcon.ContextMenu = new ContextMenu(InitializeMenu());
            }
        }


        private void DeclareGlobals()
        {
            try
            {
                if (!LatLongHandler.HasRecord())
                {
                    if (LatLongObjects != null)
                    {
                        var ordered = LatLongObjects.OrderBy(d => {return (d.Metadata.ClassName == "OpenDataFlatFileLookup") ? 1 : 2;});
                        var obj = (from latlongObject in ordered let lat = latlongObject.Value where lat.worked()
                                   select new Tuple<double, double>(lat.Latitude(), lat.Longitude())).First();
                        LatLongHandler.Set(obj.Item1, obj.Item2);
                    }
                }
                g_Weather = (from v in PullByPrefered(WeatherObjects, AppSetttingsHandler.Weather, "GovWeather3")
                             where TryLoad(v) select v).First();
                g_SunRiseSet = (from v in PullByPrefered(SRSObjects, AppSetttingsHandler.SunRiseSet, "InternalSunRiseSet")
                                where TryLoad(v) select v).First();
            }
            catch (Exception x)
            {
                ErrorHandler.Send(x);
            }

            UpdateImageCache();
            UpdateDeniedList();
        }

        private static bool TryLoad(Interface.ISharedInterface item) 
        {
            try{item.Load(); return true;}catch (Exception x){ ErrorHandler.Send(x); return false; }
        }

        private IEnumerable<T> PullByPrefered<T>(IEnumerable<Lazy<T, Interface.IClassName>> collection, string prefered, string secondary)
        {
            T response = GetByName(collection, prefered);
            if (response != null) yield return response;

            var fixedOrder = new List<string> { secondary };

            var ordered = collection.OrderBy(d =>
            {
                var index = fixedOrder.IndexOf(d.Metadata.ClassName);
                if (d.Metadata.ClassName.StartsWith("Mock")) return int.MaxValue;
                return (index == -1) ? int.MaxValue - 1 : index;
            });
            var TT = ordered.ToArray();
            foreach (var item in ordered)
            {
                yield return item.Value;
            }
        }
        #region Support functions to reduce complexity
        private void UpdateDeniedList()
        {
            int.TryParse(AppSetttingsHandler.DeniedHours, out int intDenyedHours);
            int.TryParse(AppSetttingsHandler.DeniedDays, out int intDenyedDays);
            DeniedHours = intDenyedHours.ToBitArray();
            DeniedDays = intDenyedDays.ToBitArray();
        }
        private void CreateTimer()
        {
            var t = new System.Windows.Forms.Timer{Interval = 3000};
            t.Tick += new EventHandler(OnTimedEvent);
            t.Start();
        }
        private void UpdateImageCache()
        {
            var nightday = new string[] { cDay, cNight };
            g_ImageDictionary = Enum.GetNames(typeof(SharedObjects.WeatherTypes))
                .SelectMany(x => 
                   from y in nightday
                   let Item = AppSetttingsHandler.Read(y+x)
                   where (!string.IsNullOrEmpty(Item))
                   select new Tuple<string, string>(y+x, Item))
                .ToDictionary(tkey => tkey.Item1, tvalue=> tvalue.Item2);
        }
        private void LazyLoader()
        {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(WeatherDesktop.MainApp).Assembly));
            catalog.Catalogs.Add(new DirectoryCatalog(Environment.CurrentDirectory));

            if (Directory.Exists(PluginPaths)) {
                catalog.Catalogs.Add(new DirectoryCatalog(PluginPaths));                
                foreach (var item in Directory.EnumerateDirectories(PluginPaths))
                {
                    catalog.Catalogs.Add(new DirectoryCatalog(item));
                }
            }
            _container = new CompositionContainer(catalog);
            try
            {
                this._container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                ErrorHandler.Send(compositionException);
            }
        }
        #endregion
        #region IDisposable Support
        private bool disposedValue = false;

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing){notifyIcon.Dispose();  _container.Dispose();}
                disposedValue = true;
            }
        }

        void IDisposable.Dispose() => Dispose(true);
        #endregion
        #endregion
    }
}
