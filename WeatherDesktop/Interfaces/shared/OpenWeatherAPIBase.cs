using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using Microsoft.VisualBasic;
using WeatherDesktop.Shared;
namespace WeatherDesktop.Interface
{
    internal class OpenWeatherAPIBase :ISharedInterface
    {
        const String apiCall = "http://api.openweathermap.org/data/2.5/weather?zip={0}&appid={1}&units=imperial";//&units=imperial //&units=metric
        const string ClassName = "OpenWeatherMap";
        #region Globals
        string _apiKey;
        string _zip = string.Empty;
        //Coord _LatLong;
        int _updateInt = 0;
        //private WeatherResponse _cacheValue = new WeatherResponse();
        private OpenWeatherMapObject _Cache;
        private Boolean HasBeenCalled = false;
        private DateTime _lastCall;
        private string _Status;
        #endregion

       // public Coord LatLong{ get { return _LatLong; } }
        public OpenWeatherMapObject Response
        { get { return _Cache; } }

        public string Status
        { get { return _Status; } }

        public virtual ISharedResponse Invoke() //this will be overridden
        {
            if (!HasBeenCalled || DateTime.Now > _lastCall.AddMinutes(UpdateInterval))
            {
                try
                {
                     LiveCall();
                    HasBeenCalled = true;
                    _lastCall = DateTime.Now;
                }
                catch (Exception ex) { _Status = ex.Message; }

            }
            return new WeatherResponse(); //empty dumb value
        }

        private void LiveCall()
        {
            try
            {
                string url = string.Format(apiCall, ZipCode, APIKey);
                string value = Shared.CompressedCallSite(url);
                JavaScriptSerializer jsSerialization = new JavaScriptSerializer();
                OpenWeatherMapObject weatherObject = jsSerialization.Deserialize<OpenWeatherMapObject>(value);
                _Cache = weatherObject;
            }
            catch (Exception x) { _Status = x.Message; }

            //return new ISharedResponse();
        }

        public virtual string Debug()
        {
            return _Status;
        }

        public MenuItem[] SettingsItems()
        {
            List<MenuItem> Settings = new List<MenuItem>();
            Settings.Add(new MenuItem("API key", ChangeAPI));
            Settings.Add(new MenuItem("zip Code", ChangeZipClick));
            Settings.Add(new MenuItem("Update Interval", Enterinterval));
            return Settings.ToArray();

        }

        public OpenWeatherAPIBase()
            {}

        #region Properties
        public string APIKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_apiKey)) { _apiKey = Interface.Shared.ReadSettingEncrypted(ClassName + ".APIKey"); }
                return _apiKey;
            }
            set
            {
                _apiKey = value;
                Interface.Shared.AddupdateAppSettingsEncrypted(ClassName + ".APIKey", _apiKey);
            }
        }

        public string ZipCode
        {
            get
            {
                if (string.IsNullOrEmpty(_zip)) { _zip = Shared.ReadSettingEncrypted(SystemLevelConstants.ZipCode); }
                return _zip;
            }

            set
            {
                int dumbyvalidator;
                if (int.TryParse(value, out dumbyvalidator))
                {
                    _zip = value;
                    Shared.AddupdateAppSettingsEncrypted(SystemLevelConstants.ZipCode, _zip);
                }
                else { MessageBox.Show("invalid Zip code"); }

            }
        }

       public int UpdateInterval
        {
            get
            {

                if (_updateInt == 0) { int.TryParse(Shared.ReadSetting(ClassName + ".UpdateInt"), out _updateInt); }
                if (_updateInt < 10) { _updateInt = 10; }
                return _updateInt;
            }
            set
            {
                if (value > 10 && value < 120)
                {
                    _updateInt = value;
                    Shared.AddUpdateAppSettings(ClassName + ".UpdateInt", _updateInt.ToString());

                }
                else { MessageBox.Show("Please enter a number between 10 and 120"); }
            }
        }


        public void Enterzip()
        {
            ZipCode = Interaction.InputBox("Enter New Zip Code", "Zip code", ZipCode.ToString());
        }

        public void EnterAPIKey()
        {
            APIKey = Interaction.InputBox("Please enter the API key provided by openweathermap.org", "Enter API");

        }
        #endregion

        #region Events
        private void ChangeZipClick(object sender, EventArgs e)
        {
            Enterzip();
        }

        private void ChangeAPI(object sender, EventArgs e)
        {
            EnterAPIKey();
        }

        private void Enterinterval(object sender, EventArgs e)
        {
            int DumbyValue = 60;
            if (int.TryParse(Interaction.InputBox("Enter New interval between 10 and 120", "Update Interval Minutes", UpdateInterval.ToString()), out DumbyValue))
            {
                UpdateInterval = DumbyValue;
            }
        }
        #endregion

        #region Auto Generated Code
        //------------------------------------------------------------------------------
        // <auto-generated>
        //     This code was generated by a tool.
        //     Runtime Version:4.0.30319.42000
        //
        //     Changes to this file may cause incorrect behavior and will be lost if
        //     the code is regenerated.
        // </auto-generated>
        //------------------------------------------------------------------------------



        // Type created for JSON at <<root>>
        [System.Runtime.Serialization.DataContractAttribute()]
        public partial class OpenWeatherMapObject
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public Coord coord;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public Sys sys;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public Weather[] weather;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string @base;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public Main main;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public Wind wind;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public Clouds clouds;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public int dt;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public int id;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string name;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public int cod;
        }

        // Type created for JSON at <<root>> --> coord
        [System.Runtime.Serialization.DataContractAttribute(Name = "coord")]
        public partial class Coord
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double lon;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double lat;
        }

        // Type created for JSON at <<root>> --> sys
        [System.Runtime.Serialization.DataContractAttribute(Name = "sys")]
        public partial class Sys
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public int type;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public int id;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double message;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string country;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public int sunrise;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public int sunset;
        }

        // Type created for JSON at <<root>> --> weather
        [System.Runtime.Serialization.DataContractAttribute(Name = "weather")]
        public partial class Weather
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public int id;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string main;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string description;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public string icon;
        }

        // Type created for JSON at <<root>> --> main
        [System.Runtime.Serialization.DataContractAttribute(Name = "main")]
        public partial class Main
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double temp;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public int humidity;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double pressure;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double temp_min;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double temp_max;
        }

        // Type created for JSON at <<root>> --> wind
        [System.Runtime.Serialization.DataContractAttribute(Name = "wind")]
        public partial class Wind
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double speed;

            [System.Runtime.Serialization.DataMemberAttribute()]
            public double deg;
        }

        // Type created for JSON at <<root>> --> clouds
        [System.Runtime.Serialization.DataContractAttribute(Name = "clouds")]
        public partial class Clouds
        {

            [System.Runtime.Serialization.DataMemberAttribute()]
            public int all;
        }
        #endregion

    }
}
/*
 * 
 * api.openweathermap.org/data/2.5/weather?zip=94040,us
 {"coord":{"lon":-122.09,"lat":37.39},
"sys":{"type":3,"id":168940,"message":0.0297,"country":"US","sunrise":1427723751,"sunset":1427768967},
"weather":[{"id":800,"main":"Clear","description":"Sky is Clear","icon":"01n"}],
"base":"stations",
"main":{"temp":285.68,"humidity":74,"pressure":1016.8,"temp_min":284.82,"temp_max":286.48},
"wind":{"speed":0.96,"deg":285.001},
"clouds":{"all":0},
"dt":1427700245,
"id":0,
"name":"Mountain View",
"cod":200}



    Group 2xx: Thunderstorm
ID	Meaning	
200	thunderstorm with light rain
201	thunderstorm with rain	 
202	thunderstorm with heavy rain	 
210	light thunderstorm	 
211	thunderstorm	 
212	heavy thunderstorm	 
221	ragged thunderstorm	 
230	thunderstorm with light drizzle	 
231	thunderstorm with drizzle	 
232	thunderstorm with heavy drizzle	 

    Group 3xx: Drizzle
ID	Meaning	Icon
300	light intensity drizzle	 
301	drizzle	 
302	heavy intensity drizzle	 
310	light intensity drizzle rain	 
311	drizzle rain	 
312	heavy intensity drizzle rain	 
313	shower rain and drizzle	 
314	heavy shower rain and drizzle	 
321	shower drizzle	 

    Group 5xx: Rain
ID	Meaning	
500	light rain	 
501	moderate rain	 
502	heavy intensity rain	 
503	very heavy rain	 
504	extreme rain	 
511	freezing rain	 
520	light intensity shower rain	 
521	shower rain	 09d
522	heavy intensity shower rain	 
531	ragged shower rain	 

    Group 6xx: Snow
ID	Meaning	
600	light snow	 
601	snow	 
602	heavy snow	 
611	sleet	 
612	shower sleet	 
615	light rain and snow	 
616	rain and snow	 
620	light shower snow	 
621	shower snow	 
622	heavy shower snow	 

Group 7xx: Atmosphere
ID	Meaning	Icon
701	mist	 
711	smoke	 
721	haze	 
731	sand, dust whirls	 
741	fog	 
751	sand	 
761	dust	 
762	volcanic ash	 
771	squalls	 
781	tornado	 

Group 800: Clear
ID	Meaning	
800	clear sky	 

Group 80x: Clouds
ID	Meaning	
801	few clouds	   
802	scattered clouds	   
803	broken clouds	
804	overcast clouds

Group 90x: Extreme
ID	Meaning
900	tornado
901	tropical storm
902	hurricane
903	cold
904	hot
905	windy
906	hail

Group 9xx: Additional
ID	Meaning
951	calm
952	light breeze
953	gentle breeze
954	moderate breeze
955	fresh breeze
956	strong breeze
957	high wind, near gale
958	gale
959	severe gale
960	storm
961	violent storm
962	hurricane
     */
