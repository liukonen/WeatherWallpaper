using System;
using System.IO;
using System.Net;
using System.Configuration;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;


namespace WeatherDesktop.Interface
{
    /// <summary>
    /// Description of shared.
    /// </summary>
    public static class Shared
    {

        public static class Cache
        {

            private static string TransformKey(string key)
            { return WeatherDesktop.Shared.SystemLevelConstants.AppName + "_" + key; }

            public static object Value(string key)
            {
                string NewKey = TransformKey(key);
                System.Runtime.Caching.MemoryCache cache = System.Runtime.Caching.MemoryCache.Default;
                if (cache.Contains(NewKey)) return cache[NewKey];
                return string.Empty;
            }

            public static string StringValue(string key)
            {
                return (string)Value(key);
            }

            public static bool Exists(string key)
            {
                System.Runtime.Caching.MemoryCache cache = System.Runtime.Caching.MemoryCache.Default;
                return cache.Contains(TransformKey(key));
            }

            public static void Set(string key, object o, int timeout)
            {
                System.Runtime.Caching.MemoryCache cache = System.Runtime.Caching.MemoryCache.Default;
                cache.Add(TransformKey(key), o, DateTime.Now.AddMinutes(timeout));
            }
            public static void Set(string key, object o)
            {
                Set(key, o, 15);
            }

        }

        public class LatLong
        {
            //public bool HasValue { return false; }

            public static double lat
            {
                get
                {
                    string value = WeatherDesktop.Interface.Shared.ReadSettingEncrypted(WeatherDesktop.Shared.SystemLevelConstants.csvEncryptedLatLongName);
                    if (value != null) return double.Parse(value.Split(',')[0].Replace(",", string.Empty));
                    return 0;
                }
            }
            public static double lng {
                get
                {
                    string value = WeatherDesktop.Interface.Shared.ReadSettingEncrypted(WeatherDesktop.Shared.SystemLevelConstants.csvEncryptedLatLongName);
                    if (value != null) return double.Parse(value.Split(',')[1].Replace(",", string.Empty));
                    return 0;
                }
            }

            public static bool HasRecord() { return (!string.IsNullOrWhiteSpace(WeatherDesktop.Interface.Shared.ReadSettingEncrypted(WeatherDesktop.Shared.SystemLevelConstants.csvEncryptedLatLongName))); }
            public static void set(double dLat, double dLng)
            {
                WeatherDesktop.Interface.Shared.AddupdateAppSettingsEncrypted(WeatherDesktop.Shared.SystemLevelConstants.csvEncryptedLatLongName, string.Join(",", dLat, dLng));
            }

        }

        public enum WeatherTypes { ThunderStorm, Rain, Snow, Dust, Fog, Haze, Smoke, Windy, Frigid, Cloudy, PartlyCloudy, Clear, Hot };


        #region Web Request
        public static string CompressedCallSite(string Url)
        {
            return CompressedCallSite(Url, string.Empty);

        }
        public static string CompressedCallSite(string Url, string UserAgent)
        {
            HttpWebRequest request = (System.Net.HttpWebRequest)HttpWebRequest.Create(Url);
            if (!string.IsNullOrWhiteSpace(UserAgent)) { request.UserAgent = UserAgent; }
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.AllowAutoRedirect = true;

            using (System.Net.WebResponse response = request.GetResponse())
            {
                StreamReader Reader = new System.IO.StreamReader(response.GetResponseStream());
                return Reader.ReadToEnd();
            }

        }

        #endregion

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
            { MessageBox.Show(x.Message, "error writing to Config file"); }
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

        #region Helpers
        public static bool BetweenTimespans(TimeSpan test, TimeSpan LowerValue, TimeSpan Highervalue){ return (LowerValue < test && test < Highervalue);}

        public static string CompileDebug(string objectName, System.Collections.Generic.Dictionary<string, string> ItemsTodisplay)
        {
            StringBuilder SB = new StringBuilder();
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

        public static System.Collections.BitArray ConverTIntToBitArray(int item)
        {
            return new System.Collections.BitArray(new int[] {item});
        }

        #endregion

        public static MenuItem ZipMenuItem
        {
            get { return new MenuItem("Change Zip Code", ChangeZipClick); }
        }

        public static void ChangeZipClick(object sender, EventArgs e)
        {
            string NewZip;
            try
            {
                int DumbyInt = 0;
                string CurrentZip = ReadSettingEncrypted(WeatherDesktop.Shared.SystemLevelConstants.ZipCode);
                NewZip = Microsoft.VisualBasic.Interaction.InputBox("Please enter your zipcode", "Zip Code", CurrentZip);
                if (!int.TryParse(NewZip, out DumbyInt)) { MessageBox.Show("Could not save zip"); }
                else {                    AddupdateAppSettingsEncrypted(WeatherDesktop.Shared.SystemLevelConstants.ZipCode, NewZip);}
            }
            catch { MessageBox.Show("Unable to update Zip code", "warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
         

 

    }
    
    

}
