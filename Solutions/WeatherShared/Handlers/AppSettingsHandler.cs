using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace WeatherShared.Handlers
{


    public static class AppSettingsHandler
    {
        #region App Config (No Encryption)

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

        public static void RemoveAppSetting(string key)
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
        public static string ReadSetting(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error reading app settings"); return string.Empty;
            }
        }

        #endregion

        #region App Config (Encrypted)

        public static void AddupdateAppSettingsEncrypted(string key, string value)
        {
            try
            {
                byte[] entropy = Encoding.Unicode.GetBytes(key);// entropy only adds additional complexity. could use null
                var Encrypted = ProtectedData.Protect(Encoding.Unicode.GetBytes(value), entropy, DataProtectionScope.LocalMachine);
                AddUpdateAppSettings(key, Convert.ToBase64String(Encrypted));
            }
            catch (Exception x)
            { MessageBox.Show(x.Message, "error writing to Config file"); WeatherDesktop.Share.ErrorHandler.LogException(x); }
        }

        public static string ReadSettingEncrypted(string key)
        {
            byte[] entropy = new byte[0]; byte[] decryptedData = new byte[0];

            try
            {
                entropy = Encoding.Unicode.GetBytes(key);// entropy only adds additional complexity. could use null
                string EncryptedString = ReadSetting(key);
                decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(EncryptedString), entropy, DataProtectionScope.LocalMachine);
                return Encoding.Unicode.GetString(decryptedData);
            }
            catch { return string.Empty; }
            finally
            {
                Array.Clear(entropy, 0, entropy.Length);
                Array.Clear(decryptedData, 0, decryptedData.Length);
            }

        }

        #endregion
    }
}
