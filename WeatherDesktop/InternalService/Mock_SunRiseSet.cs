using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WeatherDesktop.Interface;
using System.ComponentModel.Composition;
using WeatherDesktop.Share;
using WeatherDesktop.Shared.Extentions;
using WeatherDesktop.Shared.Handlers;

namespace InternalService
{
    [Export(typeof(WeatherDesktop.Interface.IsharedSunRiseSetInterface))]
    [ExportMetadata("ClassName", "Mock_SunRiseSet")]
    class Mock_SunRiseSet : IsharedSunRiseSetInterface
    {

        const string ClassName = "Mock_SunRiseSet";

        public Exception ThrownException() { return null; }

        public void Load() { }

        SunRiseSetResponse _cache = new SunRiseSetResponse();

        DateTime SunRiseDateTime
        {
            get
            {
                if (_cache != null && _cache.SunRise != null) { return _cache.SunRise; }
                string setting = AppSetttingsHandler.Read(ClassName + ".SunRise");
                if (!string.IsNullOrWhiteSpace(setting)) { return TimeSpanToDateTime(TimeSpan.Parse(setting)); }
                return new DateTime();
            }
            set
            {
                _cache.SunRise = TimeSpanToDateTime(value.TimeOfDay);
                AppSetttingsHandler.Write(ClassName + ".SunRise", value.TimeOfDay.ToString());
            }
        }

        DateTime SunSetDateTime
        {
            get
            {
                if (_cache != null && _cache.SunSet != null) { return _cache.SunSet; }
                string setting = AppSetttingsHandler.Read(ClassName + ".SunSet");
                if (!string.IsNullOrWhiteSpace(setting)) { return TimeSpanToDateTime(TimeSpan.Parse(setting)); }
                return new DateTime();
            }
            set
            {

                _cache.SunSet = TimeSpanToDateTime(value.TimeOfDay);
                AppSetttingsHandler.Write(ClassName + ".SunSet", value.TimeOfDay.ToString());
            }
        }


        private static DateTime TimeSpanToDateTime(TimeSpan Request)
        {
            return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Request.Hours, Request.Minutes, Request.Seconds);
        }

        public MenuItem[] SettingsItems()
        {
            return new MenuItem[] { new MenuItem("Update SunRise", ChangehourToUpdate), new MenuItem("Update SunSet", ChangehourToUpdate) };
        }

        public Mock_SunRiseSet()
        {
            _cache = new SunRiseSetResponse() { SunSet = SunSetDateTime, SunRise = SunRiseDateTime };
        }
        public ISharedResponse Invoke()
        {
            return _cache;
        }

        private void ChangehourToUpdate(object sender, EventArgs e)
        {
            string Title = ((MenuItem)sender).Text;
            string sTimeSpan = InputHandler.InputBox("Please Enter the Timespan (example 7:00:00", Title);
            TimeSpan extract = new TimeSpan();
            if (!TimeSpan.TryParse(sTimeSpan, out extract))
            { MessageBox.Show("Error getting timespan, try again"); }
            else
            {
                DateTime Now = DateTime.Now;
                DateTime Parsed = new DateTime(Now.Year, Now.Month, Now.Day, extract.Hours, extract.Minutes, extract.Seconds);
                if (Title.EndsWith("SunSet"))
                { _cache.SunSet = Parsed; SunSetDateTime = Parsed; }
                else { _cache.SunRise = Parsed; SunRiseDateTime = Parsed; ; }
            }


        }

        public string Debug() =>
            new Dictionary<string, string>
            {
                {"SunRise", _cache.SunRise.ToString()},
                {"SunSet", _cache.SunSet.ToString() }
            }.CompileDebug();

    }
}
