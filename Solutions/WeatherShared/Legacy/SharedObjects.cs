using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Configuration;
using System.Security.Cryptography;
using WeatherShared.Extentions;

namespace WeatherDesktop.Share
{
    public static class SharedObjects
    {
        //public enum WeatherTypes { ThunderStorm, Rain, Snow, Dust, Fog, Haze, Smoke, Windy, Frigid, Cloudy, PartlyCloudy, Clear, Hot };


        #region VB Input boxes
        [Obsolete("Please use Handler.Inputhandler")]
        public static string InputBox(string Prompt) => WeatherShared.Handlers.InputHandler.Input(Prompt);
        [Obsolete("Please use Handler.Inputhandler")]
        public static string InputBox(string Prompt, string Title) => WeatherShared.Handlers.InputHandler.Input(Prompt);

        [Obsolete("Please use Handler.Inputhandler")]
        public static string InputBox(string Prompt, string Title, string DefaultResponse) => WeatherShared.Handlers.InputHandler.Input(Prompt, DefaultResponse);
        #endregion


        public static class ZipObjects
        {
            [Obsolete("Moved to Handlers.ZipcodeHandler")]
            public static MenuItem ZipMenuItem => WeatherShared.Handlers.ZipcodeHandler.ZipMenuItem;

            [Obsolete("Moved to Handlers.ZipcodeHandler")]
            public static void ChangeZipClick(object sender, EventArgs e) => WeatherShared.Handlers.ZipcodeHandler.ChangeZipClick(sender, e);   

            /// <summary>
            /// Opens a dialog box, saves zip and returns value. if cancel, closes app gracefully... will retry if not a number
            /// </summary>
            /// <returns></returns>
            [Obsolete("Moved to Handlers.ZipcodeHandler")]
            public static string GetZip() => WeatherShared.Handlers.ZipcodeHandler.GetZip();

            [Obsolete("Moved to Handlers.ZipcodeHandler")]
            public static string Rawzip => WeatherShared.Handlers.ZipcodeHandler.Rawzip;
            [Obsolete("Moved to Handlers.ZipcodeHandler")]
            public static string TryGetZip() => WeatherShared.Handlers.ZipcodeHandler.TryGetZip();
        }

        public static class AppSettings
        {
            #region App Config (No Encryption)

            [Obsolete("Moved to Handlers.AppSettingsHandler")]
            public static void AddUpdateAppSettings(string key, string value) 
                => WeatherShared.Handlers.AppSettingsHandler.AddUpdateAppSettings(key, value);

            [Obsolete("Moved to Handlers.AppSettingsHandler")]
            public static void RemoveAppSetting(string key)
                => WeatherShared.Handlers.AppSettingsHandler.RemoveAppSetting(key);
            [Obsolete("Moved to Handlers.AppSettingsHandler")]
            public static string ReadSetting(string key)
                => WeatherShared.Handlers.AppSettingsHandler.ReadSetting(key);

            #endregion

            #region App Config (Encrypted)
            [Obsolete("Moved to Handlers.AppSettingsHandler")]
            public static void AddupdateAppSettingsEncrypted(string key, string value)
                => WeatherShared.Handlers.AppSettingsHandler.AddupdateAppSettingsEncrypted(key, value);
            [Obsolete("Moved to Handlers.AppSettingsHandler")]
            public static string ReadSettingEncrypted(string key)
                => WeatherShared.Handlers.AppSettingsHandler.ReadSettingEncrypted(key);

            #endregion
        }

        public static class Cache
        {


            [Obsolete("Please use Handlers.MemCacheHandler")]
            public static object Value(string key)
            {
                if (WeatherShared.Handlers.MemCacheHandler.Instance.Exists(key)) return WeatherShared.Handlers.MemCacheHandler.Instance.GetItem<object>(key);
                return string.Empty;
            }
            [Obsolete("Please use  Handlers.MemCacheHandler.")]
            public static string StringValue(string key) => WeatherShared.Handlers.MemCacheHandler.Instance.GetItem<string>(key);

            [Obsolete("Please use  Handlers.MemCacheHandler.")]
            public static void Set(string key, object o, int timeout) => WeatherShared.Handlers.MemCacheHandler.Instance.SetItem<object>(key, o, timeout);

            [Obsolete("Please use  Handlers.MemCacheHandler.")]
            public static void Set(string key, object o) => WeatherShared.Handlers.MemCacheHandler.Instance.SetItem<object>(key, o);

        }

        public class LatLong
        {
            //public bool HasValue { return false; }
            const string csvEncryptedLatLongName = "LatLong";
            public static double Lat
            {
                get
                {
                    string value = WeatherShared.Handlers.AppSettingsHandler.ReadSettingEncrypted(csvEncryptedLatLongName);
                    if (value != null) return double.Parse(value.Split(',')[0].Replace(",", string.Empty));
                    return 0;
                }
            }
            public static double Lng
            {
                get
                {
                    string value = WeatherShared.Handlers.AppSettingsHandler.ReadSettingEncrypted(csvEncryptedLatLongName);
                    if (value != null) return double.Parse(value.Split(',')[1].Replace(",", string.Empty));
                    return 0;
                }
            }

            public static bool HasRecord() { return (!string.IsNullOrWhiteSpace(WeatherShared.Handlers.AppSettingsHandler.ReadSettingEncrypted(csvEncryptedLatLongName))); }
            public static void Set(double dLat, double dLng)
            {
                WeatherShared.Handlers.AppSettingsHandler.AddupdateAppSettingsEncrypted(csvEncryptedLatLongName, string.Join(",", dLat, dLng));
            }

        }

        #region Web Request
        [Obsolete("Call the Handler.WebReqeustHandler direct")]
        public static string CompressedCallSite(string Url) => WeatherShared.Handlers.WebRequestHandler.Instance.CallSite(Url);
        [Obsolete("Call the Handler.WebReqeustHandler direct")]
        public static string CompressedCallSite(string Url, string UserAgent) => WeatherShared.Handlers.WebRequestHandler.Instance.CallSite(Url, UserAgent);



        #endregion

        [Obsolete("Please use the Extentions DateTime.BetweenTimespans")]
        public static bool BetweenTimespans(TimeSpan test, TimeSpan LowerValue, TimeSpan Highervalue)
            => test.BetweenTimespans(LowerValue, Highervalue);

        [Obsolete("Use Extentions dictionary<string, string>.CompileDebug")]
        public static string CompileDebug(System.Collections.Generic.Dictionary<string, string> ItemsTodisplay)
            => ItemsTodisplay.CompileDebug();

        /// <summary>
        /// Bitarray should be under 32 bits
        /// </summary>
        /// <param name="Array"></param>
        /// <returns></returns>
        [Obsolete("Use Extentions BitArray.ToInt")]
        public static int ConvertBitarrayToInt(BitArray Array) => Array.ToInt();

        [Obsolete("Use Extentions int.ToBitArray")]
        public static BitArray ConverTIntToBitArray(int item) => item.ToBitArray();


    }
}
