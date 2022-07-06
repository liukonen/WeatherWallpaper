using System.Configuration;
using System.Windows.Forms;

namespace WeatherDesktop.Shared.Handlers
{
    public class AppSetttingsHandler
    {
        public static string Read(string Key) 
        {
            try
            {
                return ConfigurationManager.AppSettings[Key];
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show(Properties.Resources.AppSettingsReadErrorMessage);
                return string.Empty;
            }
        }
        public static void Write(string key, string value) 
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null) { settings.Add(key, value); }
                else { settings[key].Value = value; }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show(Properties.Resources.AppSettingsWriteErrorMessage
                    , Properties.Resources.AppSettingsWriteErrorTitle
                    , MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void Remove(string key)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            if (settings[key] != null)
            {
                settings.Remove(key);

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
        }


    }
}
