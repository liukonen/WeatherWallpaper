using System;
using System.Xml;
using WeatherDesktop.Shared;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;

namespace InternalService
{
    [Export(typeof(WeatherDesktop.Interface.ILatLongInterface))]
    [ExportMetadata("ClassName", "GovWeatherLatLong")]
    class GovWeatherLatLong: ILatLongInterface
    {

        private bool _worked;
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
                string Zip = Shared.Rawzip;
                if (string.IsNullOrEmpty(Zip)) { Zip = Shared.GetZip(); }
                string url = string.Format(Properties.Resources.Gov_LatLong_Url, Zip);
                string results = Shared.CompressedCallSite(url, Properties.Resources.Gov_User);
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

