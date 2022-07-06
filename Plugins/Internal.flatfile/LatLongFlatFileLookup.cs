using WeatherDesktop.Share;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;
using System.Linq;
using System.IO;
using WeatherDesktop.Shared.Handlers;
using Internal.flatfile.Objects;
using System.Collections.Generic;

namespace Internal.flatfile
{
    [Export(typeof(ILatLongInterface))]
    [ExportMetadata("ClassName", "OpenDataFlatFileLookup")]
    public class OpenDataFlatFileLookup : ILatLongInterface
    {
        private bool _worked;
        private Geography Geography;

        public double Latitude() => Geography.Latitude;

        public double Longitude() => Geography.Longitude;

        public bool worked() => _worked;

        public OpenDataFlatFileLookup()
        {
            string Zip = ZipcodeHandler.Rawzip;
            if (string.IsNullOrEmpty(Zip)) { Zip = ZipcodeHandler.GetZip(); }

            Geography = (from string item
                     in ReadLines(Properties.Resources.ZipCodeItems)
                         let Z = new ZipRowItem(item)
                         where Z.Zipcode == Zip
                         select new Geography(Z.Latitude, Z.Longitude)).First();
            _worked = (Geography != null);
        }

        private static IEnumerable<string> ReadLines(string MassiveString)
        {
            string line;
            using (var reader = new StringReader(MassiveString))
                while ((line = reader.ReadLine()) != null) yield return line;
        }
    }

  
}
