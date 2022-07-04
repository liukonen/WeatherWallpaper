using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherShared.Handlers
{
    public class LatLongHandler
    {

        //public bool HasValue { return false; }
        const string csvEncryptedLatLongName = "LatLong";
        public static double Lat
        {
            get
            {
                string value = AppSettingsHandler.ReadSettingEncrypted(csvEncryptedLatLongName);
                if (value != null) return double.Parse(value.Split(',')[0].Replace(",", string.Empty));
                return 0;
            }
        }
        public static double Lng
        {
            get
            {
                string value = AppSettingsHandler.ReadSettingEncrypted(csvEncryptedLatLongName);
                if (value != null) return double.Parse(value.Split(',')[1].Replace(",", string.Empty));
                return 0;
            }
        }

        public static bool HasRecord() { return (!string.IsNullOrWhiteSpace(AppSettingsHandler.ReadSettingEncrypted(csvEncryptedLatLongName))); }
        public static void Set(double dLat, double dLng)
        {
            AppSettingsHandler.AddupdateAppSettingsEncrypted(csvEncryptedLatLongName, string.Join(",", dLat, dLng));
        }

    }
}
