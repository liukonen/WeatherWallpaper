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

        public static string Weather 
        {
            get => Read("Weatherapp");
            set => Write("Weatherapp", value);         
        }

        public static string SunRiseSet
        {
            get => Read("SunRiseSet");
            set => Write("SunRiseSet", value);
        }

        public static string DeniedDays
        {
            get => Read("DeniedDays");
            set => Write("DeniedDays", value);
        }

        public static string DeniedHours
        {
            get => Read("DeniedHours");
            set => Write("DeniedHours", value);
        }

        public static string HourUpdate
        {
            get => Read("HourUpdate");
            set => Write("HourUpdate", value);
        }
    }
}
