using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Configuration;
using System.Security.Cryptography;
using WeatherDesktop.Shared.Handlers;
using WeatherDesktop.Shared.Extentions;

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

            public static void AddUpdateAppSettings(string key, string value)
                => AppSetttingsHandler.Write(key, value);

            public static void RemoveAppSetting(string key)
                => AppSetttingsHandler.Remove(key);

            public static string ReadSetting(string key)
                => AppSetttingsHandler.Read(key);

            #endregion

            #region App Config (Encrypted)

            public static void AddupdateAppSettingsEncrypted(string key, string value)
                => EncryptedAppSettingsHandler.Write(key, value);

            public static string ReadSettingEncrypted(string key) 
                => EncryptedAppSettingsHandler.Read(key);


            #endregion
        }

        public static class Cache
        {
          
            public static bool Exists(string key) => MemCacheHandler.Instance.Exists(key);
          
            //[Obsolete("Please use generic GetValue.")]
            public static object Value(string key)
            {
                if (MemCacheHandler.Instance.Exists(key)) return MemCacheHandler.Instance.GetItem<object>(key);
                return string.Empty;
            }
            //[Obsolete("Please use Generic GetValue.")]
            public static string StringValue(string key)=> MemCacheHandler.Instance.GetItem<string>(key);

            //[Obsolete("Please use Generic SetValue.")]
            public static void Set(string key, object o, int timeout) => MemCacheHandler.Instance.SetItem<object>(key, o, timeout);

            //[Obsolete("Please use Generic SetValue.")]
            public static void Set(string key, object o) => MemCacheHandler.Instance.SetItem<object>(key, o);

        }

        public class LatLong
        {
            //public bool HasValue { return false; }
            const string csvEncryptedLatLongName = "LatLong";
            public static double Lat
            {
                get
                {
                    string value = SharedObjects.AppSettings.ReadSettingEncrypted(csvEncryptedLatLongName);
                    if (value != null) return double.Parse(value.Split(',')[0].Replace(",", string.Empty));
                    return 0;
                }
            }
            public static double Lng
            {
                get
                {
                    string value = SharedObjects.AppSettings.ReadSettingEncrypted(csvEncryptedLatLongName);
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

        public static bool BetweenTimespans(TimeSpan test, TimeSpan LowerValue, TimeSpan Highervalue) => test.Between(LowerValue, Highervalue);

        public static string CompileDebug(System.Collections.Generic.Dictionary<string, string> ItemsTodisplay) => ItemsTodisplay.CompileDebug();

        /// <summary>
        /// Bitarray should be under 32 bits
        /// </summary>
        /// <param name="Array"></param>
        /// <returns></returns>
        public static int ConvertBitarrayToInt(BitArray Array) => Array.ToInt();

        public static BitArray ConverTIntToBitArray(int item) => item.ToBitArray();
        

    }
}
