using System;
using WeatherDesktop.Shared;
using System.Windows.Forms;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace WeatherDesktop.Interface
{
    class WeatherGovLatLong: ILatLongInterface
    {

        double _lat;
        double _long;
        bool _Status = false;

        public WeatherGovLatLong()
        {
            try
            {
                string Zip = Shared.ReadSettingEncrypted(SystemLevelConstants.ZipCode);
                if (string.IsNullOrEmpty(Zip)) { MessageBox.Show("Zip not found"); }
                else
                    {
                    string Results;
                    WeatherGovService.ndfdXMLPortTypeClient client = new WeatherGovService.ndfdXMLPortTypeClient();
                    using (new OperationContextScope(client.InnerChannel))
                    {
                        // Add a HTTP Header to an outgoing request
                        HttpRequestMessageProperty requestMessage = new HttpRequestMessageProperty();
                        requestMessage.Headers["User-Agent"] = "WeatherWallpaper/v1.0 (https://github.com/liukonen/WeatherWallpaper/; liukonen@gmail.com)";
                        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = requestMessage;
                       var Res = client.LatLonListZipCode(Zip);
                        Results = Res.ToString();
                    }

                    string[] Items = Results.Split(' ');
                    _lat = double.Parse(Items[0]);
                    _long = double.Parse(Items[1]);
                    _Status = true;
                }
            }
            catch (Exception x){ MessageBox.Show(x.Message); _Status = false; }


        }

        public double Latitude()
        {
            return _lat;
        }

        public double Longitude()
        {
            return _long;
        }

        public bool worked()
        {
            return _Status;
        }
    }
}
