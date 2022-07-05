using System;
using System.Collections.Generic;
using System.Windows.Forms;
using WeatherDesktop.Share;
using WeatherDesktop.Interface;
using Newtonsoft.Json;
using WeatherDesktop.Services.External.OpenWeather.Objects;
using WeatherDesktop.Shared.Handlers;

namespace WeatherDesktop.Services.External.OpenWeather
{
    internal class OpenWeatherAPIBase : ISharedInterface
    {
        const string c_URL = "http://api.openweathermap.org/data/2.5/weather?zip={0}&amp;appid={1}&amp;units=imperial";

        //&units=imperial //&units=metric
        const string ClassName = "OpenWeatherMap";
        #region Globals
        string _apiKey;
        string _zip = string.Empty;
        int _updateInt = 0;
        private OpenWeatherMapObject _Cache;
        private Boolean HasBeenCalled = false;
        private DateTime _lastCall;
        private string _Status;
        private Exception _ThrownException = null;
        private readonly string UpdateIntervalName = $"{ClassName}.UpdateInt";
        private readonly string APIKeyName = $"{ClassName}.APIKey";
        #endregion

        public Exception ThrownException() { return _ThrownException; }

        public OpenWeatherMapObject Response
        { get { return _Cache; } }

        public string Status
        { get { return _Status; } }


        private string GetValue
        {
            get
            {
                if (SharedObjects.Cache.Exists(ClassName)) return SharedObjects.Cache.StringValue(ClassName);
                string url = string.Format(c_URL, ZipCode, APIKey);
                string value = SharedObjects.CompressedCallSite(url);
                SharedObjects.Cache.Set(ClassName, value, 60);
                return value;
            }
        }

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
                catch (Exception ex) { _Status = ex.Message; _ThrownException = ex; }

            }
            return new WeatherResponse(); //empty dumb value
        }

        private void LiveCall()
        {
            try
            {
                string value = GetValue;
                var weatherObject = JsonConvert.DeserializeObject< OpenWeatherMapObject>(value);
                _Cache = weatherObject;
                if (!SharedObjects.LatLong.HasRecord()) { SharedObjects.LatLong.Set(_Cache.Coord.Lat, _Cache.Coord.Lon); }
            }
            catch (Exception x) { _Status = x.Message; _ThrownException = x; }
        }



        public virtual string Debug()
        {
            return _Status;
        }

        public MenuItem[] SettingsItems()
        {
            return new List<MenuItem>
            {
                new MenuItem(Properties.OpenWeather.MenuAPIKey, ChangeAPI),
                SharedObjects.ZipObjects.ZipMenuItem,
                new MenuItem(Properties.OpenWeather.MenuUpdate, Enterinterval)
            }.ToArray();

        }

        public OpenWeatherAPIBase() { }
        public void Load() { }



        #region Properties
        public string APIKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_apiKey)) { _apiKey = EncryptedAppSettingsHandler.Read(APIKeyName); }
                return _apiKey;
            }
            set
            {
                _apiKey = value;
                EncryptedAppSettingsHandler.Write(APIKeyName, _apiKey);
            }
        }

        public string ZipCode
        {
            get 
            {
                if (string.IsNullOrEmpty(_zip))  _zip = SharedObjects.ZipObjects.TryGetZip(); 
                return _zip;
            }
           // set => _zip = value;
        }

        public int UpdateInterval
        {
            get
            { 
                if (_updateInt == 0) { int.TryParse(AppSetttingsHandler.Read(UpdateIntervalName), out _updateInt); }
                if (_updateInt < 10) { _updateInt = 10; }
                return _updateInt;
            }
            set
            {
                if (value > 10 && value < 120)
                {
                    _updateInt = value;
                    AppSetttingsHandler.Write(UpdateIntervalName, _updateInt.ToString());
                }
                else { MessageBox.Show(Properties.OpenWeather.UpdateIntervalMessage); }
            }
        }


        public void EnterAPIKey()
        {
            APIKey = SharedObjects.InputBox(Properties.OpenWeather.EnterAPIKeyMessage, Properties.OpenWeather.EnterAPIKeyTitle);
        }
        #endregion

        #region Events
        //private void ChangeZipClick(object sender, EventArgs e) 
        //    => SharedObjects.ZipObjects.GetZip();
        

        private void ChangeAPI(object sender, EventArgs e) => EnterAPIKey();
        

        private void Enterinterval(object sender, EventArgs e)
        {
            if (int.TryParse(SharedObjects.InputBox(
                Properties.OpenWeather.EnterIntervalMessage,
                Properties.OpenWeather.EnterIntervalTitle,
                UpdateInterval.ToString()), out int DumbyValue))
            {
                UpdateInterval = DumbyValue;
            }
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
