using WeatherDesktop.Share;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;
using System.Linq;
using System.IO;
using WeatherDesktop.Shared.Handlers;

namespace WeatherDesktop.Services
{
    [Export(typeof(ILatLongInterface))]
    [ExportMetadata("ClassName", "OpenDataFlatFileLookup")]
    public class OpenDataFlatFileLookup : ILatLongInterface
    {
        const string FileLocation = ".\\Services\\resources\\us-zip-code-latitude-and-longitude.csv";
        private  bool _worked;
        private Geography Geography;

        public double Latitude() => Geography.Latitude;
        
        public double Longitude() => Geography.Longitude;
  
        public bool worked() => _worked;

        public OpenDataFlatFileLookup()
        {
            if (File.Exists(FileLocation))
            {
                string Zip = ZipcodeHandler.Rawzip;
                if (string.IsNullOrEmpty(Zip)) { Zip = ZipcodeHandler.GetZip(); }

                Geography = (from string item
                         in File.ReadLines(FileLocation)
                         let Z = new Internal.LatLongFlatFile.ZipRowItem(item)
                         where Z.Zipcode == Zip
                         select new Geography(Z.Latitude, Z.Longitude)).First();
                _worked = true;
            }
            else
            {
                _worked = false;
            }
        }
    }
}
