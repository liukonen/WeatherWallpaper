using System;
using System.ComponentModel.Composition;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using WeatherDesktop.Interface;
using WeatherDesktop.Share;
namespace InternalService
{
    [Export(typeof(WeatherDesktop.Interface.ILatLongInterface))]
    [ExportMetadata("ClassName", "YahooLatLong")]
    class YahooLatLong : ILatLongInterface
    {
        Double _lat;
        Double _Long;
        Boolean _worked = false;

        public YahooLatLong()
        {
            try
            {
                string Zip = SharedObjects.ZipObjects.TryGetZip();
                string url = string.Format(Properties.Resources.Yahoo_LatLong_Url, Zip);
                string results = SharedObjects.CompressedCallSite(url);
                JavaScriptSerializer jsSerialization = new JavaScriptSerializer();
                YahooLatLongObject Response = jsSerialization.Deserialize<YahooLatLongObject>(results);
                _lat = double.Parse(Response.Query.Results.Place.Centroid.Latitude);
                _Long = double.Parse(Response.Query.Results.Place.Centroid.Longitude);
                _worked = true;
            }
            catch (Exception x) { MessageBox.Show(Properties.Resources.warning_LatLong + x.Message); _worked = false; }
        }

        public double Latitude(){return _lat;}

        public double Longitude(){return _Long;}

        public bool worked(){return _worked;}


        #region Auto Generated code
        //taken from https://jsonutils.com/

        [DataContract]
        public class Centroid
        {

            [DataMember(Name = "latitude")]
            public string Latitude { get; set; }

            [DataMember(Name = "longitude")]
            public string Longitude { get; set; }
        }

        [DataContract]
        public class Place
        {

            [DataMember(Name = "centroid")]
            public Centroid Centroid { get; set; }
        }

        [DataContract]
        public class Results
        {

            [DataMember(Name = "place")]
            public Place Place { get; set; }
        }

        [DataContract]
        public class Query
        {

            [DataMember(Name = "count")]
            public int Count { get; set; }

            [DataMember(Name = "created")]
            public DateTime Created { get; set; }

            [DataMember(Name = "lang")]
            public string Lang { get; set; }

            [DataMember(Name = "results")]
            public Results Results { get; set; }
        }

        [DataContract]
        public class YahooLatLongObject
        {

            [DataMember(Name = "query")]
            public Query Query { get; set; }
        }
        #endregion
    }
}
