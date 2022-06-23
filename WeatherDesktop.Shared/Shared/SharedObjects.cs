﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Configuration;
using System.Security.Cryptography;
using WeatherDesktop.Shared.Internal;

namespace WeatherDesktop.Share
{
    public static class SharedObjects
    {
        public enum WeatherTypes { ThunderStorm, Rain, Snow, Dust, Fog, Haze, Smoke, Windy, Frigid, Cloudy, PartlyCloudy, Clear, Hot };


        #region VB Input boxes
        public static string InputBox(string Prompt) { return InputBox(Prompt, string.Empty); }
        public static string InputBox(string Prompt, string Title) { return InputBox(Prompt, Title, string.Empty); }
        public static string InputBox(string Prompt, string Title, string DefaultResponse) { return Microsoft.VisualBasic.Interaction.InputBox(Prompt, Title, DefaultResponse); }
        #endregion


        public static class ZipObjects
        {
            public static MenuItem ZipMenuItem
            {
                get { return new MenuItem("Change Zip Code", ChangeZipClick); }
            }

            public static void ChangeZipClick(object sender, EventArgs e)
            {
                GetZip();
            }

            /// <summary>
            /// Opens a dialog box, saves zip and returns value. if cancel, closes app gracefully... will retry if not a number
            /// </summary>
            /// <returns></returns>
            public static string GetZip()
            {
                string zip = string.Empty;
                object locker = new object();
                lock (locker)
                {
                    while (!int.TryParse(zip, out int zipparse))
                    {
                        zip = Microsoft.VisualBasic.Interaction.InputBox("Please enter your zip code.", "Zip Code", Rawzip);
                        if (string.IsNullOrWhiteSpace(zip))
                        {
                            if (MessageBox.Show("Application needs your zip code. try again, or close", "error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                            {
                                Application.Exit();
                            }
                        }
                    }
                }
                Rawzip = zip;
                return zip;
            }

            public static string Rawzip
            {
                get
                {
                    return AppSettings.ReadSettingEncrypted("zipcode");
                }
                set
                {
                    AppSettings.AddupdateAppSettingsEncrypted("zipcode", value);
                }
            }

            public static string TryGetZip()
            {
                if (string.IsNullOrWhiteSpace(Rawzip)) return GetZip();
                return Rawzip;
            }
        }

        public static class AppSettings
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
                    byte[] entropy = System.Text.Encoding.Unicode.GetBytes(key);// entropy only adds additional complexity. could use null
                    var Encrypted = ProtectedData.Protect(System.Text.Encoding.Unicode.GetBytes(value), entropy, DataProtectionScope.LocalMachine);
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
                    entropy = System.Text.Encoding.Unicode.GetBytes(key);// entropy only adds additional complexity. could use null
                    string EncryptedString = ReadSetting(key);
                    decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(EncryptedString), entropy, DataProtectionScope.LocalMachine);
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

            #endregion
        }

        public static class Cache
        {

            public static T GetValue<T>(string key) => MemCache.Instance.Exists(key) ? MemCache.Instance.GetItem<T>(key) : default;

            public static bool Exists(string key) => MemCache.Instance.Exists(key);

            public static void SetValue<T>(string key, T o, int timeout) => MemCache.Instance.SetItem<T>(key, o, timeout);

            public static void SetValue<T>(string key, T o) => MemCache.Instance.SetItem<T>(key, o);


            [Obsolete("Please use generic GetValue.")]
            public static object Value(string key)
            {
                if (MemCache.Instance.Exists(key)) return MemCache.Instance.GetItem<object>(key);
                return string.Empty;
            }
            [Obsolete("Please use Generic GetValue.")]
            public static string StringValue(string key)=> MemCache.Instance.GetItem<string>(key);

            [Obsolete("Please use Generic SetValue.")]
            public static void Set(string key, object o, int timeout) => MemCache.Instance.SetItem<object>(key, o, timeout);

            [Obsolete("Please use Generic SetValue.")]
            public static void Set(string key, object o) => MemCache.Instance.SetItem<object>(key, o);

        }

        public class LatLong
        {
            //public bool HasValue { return false; }
            const string csvEncryptedLatLongName = "LatLong";
            public static double Lat
            {
                get
                {
                    string value = WeatherDesktop.Share.SharedObjects.AppSettings.ReadSettingEncrypted(csvEncryptedLatLongName);
                    if (value != null) return double.Parse(value.Split(',')[0].Replace(",", string.Empty));
                    return 0;
                }
            }
            public static double Lng
            {
                get
                {
                    string value = WeatherDesktop.Share.SharedObjects.AppSettings.ReadSettingEncrypted(csvEncryptedLatLongName);
                    if (value != null) return double.Parse(value.Split(',')[1].Replace(",", string.Empty));
                    return 0;
                }
            }

            public static bool HasRecord() { return (!string.IsNullOrWhiteSpace(WeatherDesktop.Share.SharedObjects.AppSettings.ReadSettingEncrypted(csvEncryptedLatLongName))); }
            public static void Set(double dLat, double dLng)
            {
                WeatherDesktop.Share.SharedObjects.AppSettings.AddupdateAppSettingsEncrypted(csvEncryptedLatLongName, string.Join(",", dLat, dLng));
            }

        }

        #region Web Request
        public static string CompressedCallSite(string Url) => WebHandler.Instance.CallSite(Url);

        public static string CompressedCallSite(string Url, string UserAgent) => WebHandler.Instance.CallSite(Url, UserAgent);
  
    

        #endregion

        public static bool BetweenTimespans(TimeSpan test, TimeSpan LowerValue, TimeSpan Highervalue) { return (LowerValue < test && test < Highervalue); }

        public static string CompileDebug(System.Collections.Generic.Dictionary<string, string> ItemsTodisplay)
        {
            StringBuilder SB = new StringBuilder();
            SB.Append(Environment.NewLine);
            foreach (var item in ItemsTodisplay)
            {
                SB.Append(item.Key).Append(": ").Append(item.Value).Append(Environment.NewLine);
            }
            SB.Append(Environment.NewLine);
            return SB.ToString();
        }

        /// <summary>
        /// Bitarray should be under 32 bits
        /// </summary>
        /// <param name="Array"></param>
        /// <returns></returns>
        public static int ConvertBitarrayToInt(System.Collections.BitArray Array)
        {
            int[] array = new int[1];
            Array.CopyTo(array, 0);
            return array[0];
        }

        public static BitArray ConverTIntToBitArray(int item) => new BitArray(new int[] { item });
        

    }
}
