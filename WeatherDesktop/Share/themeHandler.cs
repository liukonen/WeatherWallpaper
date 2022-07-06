using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using WeatherDesktop.Shared.Handlers;

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
            var themepath = themesDir + Path.DirectorySeparatorChar + ((MenuItem)sender).Text;

            var rootElement = XElement.Parse(File.ReadAllText(themepath + Path.DirectorySeparatorChar + ThemeFileName));
            var ThemeItems = new Dictionary<string, string>();
            foreach (XElement item in rootElement.Elements())
            {
                ThemeItems.Add(item.Name.LocalName, item.Value);
            }
            var cSRS = new string[] { "day-", "night-" };


            //copy files to theme dir
            foreach (string NightDay in cSRS)
            {
                foreach (string item in Enum.GetNames(typeof(SharedObjects.WeatherTypes)))
                {
                    var key = NightDay + item;
                    if (ThemeItems.ContainsKey(key))
                    {
                        AppSetttingsHandler.Write(key, themepath + Path.DirectorySeparatorChar + ThemeItems[key]);
                    }
                    else
                    {
                        AppSetttingsHandler.Remove(key);
                    }
                }
            }
        }

        private void SaveTheme(object sender, EventArgs e)
        {
            var Theme = new Dictionary<string, string>();

            var Dia = new SaveFileDialog
            {
                AddExtension = true,
                Filter = Filter
            };
            if (Dia.ShowDialog() == DialogResult.OK)
            {
                var FileName = Dia.FileName;
                var CurrentThemeDir = themesDir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(FileName);
                if (Directory.Exists(CurrentThemeDir)) { MessageBox.Show("Theme already exists, please try another name"); }
                else
                {
                    Directory.CreateDirectory(CurrentThemeDir);

                    var cSRS = new string[] { "day-", "night-" };

                    //copy files to theme dir
                    foreach (string NightDay in cSRS)
                    {
                        foreach (string item in Enum.GetNames(typeof(SharedObjects.WeatherTypes)))
                        {
                            var key = NightDay + item;
                            var imagelocation = AppSetttingsHandler.Read(key);
                            if (!string.IsNullOrWhiteSpace(imagelocation))
                            {
                                var newLocation = 
                                    CurrentThemeDir + 
                                    Path.DirectorySeparatorChar + 
                                    Path.GetFileName(imagelocation);

                                File.Copy(imagelocation, newLocation);
                                Theme.Add(key, Path.GetFileName(imagelocation));
                            }

                        }
                    }
                    //create theme index

                    var items = Theme.Select(x => new XElement(x.Key, x.Value));
                    var element = new XElement("root", items);
                    element.Save(CurrentThemeDir + Path.DirectorySeparatorChar + ThemeFileName);

                    //compress theme to filename zip file
                    ZipFile.CreateFromDirectory(CurrentThemeDir, FileName);
                    _RefreshMenu = true;
                }
            }
        }

        private void ImportTheme(object sender, EventArgs e)
        {
            var Dia = new OpenFileDialog
            {
                Filter = Filter
            };
            if (Dia.ShowDialog() == DialogResult.OK)
            {
                var FileName = Dia.FileName;
                var test = ZipFile.OpenRead(FileName);
                var ValidTheme = (from ZipArchiveEntry B in test.Entries where B.Name == ThemeFileName select B).Any();
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

        public MenuItem[] SettingsItems() => new List<MenuItem>()
            {
               new MenuItem("Save Current Settings as Theme.", SaveTheme),
               new MenuItem("Load Theme", ThemeArray().ToArray()),
               new MenuItem("Import Theme", ImportTheme)
        }.ToArray();
        
    }
}
