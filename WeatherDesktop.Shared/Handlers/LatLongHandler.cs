

namespace WeatherDesktop.Shared.Handlers
{
    public class LatLongHandler
    {
        //public bool HasValue { return false; }
        const string csvEncryptedLatLongName = "LatLong";
        public static double Lat
        {
            get
            {
                var value = EncryptedAppSettingsHandler.Read(csvEncryptedLatLongName);
                return (value != null) 
                    ? double.Parse(value.Split(',')[0].Replace(",", string.Empty)) 
                    : 0;
            }
        }
        public static double Lng
        {
            get
            {
                var value = EncryptedAppSettingsHandler.Read(csvEncryptedLatLongName);
                return (value != null) 
                    ? double.Parse(value.Split(',')[1].Replace(",", string.Empty))
                    : 0;
            }
        }

        public static bool HasRecord() => 
            (!string.IsNullOrWhiteSpace(EncryptedAppSettingsHandler.Read(csvEncryptedLatLongName))); 
        public static void Set(double dLat, double dLng)
        {
            EncryptedAppSettingsHandler.Write(csvEncryptedLatLongName, string.Join(",", dLat, dLng));
        }
    }
}
