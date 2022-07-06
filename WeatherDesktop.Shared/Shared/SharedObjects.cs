using System;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using WeatherDesktop.Shared.Handlers;
using WeatherDesktop.Shared.Extentions;

namespace WeatherDesktop.Share
{
    public static class SharedObjects
    {
        public enum WeatherTypes { ThunderStorm, Rain, Snow, Dust, Fog, Haze, Smoke, Windy, Frigid, Cloudy, PartlyCloudy, Clear, Hot };

        #region VB Input boxes
        [Obsolete("Moved to InputHandler.")]
        public static string InputBox(string Prompt) => InputHandler.InputBox(Prompt);
        [Obsolete("Moved to InputHandler.")]
        public static string InputBox(string Prompt, string Title) => InputHandler.InputBox(Prompt, Title);
        [Obsolete("Moved to InputHandler.")]
        public static string InputBox(string Prompt, string Title, string DefaultResponse) { return Microsoft.VisualBasic.Interaction.InputBox(Prompt, Title, DefaultResponse); }
        #endregion

        [Obsolete("Moved to ZipcodeHandler.")]
        public static class ZipObjects
        {
            [Obsolete("Moved to ZipcodeHandler.")]
            public static MenuItem ZipMenuItem => ZipcodeHandler.ZipMenuItem;
            [Obsolete("Moved to ZipcodeHandler.")]
            public static void ChangeZipClick(object sender, EventArgs e) => ZipcodeHandler.ChangeZipClick(sender, e);
            [Obsolete("Moved to ZipcodeHandler.")]
            public static string GetZip() => ZipcodeHandler.GetZip();
            [Obsolete("Moved to ZipcodeHandler.")]
            public static string Rawzip => ZipcodeHandler.Rawzip;
            [Obsolete("Moved to ZipcodeHandler.")]
            public static string TryGetZip() => ZipcodeHandler.TryGetZip();

        }

        public static class AppSettings
        {
            #region App Config (No Encryption)
            [Obsolete("Moved to AppSettingsHandler.")]
            public static void AddUpdateAppSettings(string key, string value)
                => AppSetttingsHandler.Write(key, value);
            [Obsolete("Moved to AppSettingsHandler.")]
            public static void RemoveAppSetting(string key)
                => AppSetttingsHandler.Remove(key);
            [Obsolete("Moved to AppSettingsHandler.")]
            public static string ReadSetting(string key)
                => AppSetttingsHandler.Read(key);

            #endregion

            #region App Config (Encrypted)
            [Obsolete("Moved to EncryptedAppSettingsHandler.")]
            public static void AddupdateAppSettingsEncrypted(string key, string value)
                => EncryptedAppSettingsHandler.Write(key, value);
            [Obsolete("Moved to EncryptedAppSettingsHandler.")]
            public static string ReadSettingEncrypted(string key) 
                => EncryptedAppSettingsHandler.Read(key);


            #endregion
        }

        public static class Cache
        {
            [Obsolete("Please use generic GetValue.")]
            public static bool Exists(string key) => MemCacheHandler.Instance.Exists(key);
          
            [Obsolete("Please use generic GetValue.")]
            public static object Value(string key)
            {
                if (MemCacheHandler.Instance.Exists(key)) return MemCacheHandler.Instance.GetItem<object>(key);
                return string.Empty;
            }
            [Obsolete("Please use Generic GetValue.")]
            public static string StringValue(string key)=> MemCacheHandler.Instance.GetItem<string>(key);

            [Obsolete("Please use Generic SetValue.")]
            public static void Set(string key, object o, int timeout) => MemCacheHandler.Instance.SetItem<object>(key, o, timeout);

            [Obsolete("Please use Generic SetValue.")]
            public static void Set(string key, object o) => MemCacheHandler.Instance.SetItem<object>(key, o);

        }

        public class LatLong
        {
            [Obsolete("Moved to LatLongHandler.")]
            public static double Lat => LatLongHandler.Lat;
            [Obsolete("Moved to LatLongHandler.")]
            public static double Lng => LatLongHandler.Lng;
            [Obsolete("Moved to LatLongHandler.")]
            public static bool HasRecord() => LatLongHandler.HasRecord();
            [Obsolete("Moved to LatLongHandler.")]
            public static void Set(double dLat, double dLng)=>LatLongHandler.Set(dLat, dLng);

        }

        #region Web Request
        [Obsolete("Moved to WebHandler.CallSite.")]
        public static string CompressedCallSite(string Url) => WebHandler.Instance.CallSite(Url);
        [Obsolete("Moved to WebHandler.CallSite.")]
        public static string CompressedCallSite(string Url, string UserAgent) => WebHandler.Instance.CallSite(Url, UserAgent);

        #endregion
        [Obsolete("Moved to Extentions")]
        public static bool BetweenTimespans(TimeSpan test, TimeSpan LowerValue, TimeSpan Highervalue) => test.Between(LowerValue, Highervalue);
        [Obsolete("Moved to Extentions")]
        public static string CompileDebug(Dictionary<string, string> ItemsTodisplay) => ItemsTodisplay.CompileDebug();

        [Obsolete("Moved to Extentions")]
        public static int ConvertBitarrayToInt(BitArray Array) => Array.ToInt();
        [Obsolete("Moved to Extentions")]
        public static BitArray ConverTIntToBitArray(int item) => item.ToBitArray();
        

    }
}
