

namespace WeatherDesktop.Shared.Handlers
{
    public class LatLongHandler
    {
        public static double Lat
        {
            get
            {
                var value = EncryptedAppSettingsHandler.LatLong;
                return (value != null) 
                    ? double.Parse(value.Split(',')[0].Replace(",", string.Empty)) 
                    : 0;
            }
        }
        public static double Lng
        {
            get
            {
                var value = EncryptedAppSettingsHandler.LatLong;
                return (value != null) 
                    ? double.Parse(value.Split(',')[1].Replace(",", string.Empty))
                    : 0;
            }
        }

        public static bool HasRecord() => 
            (!string.IsNullOrWhiteSpace(EncryptedAppSettingsHandler.LatLong)); 
        public static void Set(double dLat, double dLng)
        {
            EncryptedAppSettingsHandler.LatLong = string.Join(",", dLat, dLng);
        }
    }
}
