using System;
using System.Xml;
using WeatherDesktop.Share;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;
using WeatherDesktop.Shared.Handlers;

namespace WeatherDesktop.Services.External
{
    [Export(typeof(ILatLongInterface))]
    [ExportMetadata("ClassName", "GovWeatherLatLong")]
    class GovWeatherLatLong : ILatLongInterface
    {
        const string LatLongUrl = "https://graphical.weather.gov/xml/sample_products/browser_interface/ndfdXMLclient.php?listZipCodeList={0}";


        private readonly bool _worked;
        //private readonly string Cache;
        private Geography geography;

        public GovWeatherLatLong()
        {
            try
            {
                var Zip = EncryptedAppSettingsHandler.zipcode;
                if (string.IsNullOrEmpty(Zip)) { Zip = ZipcodeHandler.GetZip(); }
                var url = string.Format(LatLongUrl, Zip);
                var results = WebHandler.Instance.CallSite(url, Properties.Resources.Gov2User);
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

        private static Geography ReadGeo(string item) => new Geography(
                double.Parse(item.Split(',')[0]), 
                double.Parse(item.Split(',')[1]));

        public double Latitude() => geography.Latitude;

        public double Longitude() => geography.Longitude;

        public bool worked() => _worked;
    }
}
