using System;
using System.Xml;
using WeatherDesktop.Share;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;

namespace WeatherDesktop.Services.External
{
    [Export(typeof(ILatLongInterface))]
    [ExportMetadata("ClassName", "GovWeatherLatLong")]
    class GovWeatherLatLong : ILatLongInterface
    {

        private readonly bool _worked;
        //private readonly string Cache;
        private Geography geography;

        public GovWeatherLatLong()
        {
            try
            {
                var Zip = SharedObjects.ZipObjects.Rawzip;
                if (string.IsNullOrEmpty(Zip)) { Zip = SharedObjects.ZipObjects.GetZip(); }
                var url = string.Format(Properties.Gov2.Gov_LatLong_Url, Zip);
                var results = SharedObjects.CompressedCallSite(url, Properties.Gov2.Gov_User);
                var reader = XmlReader.Create(new System.IO.StringReader(results));
                while (reader.Read())
                {
                    if ((reader.NodeType == XmlNodeType.Element))
                        switch (reader.Name)
                        {
                            case "latLonList":
                                geography = ReadGeo(reader.ReadInnerXml());
                                break;
                        }
                }
                _worked = true;
            }
            catch (Exception x) { _worked = false; MessageBox.Show(x.Message); }
        }

        private Geography ReadGeo(string item) 
            => new Geography(
                double.Parse(item.Split(',')[0]), 
                double.Parse(item.Split(',')[1]));

        public double Latitude() => geography.Latitude;

        public double Longitude() => geography.Longitude;

        public bool worked() => _worked;
    }
}
