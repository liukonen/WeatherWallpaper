using System;
using System.Xml;
using WeatherDesktop.Share;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;

namespace ExternalService
{
    [Export(typeof(ILatLongInterface))]
    [ExportMetadata("ClassName", "GovWeatherLatLong")]
    class GovWeatherLatLong: ILatLongInterface
    {

        private readonly bool _worked;
        private readonly string Cache;

        public GovWeatherLatLong()
        {
            try
            {
                var Zip = SharedObjects.ZipObjects.Rawzip;
                if (string.IsNullOrEmpty(Zip)) { Zip = SharedObjects.ZipObjects.GetZip(); }
                var url = string.Format(Properties.Resources.Gov_LatLong_Url, Zip);
                var results = SharedObjects.CompressedCallSite(url, Properties.Resources.Gov_User);
                var reader = XmlReader.Create(new System.IO.StringReader(results));
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

        public double Latitude() => double.Parse(Cache.Split(',')[0].Replace(",", string.Empty));

        public double Longitude() => double.Parse(Cache.Split(',')[1].Replace(",", string.Empty));

        public bool worked() => _worked;


    }
}

