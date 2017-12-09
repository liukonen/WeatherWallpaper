using System;
using System.Windows.Forms;
using WeatherDesktop.Interface;
using System.ComponentModel.Composition;
using WeatherDesktop.Share;

namespace ExternalService.template
{
    [Export(typeof(ISharedWeatherinterface))]
    [ExportMetadata("ClassName", "C# External Templete")]
    public class WeatherSource : ISharedWeatherinterface
    {
        string ISharedInterface.Debug()
        {
            //SharedObjects.CompileDebug - will take a dictionary and convert it to an array of key: Value strings
            return "throw new NotImplementedException();";
        }

        ISharedResponse ISharedInterface.Invoke()
        {
            /* Sample code
            if (SharedObjects.Cache.Exists("key")) { return (WeatherResponse)SharedObjects.Cache.Value("key"); }
            string response = SharedObjects.CompressedCallSite("url");
            WeatherResponse responseObject = Transform(response);
            SharedObjects.Cache.Set("key", responseObject, 60);
            return responseObject;
            */
            return new WeatherResponse();
        }

        void ISharedInterface.Load()
        {
            //Acts like the class' create method, to prevent lazy load from creating new objects for no reason
        }

        MenuItem[] ISharedInterface.SettingsItems()
        {
            //Most weather objects need a zipcode
            //SharedObjects.ZipObjects.ZipMenuItem - provides an easy way to edit zip records after load
            //SharedObjects.ZipObjects.TryGetZip - provides a popup if the zip isn't loaded and saves, otherwise will provide zip thats saved
            //SharedObjects.ZipObjects.GetZip will generate the popup to enter zip. will NOT provide zip if already loaded          
            return new MenuItem[] { };
        }

        Exception ISharedInterface.ThrownException()
        {
            throw new NotImplementedException();
        }
    }
}
