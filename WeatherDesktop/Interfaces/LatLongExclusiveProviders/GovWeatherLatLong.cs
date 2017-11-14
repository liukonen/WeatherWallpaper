using System;
using System.Xml;
using WeatherDesktop.Shared;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;


namespace WeatherDesktop.Interface
{
    class GovWeatherLatLong: ILatLongInterface
    {
        const string curl = "https://graphical.weather.gov/xml/sample_products/browser_interface/ndfdXMLclient.php?listZipCodeList={0}";
        private bool _worked;
        private string debug;
        private string Cache;
        public double Latitude()
        {
            string[] LatLong = Cache.Split(',');
            return double.Parse(LatLong[0].Replace(",", string.Empty));
        }


        public GovWeatherLatLong()
        {
            try
            {
                string Zip = Shared.ReadSettingEncrypted(SystemLevelConstants.ZipCode);
                if (string.IsNullOrEmpty(Zip))
                {
                    Shared.ChangeZipClick(null, new EventArgs());
                    Zip = Shared.ReadSettingEncrypted(SystemLevelConstants.ZipCode);
                }
                string url = string.Format(curl, Zip);
                string results = Shared.CompressedCallSiteSpoofBrowser(url);
                XmlReader reader = XmlReader.Create(new System.IO.StringReader(results));
                while (reader.Read())
                {
                    if ((reader.NodeType == XmlNodeType.Element))
                        switch (reader.Name)
                        {
                            case "latLonList":
                                Cache = reader.ReadInnerXml();
                                break;
                        }
                }
                _worked = true;
            }
            catch (Exception x) { _worked = false; MessageBox.Show(x.Message); }
        }

        public double Longitude()
        {
            string[] LatLong = Cache.Split(',');
            return double.Parse(LatLong[1].Replace(",", string.Empty));

        }

        public bool worked()
        {
            return _worked;
        }


    }
}

