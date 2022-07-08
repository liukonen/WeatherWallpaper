using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using WeatherDesktop.Share;
using WeatherDesktop.Interface;
using WeatherDesktop.Shared.Extentions;
using WeatherDesktop.Shared.Handlers;

namespace WeatherDesktop.Services.Internal
{

    [Export(typeof(IsharedSunRiseSetInterface))]
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
        #region Globals

        Geography geography;

        DateTime _LastUpdate;
        int _HourToUpdate;
        Boolean HasUpdatedToday = false;
        SunRiseSetResponse _cache;
        Boolean _firstCall = true;
        readonly Exception _ThrownException = null;

        #endregion

        #region Settings

        public Exception ThrownException() => _ThrownException; 

        public MenuItem[] SettingsItems() { return new MenuItem[] { new MenuItem(Properties.Menu.HourUpdate, ChangehourToUpdate) }; }


        public void TryupdateMenuItem(object sender, EventArgs e)
        {
            var Current = (MenuItem)sender;
            var Name = Current.Text;
            var S = Type.GetType(Name);
            var Item = (ILatLongInterface)Activator.CreateInstance(S);
            if (Item.worked())
            {
                geography = new Geography(Item.Latitude(), Item.Longitude());
                LatLongHandler.Set(geography.Latitude, geography.Longitude);
                MessageBox.Show(Properties.Messages.UpdateComplete);
            }
            else { MessageBox.Show(Properties.Warnings.UpdateDidNotWork); }


        }
        #endregion

        #region Events
        private void ChangehourToUpdate(object sender, EventArgs e) => UpdateHour();
        #endregion

        #region New

        public InternalSunRiseSet() { }

        public void Load()
        {
            geography = GetLocationProperty();
            _HourToUpdate = int.Parse(AppSetttingsHandler.HourUpdate ?? "6");
            _LastUpdate = DateTime.Now;
            Invoke();
        }

        #endregion

        #region invoke

        public ISharedResponse Invoke()
        {

            if (_firstCall || (!HasUpdatedToday && DateTime.Now.Hour == _HourToUpdate))
            {
                _firstCall = false;
                HasUpdatedToday = true;
                _cache = LiveCall();
            }
            if (_LastUpdate.Day != DateTime.Today.Day) { HasUpdatedToday = false; }
            return _cache;
        }


        #endregion

        #region Live API call
        private SunRiseSetResponse LiveCall()
        {
            return new SunRiseSetResponse()
            {
                SolarNoon = SolarNoon.NextEvent(),
                SunRise = SunriseTime.NextEvent(),
                SunSet = SunsetTime.NextEvent()
            };
        }
        #endregion

        #region Helpers

        static bool IntialgetLatLong()
        {
            var worked = false;
            if (MessageBox.Show(
                Properties.Messages.LatLongLookupMessageYesNo,
                Properties.Titles.LatLongNotSet,
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var lat = GetDoubleWithMessage("Latitude");
                var lon = GetDoubleWithMessage("Longitude");
                LatLongHandler.Set(lat, lon);
                worked = true;
            }
            return worked;
        }

        static double GetDoubleWithMessage(string ObjectName) 
            => double.Parse(InputHandler.InputBox(String.Format(Properties.Prompts.PleaseEnterYour_, ObjectName), ObjectName));
        
        static void UpdateHour()
        {
            var current = int.TryParse(
                AppSetttingsHandler.HourUpdate, out int value) ? value : 6;
            try
            {
               var attempt = InputHandler.InputBox(Properties.Prompts.EnterHourToUpdate,
                    Properties.Titles.HourUpdate, current.ToString());
                AppSetttingsHandler.HourUpdate = int.Parse(attempt).ToString();
            }
            catch { MessageBox.Show(Properties.Warnings.CouldNotUpdateTryAgain); }
        }

        static Geography GetLocationProperty() =>
            (LatLongHandler.HasRecord() || IntialgetLatLong()) ?
                new Geography(LatLongHandler.Lat, LatLongHandler.Lng) :
                new Geography(0, 0);

        #endregion

        #region "Heavy Math"
        private readonly int TimeZoneOffset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours;
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
        private double HASunrise => DEGREES(Math.Acos(Math.Cos(RADIANS(90.833)) / (Math.Cos(RADIANS(geography.Latitude)) * Math.Cos(RADIANS(SunDeclin))) - Math.Tan(RADIANS(geography.Latitude)) * Math.Tan(RADIANS(SunDeclin))));
        private double PSolarNoon => (720 - 4 * geography.Longitude - EqofTime + TimeZoneOffset * 60) / 1440;
        public TimeSpan SolarNoon => DateTime.FromOADate(PSolarNoon).TimeOfDay;
        public TimeSpan SunriseTime => DateTime.FromOADate(PSolarNoon - HASunrise * 4 / 1440).TimeOfDay;
        public TimeSpan SunsetTime => DateTime.FromOADate(PSolarNoon + HASunrise * 4 / 1440).TimeOfDay;
        private static double MOD1(double Number, double Divider) => Number % Divider; 
        private static double RADIANS(double angle) => (Math.PI / 180) * angle; 
        private static double DEGREES(double radians) => radians * (180 / Math.PI); 
        #endregion

        #region Debug values
        public string Debug()
        {
            return new Dictionary<string, string>
            {
                {"Hour to update", _HourToUpdate.ToString()},
                {"Last update", _LastUpdate.ToString()},
                {"Latitude", geography.Latitude.ToString() },
                {"Longitude", geography.Longitude.ToString() },
                {"SunRise", _cache.SunRise.ToString() },
                {"SunSet", _cache.SunSet.ToString() },
                {"Status", _cache.Status }
            }.CompileDebug();
        }
        #endregion


    }
}
