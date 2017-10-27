using System;
using System.IO;
using System.Net;
using System.Configuration;
using System.Windows.Forms;

namespace WeatherDesktop.Interfaces
{
    /// <summary>
    /// Description of shared.
    /// </summary>
    public static class Shared
    {
        public enum WeatherTypes { ThunderStorm, Rain, Snow, Dust, Fog, Haze, Smoke, Windy, Frigid, Cloudy, PartlyCloudy, Clear, Hot };

        public static string CompressedCallSite(string Url)
        {
            HttpWebRequest request = (System.Net.HttpWebRequest)HttpWebRequest.Create(Url);
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            using (System.Net.WebResponse response = request.GetResponse())
            {
                StreamReader Reader = new System.IO.StreamReader(response.GetResponseStream());
                return Reader.ReadToEnd();
            }
        }

        //taken from MSDN https://msdn.microsoft.com/en-us/library/system.configuration.configurationmanager.aspx
        public static void AddUpdateAppSettings(string key, string value)
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
                MessageBox.Show("Error writing app settings", "Error Editing Config", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string ReadSettingEncrypted(string key)
        {
            byte[] entropy = new byte[0]; byte[] decryptedData = new byte[0];

            try
            {
                entropy = System.Text.Encoding.Unicode.GetBytes(key);// entropy only adds additional complexity. could use null
                string EncryptedString = ReadSetting(key);
                decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(Convert.FromBase64String(EncryptedString), entropy, System.Security.Cryptography.DataProtectionScope.LocalMachine);
                return System.Text.Encoding.Unicode.GetString(decryptedData);
            }
            catch { return string.Empty; }
            finally
            {
                Array.Clear(entropy, 0, entropy.Length);
                Array.Clear(decryptedData, 0, decryptedData.Length);
                entropy = new byte[0];
                decryptedData = new byte[0];
            }
           
        }

        public static string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                return appSettings[key];
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error reading app settings");
                return string.Empty;
            }
        }

        public static void AddupdateAppSettingsEncrypted(string key, string value)
        {
            try
            {
                byte[] entropy = System.Text.Encoding.Unicode.GetBytes(key);// entropy only adds additional complexity. could use null
                var Encrypted = System.Security.Cryptography.ProtectedData.Protect(System.Text.Encoding.Unicode.GetBytes(value), entropy, System.Security.Cryptography.DataProtectionScope.LocalMachine);
                AddUpdateAppSettings(key, Convert.ToBase64String(Encrypted));
            }
            catch (Exception x)
            { MessageBox.Show(x.Message, "error writing to Config file"); }
        }



        public static string CompileDebug(string objectName, System.Collections.Generic.Dictionary<string, string> ItemsTodisplay)
        {
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            SB.Append(Environment.NewLine);
            SB.Append(objectName).Append(Environment.NewLine);
            SB.Append('-', objectName.Length).Append(Environment.NewLine);
            foreach (var item in ItemsTodisplay)
            {
                SB.Append(item.Key).Append(": ").Append(item.Value).Append(Environment.NewLine);
            }
            SB.Append(Environment.NewLine);
            return SB.ToString();

        }

        public static bool BetweenTimespans(TimeSpan test, TimeSpan LowerValue, TimeSpan Highervalue)
        {
            return (LowerValue < test && test < Highervalue);
        }
    }


public abstract class SharedExternalinterface
    {

        public abstract SharedResponse Invoke();
        public abstract string Debug();
        public abstract MenuItem[] SettingsItems();


    }
}
