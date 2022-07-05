using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WeatherDesktop.Share
{
    class ThemeHandler
    {
        const string ThemeFileName = "theme.xml";
        const string Filter = "Zip file (*.zip)|*.zip";

        string themesDir = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "themes";
        bool _RefreshMenu = false;

        public bool RefreshMenu
        {
            get
            {
                if (_RefreshMenu) { _RefreshMenu = false; return true; }
                return false;
            }
        }


        private void LoadTheme(object sender, EventArgs e)
        {
            string themepath = themesDir + Path.DirectorySeparatorChar + ((MenuItem)sender).Text;

            XElement rootElement = XElement.Parse(File.ReadAllText(themepath + Path.DirectorySeparatorChar + ThemeFileName));
            Dictionary<string, string> ThemeItems = new Dictionary<string, string>();
            foreach (XElement item in rootElement.Elements())
            {
                ThemeItems.Add(item.Name.LocalName, item.Value);
            }
            string[] cSRS = new string[] { "day-", "night-" };


            //copy files to theme dir
            foreach (string NightDay in cSRS)
            {
                foreach (string item in Enum.GetNames(typeof(SharedObjects.WeatherTypes)))
                {
                    string key = NightDay + item;
                    if (ThemeItems.ContainsKey(key))
                    {
                        SharedObjects.AppSettings.AddUpdateAppSettings(key, themepath + Path.DirectorySeparatorChar + ThemeItems[key]);
                    }
                    else
                    {
                        SharedObjects.AppSettings.RemoveAppSetting(key);
                    }
                }
            }
        }

        private void SaveTheme(object sender, EventArgs e)
        {
            Dictionary<string, string> Theme = new Dictionary<string, string>();

            SaveFileDialog Dia = new SaveFileDialog
            {
                AddExtension = true,
                Filter = Filter
            };
            if (Dia.ShowDialog() == DialogResult.OK)
            {
                string FileName = Dia.FileName;
                string CurrentThemeDir = themesDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileName);
                if (Directory.Exists(CurrentThemeDir)) { MessageBox.Show("Theme already exists, please try another name"); }
                else
                {
                    Directory.CreateDirectory(CurrentThemeDir);

                    string[] cSRS = new string[] { "day-", "night-" };


                    //copy files to theme dir
                    foreach (string NightDay in cSRS)
                    {


                        foreach (string item in Enum.GetNames(typeof(SharedObjects.WeatherTypes)))
                        {
                            string key = NightDay + item;
                            string imagelocation = SharedObjects.AppSettings.ReadSetting(key);
                            if (!string.IsNullOrWhiteSpace(imagelocation))
                            {
                                string newLocation = CurrentThemeDir + System.IO.Path.DirectorySeparatorChar + Path.GetFileName(imagelocation);
                                System.IO.File.Copy(imagelocation, newLocation);
                                Theme.Add(key, System.IO.Path.GetFileName(imagelocation));
                            }

                        }
                    }
                    //create theme index

                    var Items = from KeyValuePair<string, string> X in Theme select new System.Xml.Linq.XElement(X.Key, X.Value);
                    System.Xml.Linq.XElement element = new System.Xml.Linq.XElement("root", Items);
                    element.Save(CurrentThemeDir + Path.DirectorySeparatorChar + ThemeFileName);

                    //compress theme to filename zip file
                    ZipFile.CreateFromDirectory(CurrentThemeDir, FileName);
                    _RefreshMenu = true;
                }
            }
        }

        private void ImportTheme(object sender, EventArgs e)
        {
            OpenFileDialog Dia = new OpenFileDialog
            {
                Filter = Filter
            };
            if (Dia.ShowDialog() == DialogResult.OK)
            {
                string FileName = Dia.FileName;
                var test = ZipFile.OpenRead(FileName);
                bool ValidTheme = (from ZipArchiveEntry B in test.Entries where B.Name == ThemeFileName select B).Any();
                if (ValidTheme)
                {
                    test.ExtractToDirectory(themesDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileName));
                    _RefreshMenu = true;
                }
            }
        }

        private IEnumerable<MenuItem> ThemeArray()
        {
            try
            {
                if (!Directory.Exists(themesDir)) { Directory.CreateDirectory(themesDir); }

                return Directory.EnumerateDirectories(themesDir)
                    .Select(x => 
                    new MenuItem(
                        x.Substring(x.LastIndexOf(Path.DirectorySeparatorChar) + 1), 
                        LoadTheme));
            }
            catch (Exception x) { ErrorHandler.Send(x); }
            return new List<MenuItem>();

        }

        public MenuItem[] SettingsItems()
        {
            MenuItem Save = new MenuItem("Save Current Settings as Theme.", SaveTheme);
            MenuItem Load = new MenuItem("Load Theme", ThemeArray().ToArray());
            MenuItem Import = new MenuItem("Import Theme", ImportTheme);
            return new MenuItem[] { Save, Load, Import };
        }
    }
}
