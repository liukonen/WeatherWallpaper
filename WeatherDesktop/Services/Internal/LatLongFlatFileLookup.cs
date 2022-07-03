using System;
using System.Xml;
using WeatherDesktop.Share;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;
using System.Linq;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace WeatherDesktop.Services
{
    [Export(typeof(ILatLongInterface))]
    [ExportMetadata("ClassName", "OpenDataFlatFileLookup")]
    public class OpenDataFlatFileLookup : ILatLongInterface
    {
        private  bool _worked;
        private readonly string Cache;

        public double Latitude()
        {
            string[] LatLong = Cache.Split(',');
            return double.Parse(LatLong[0].Replace(",", string.Empty));
        }

        public double Longitude()
        {
            string[] LatLong = Cache.Split(',');
            return double.Parse(LatLong[1].Replace(",", string.Empty));
        }

        public bool worked() => _worked;

        public OpenDataFlatFileLookup()
        {
            if (File.Exists(".\\Services\\resources\\us-zip-code-latitude-and-longitude.csv"))
            {
                string Zip = SharedObjects.ZipObjects.Rawzip;
                if (string.IsNullOrEmpty(Zip)) { Zip = SharedObjects.ZipObjects.GetZip(); }

                Cache = (from string item
                         in File.ReadLines(".\\Services\\resources\\us-zip-code-latitude-and-longitude.csv")
                         let Z = new ZipRowItem(item)
                         where Z.Zipcode == Zip
                         select string.Join(",", Z.Latitude, Z.Longitude)).First();
                _worked = true;
            }
            else
            {
                _worked = false;
            }
        }


        private class ZipRowItem
        {
            public string Zipcode = string.Empty;
            public string CityName = string.Empty;
            public string State = string.Empty;
            public double Latitude = 0;
            public double Longitude = 0;
            public short Timezone = 0;
            public Boolean DaylightSavings = false;

            public ZipRowItem(string item)
            {
                if (!item.StartsWith("Zip") && !string.IsNullOrWhiteSpace(item))
                {
                    //Example string 71937;Cove;AR;34.398483;-94.39398;-6;1;34.398483,-94.39398
                    string[] items = item.Split(';');
                    Zipcode = items[0];
                    CityName = items[1];
                    State = items[2];
                    Latitude = double.Parse(items[3]);
                    Longitude = double.Parse(items[4]);
                    Timezone = short.Parse(items[5]);
                    DaylightSavings = (items[6] == "1");
                }
            }
        }

    }


}
