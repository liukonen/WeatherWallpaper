using System;


namespace WeatherDesktop.Services.Internal.LatLongFlatFile
{
    internal class ZipRowItem
    {
        public string Zipcode = string.Empty;
        //public string CityName = string.Empty;
        //public string State = string.Empty;
        public double Latitude = 0;
        public double Longitude = 0;
        //public short Timezone = 0;
        //public Boolean DaylightSavings = false;

        public ZipRowItem(string item)
        {
            const string Invalid = "Zip";
            if (!item.StartsWith(Invalid) && !string.IsNullOrWhiteSpace(item))
            {
                //Example string 71937;Cove;AR;34.398483;-94.39398;-6;1;34.398483,-94.39398
                string[] items = item.Split(';');
                Zipcode = items[0];
                //CityName = items[1];
                //State = items[2];
                Latitude = double.Parse(items[3]);
                Longitude = double.Parse(items[4]);
                //Timezone = short.Parse(items[5]);
                //DaylightSavings = (items[6] == "1");
            }
        }
    }
}
