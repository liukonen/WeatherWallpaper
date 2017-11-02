using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WeatherDesktop.Interface
{
    class Mock_SunRiseSet :IsharedSunRiseSetInterface
    {

        const string ClassName = "Mock_SunRiseSet";



        SunRiseSetResponse _cache = new SunRiseSetResponse();

        DateTime SunRise {
            get
            {
                if (_cache != null && _cache.SunRise != null) { return _cache.SunRise; }
                string setting = Interface.Shared.ReadSetting(ClassName + ".SunRise");
                if (!string.IsNullOrWhiteSpace(setting)){return TimeSpanToDateTime(TimeSpan.Parse(setting));}
                return new DateTime();
            }
            set
            {
                _cache.SunRise = TimeSpanToDateTime(value.TimeOfDay);
                Interface.Shared.AddUpdateAppSettings(ClassName + ".SunRise", value.TimeOfDay.ToString());
            }
        }
        DateTime SunSet
        {
            get
            {
                if (_cache != null && _cache.SunSet != null) { return _cache.SunSet; }
                string setting = Interface.Shared.ReadSetting(ClassName + ".SunSet");
                if (!string.IsNullOrWhiteSpace(setting)) { return TimeSpanToDateTime(TimeSpan.Parse(setting)); }
                return new DateTime();
            }
            set
            {

                _cache.SunSet = TimeSpanToDateTime(value.TimeOfDay); 
                Interface.Shared.AddUpdateAppSettings(ClassName + ".SunSet", value.TimeOfDay.ToString());
            }
        }


        private DateTime TimeSpanToDateTime(TimeSpan Request)
        {
            DateTime Now = DateTime.Now;            
            return new DateTime(Now.Year, Now.Month, Now.Day, Request.Hours, Request.Minutes, Request.Seconds);
        }

        public MenuItem[] SettingsItems()
        {
            List<MenuItem> returnValue = new List<MenuItem>();
            returnValue.Add(new MenuItem("Update SunRise", ChangehourToUpdate));
            returnValue.Add(new MenuItem("Update SunSet", ChangehourToUpdate));
            return returnValue.ToArray();
        }

        public Mock_SunRiseSet()
        {
            _cache = new SunRiseSetResponse();
            _cache.SunSet = SunSet;
            _cache.SunRise = SunRise;
            
        }
        public ISharedResponse Invoke()
        {
            return _cache;
        }

        private void ChangehourToUpdate(object sender, EventArgs e)
        {
            string Title = ((MenuItem)sender).Text;
            string sTimeSpan = Microsoft.VisualBasic.Interaction.InputBox("Please Enter the Timespan (example 7:00:00", Title);
            TimeSpan extract = new TimeSpan();
            if (!TimeSpan.TryParse(sTimeSpan, out extract))
            { MessageBox.Show("Error getting timespan, try again"); }
            else
            {
                DateTime Now = DateTime.Now;
                DateTime Parsed = new DateTime(Now.Year, Now.Month, Now.Day, extract.Hours, extract.Minutes, extract.Seconds);
                if (Title.EndsWith("SunSet"))
                { _cache.SunSet = Parsed; SunSet = Parsed; }
                else { _cache.SunRise = Parsed; SunRise = Parsed; ; }
            }
            

        }

        public string Debug()
        {
            Dictionary<string, string> DebugValues = new Dictionary<string, string>();
            DebugValues.Add("SunRise", _cache.SunRise.ToString());
            DebugValues.Add("SunSet", _cache.SunSet.ToString());
            return Shared.CompileDebug("SunRiseSet Service", DebugValues);
        }
    }
}
