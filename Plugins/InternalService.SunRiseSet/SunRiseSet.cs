using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using WeatherDesktop.Interface;
using WeatherDesktop.Share;
using InternalService;

namespace ExternalService
{
    [Export(typeof(WeatherDesktop.Interface.IsharedSunRiseSetInterface))]
    [ExportMetadata("ClassName", "InternalSunRiseSet")]
    public class InternalSunRiseSet : IsharedSunRiseSetInterface
    {
        // General
        /*The calculations in the NOAA Sunrise/Sunset and Solar Position Calculators are based on equations 
          from Astronomical Algorithms, by Jean Meeus.
          The sunrise and sunset results are theoretically accurate to within a minute for locations between
           +/- 72° latitude, and within 10 minutes outside of those latitudes.However, due to variations in 
           atmospheric composition, temperature, pressure and conditions, observed values may vary from calculations. 
      */


        #region Constants
        const string ClassName = "InternalSunRiseSet";
        const string AppProperty = "HourUpdate";
        #endregion

        #region Globals
        double _lat;
        double _long;
        DateTime _LastUpdate;
        int _HourToUpdate;
        Boolean HasUpdatedToday = false;
        SunRiseSetResponse _cache;
        Boolean _firstCall = true;
        Exception _ThrownException = null;

        #endregion

        #region Settings

        public Exception ThrownException() { return _ThrownException; }

        public MenuItem[] SettingsItems() {return new MenuItem[] { new MenuItem("Hour To Update", ChangehourToUpdate)};}


        public void TryupdateMenuItem(object sender, EventArgs e)
        {
            MenuItem Current = (MenuItem)sender;
            string Name = Current.Text;
            Type S = Type.GetType(Name);
            ILatLongInterface Item = (ILatLongInterface)Activator.CreateInstance(S);
            if (Item.worked())
            {
                _lat = Item.Latitude();
                _long = Item.Longitude();
                SharedObjects.LatLong.Set(_lat, _long);
                MessageBox.Show("Update complete");

            }
            else { MessageBox.Show("Update did not work"); }
            
            
        }
        #endregion

        #region Events
        private void ChangehourToUpdate(object sender, EventArgs e) => UpdateHour(); 
        #endregion

        #region New

        public InternalSunRiseSet(){}

        public void Load()
        {
            KeyValuePair<double, double> latlong = GetLocationProperty();
            _lat = latlong.Key;
            _long = latlong.Value;

            string HTU = SharedObjects.AppSettings.ReadSetting(AppProperty);
            if (string.IsNullOrWhiteSpace(HTU)) { HTU = "6"; }
            _HourToUpdate = int.Parse(HTU);
            _LastUpdate = DateTime.Now;
            Invoke();
        }

        #endregion

        #region invoke

        public ISharedResponse Invoke()
        {
            if (_firstCall) { _firstCall = false; _cache = LiveCall(_lat, _long); HasUpdatedToday = true; }

            if (_LastUpdate.Day != DateTime.Today.Day) { HasUpdatedToday = false; }
            if (!HasUpdatedToday && DateTime.Now.Hour == _HourToUpdate)
            {
                _cache = LiveCall(_lat, _long); HasUpdatedToday = true;
            }
            return _cache;
        }
        #endregion

        #region Live API call
        private SunRiseSetResponse LiveCall(double Latitude, double Longitude)
        {
            SunRiseSetResponse response = new SunRiseSetResponse();
            try
            {
                response.SolarNoon = DateTime.Now.Date.Add(SolarNoon);
                if (response.SolarNoon < DateTime.Now) { response.SolarNoon = response.SolarNoon.AddDays(1); }
                response.SunRise = DateTime.Now.Date.Add(SunriseTime);
                if (response.SunRise < DateTime.Now) { response.SunRise = response.SunRise.AddDays(1); }
                response.SunSet = DateTime.Now.Date.Add(SunsetTime);
                if (response.SunSet < DateTime.Now) { response.SunSet = response.SunSet.AddDays(1); }

            }
            catch (Exception x) { response.Status = x.ToString(); _ThrownException = x; }
            return response;
        }
        #endregion

        #region Helpers

        static bool IntialgetLatLong()
        {
            var worked = false;
            if (MessageBox.Show(
                InternalService.Properties.Resources.LatLongSetMessage,
                InternalService.Properties.Resources.LatLongSetHeader,
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var lat = GetDoubleWithMessage("Latitude");
                var lon = GetDoubleWithMessage("Longitude");
                SharedObjects.LatLong.Set(lat, lon);
                worked = true;
            }
            return worked;
        }

        static double GetDoubleWithMessage(string ObjectName) => double.Parse(SharedObjects.InputBox($"Please Enter your {ObjectName}", ObjectName));


        static void UpdateHour()
        {

            int current;
            try { current = int.Parse(SharedObjects.AppSettings.ReadSetting(AppProperty)); }
            catch { current = 6; }

            try
            {
                string attempt;
                attempt = SharedObjects.InputBox(InternalService.Properties.Resources.HourUpdateMessage, 
                    InternalService.Properties.Resources.HourUpdateHeader, current.ToString());
                SharedObjects.AppSettings.AddUpdateAppSettings(AppProperty, int.Parse(attempt).ToString());
            }
            catch { MessageBox.Show(InternalService.Properties.Resources.CouldNotUpdate); }
        }

        static KeyValuePair<double, double> GetLocationProperty() => 
            (SharedObjects.LatLong.HasRecord() || IntialgetLatLong()) ?
                new KeyValuePair<double, double>(SharedObjects.LatLong.Lat, SharedObjects.LatLong.Lng) :
                new KeyValuePair<double, double>(0, 0);
        


        #endregion

        #region "Heavy Math"
        public int TimeZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;

        private double JulieanDay => DateTime.Now.Date.ToOADate() + 2415018.5 + 0 - TimeZoneOffset / 24;

        private double JulieanCent => (JulieanDay - 2451545) / 36525;

        private double GeomMeanLongSun => MOD1(280.46646 + JulieanCent * (36000.76983 + JulieanCent * 0.0003032), 360);

        private double GeomMeanAnomSun => 357.52911 + JulieanCent * (35999.05029 - 0.0001537 * JulieanCent);

        private double EccentEarthOrbit => 0.016708634 - JulieanCent * (0.000042037 + 0.0000001267 * JulieanCent);

        private double SunEqofCtr => Math.Sin(RADIANS(GeomMeanAnomSun)) * (1.914602 - JulieanCent * (0.004817 + 0.000014 * JulieanCent)) + Math.Sin(RADIANS(2 * GeomMeanAnomSun)) * (0.019993 - 0.000101 * JulieanCent) + Math.Sin(RADIANS(3 * GeomMeanAnomSun)) * 0.000289;

        private double SunTrueLong => GeomMeanLongSun + SunEqofCtr;

        private double SunAppLong => SunTrueLong - 0.00569 - 0.00478 * Math.Sin(RADIANS(125.04 - 1934.136 * JulieanCent));

        private double MeanObliqEcliptic => 23 + (26 + ((21.448 - JulieanCent * (46.815 + JulieanCent * (0.00059 - JulieanCent * 0.001813)))) / 60) / 60;

        private double ObliqCorr => MeanObliqEcliptic + 0.00256 * Math.Cos(RADIANS(125.04 - 1934.136 * JulieanCent));

        private double SunDeclin => DEGREES(Math.Asin(Math.Sin(RADIANS(ObliqCorr)) * Math.Sin(RADIANS(SunAppLong))));

        private double Y => Math.Tan(RADIANS(ObliqCorr / 2)) * Math.Tan(RADIANS(ObliqCorr / 2));

        private double EqofTime => 4 * DEGREES(Y * Math.Sin(2 * RADIANS(GeomMeanLongSun)) - 2 * EccentEarthOrbit * Math.Sin(RADIANS(GeomMeanAnomSun)) + 4 * EccentEarthOrbit * Y * Math.Sin(RADIANS(GeomMeanAnomSun)) * Math.Cos(2 * RADIANS(GeomMeanLongSun)) - 0.5 * Y * Y * Math.Sin(4 * RADIANS(GeomMeanLongSun)) - 1.25 * EccentEarthOrbit * EccentEarthOrbit * Math.Sin(2 * RADIANS(GeomMeanAnomSun)));

        private double HASunrise => DEGREES(Math.Acos(Math.Cos(RADIANS(90.833)) / (Math.Cos(RADIANS(_lat)) * Math.Cos(RADIANS(SunDeclin))) - Math.Tan(RADIANS(_lat)) * Math.Tan(RADIANS(SunDeclin))));

        private double PSolarNoon => (720 - 4 * _long - EqofTime + TimeZoneOffset * 60) / 1440;

        public TimeSpan SolarNoon => DateTime.FromOADate(PSolarNoon).TimeOfDay;

        public TimeSpan SunriseTime => DateTime.FromOADate(PSolarNoon - HASunrise * 4 / 1440).TimeOfDay;

        public TimeSpan SunsetTime => DateTime.FromOADate(PSolarNoon + HASunrise * 4 / 1440).TimeOfDay;

        private double MOD1(double Number, double Divider) { return Number % Divider; }

        private double RADIANS(double angle) { return (Math.PI / 180) * angle; }

        private double DEGREES(double radians) { return radians * (180 / Math.PI); }

        #endregion

        #region Debug values
        public string Debug()
        {
            var DebugValues = new Dictionary<string, string>
            {
                {"Houre to update", _HourToUpdate.ToString()},
                {"Last update", _LastUpdate.ToString()},
                {"Latitude", _lat.ToString() },
                {"Longitude", _long.ToString() },
                {"SunRise", _cache.SunRise.ToString() },
                {"SunSet", _cache.SunSet.ToString() },
                {"Status", _cache.Status }
            };
            return SharedObjects.CompileDebug(DebugValues);
        }
        #endregion


    }



}