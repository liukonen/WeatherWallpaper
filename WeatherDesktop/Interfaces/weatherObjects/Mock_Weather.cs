using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using System.Windows.Forms;

namespace WeatherDesktop.Interface
{
    class Mock_Weather :  ISharedWeatherinterface
    {
        const string ClassName  = "Mock_Weather";

        Shared.WeatherTypes activeWeathertype;
        string ForcastDescription = "Mock Weather Forcast.";
        int Temp = 98;

        Shared.WeatherTypes SetWeatherType
        {
            get { return activeWeathertype; }
            set { activeWeathertype = value; }
        }

        public MenuItem[] SettingsItems()
        {
            List<MenuItem> returnValue = new List<MenuItem>();
            returnValue.Add(new MenuItem("Change Temp", ChangeTempEvent));
            returnValue.Add(new MenuItem("Change Forcast", ChangeForcastEvent));
            List<MenuItem> WeatherTypes = new List<MenuItem>();
            foreach (var item in System.Enum.GetValues(typeof(Interface.Shared.WeatherTypes)))
            {
                WeatherTypes.Add(new MenuItem(Enum.GetName(typeof(Interface.Shared.WeatherTypes), item), ChangeWeatherType));
            }
            returnValue.Add(new MenuItem("Change Type", WeatherTypes.ToArray()));
            return returnValue.ToArray();
        }

        public ISharedResponse Invoke()
        {
            WeatherResponse Value = new WeatherResponse();
            Value.Temp = Temp;
            Value.ForcastDescription = ForcastDescription;
            Value.WType = SetWeatherType;
            return Value;
        }

        private void ChangeTempEvent(object sender, EventArgs e)
        {
          string sTemp =   Microsoft.VisualBasic.Interaction.InputBox("Change Temp", ClassName, Temp.ToString());
            if (!int.TryParse(sTemp, out Temp)) { MessageBox.Show("Could not parse Temp"); }
        }

        private void ChangeForcastEvent(object sender, EventArgs e)
        {
            ForcastDescription = Interaction.InputBox("Change Forcast", ClassName, ForcastDescription);
        }

        private void ChangeWeatherType(object sender, EventArgs e)
        {
            SetWeatherType = (Shared.WeatherTypes)System.Enum.Parse(typeof(Shared.WeatherTypes), ((MenuItem)sender).Text);
        }

        public string Debug()
        {
            Dictionary<string, string> DebugValues = new Dictionary<string, string>();
            DebugValues.Add("ActiveWeatherType", Enum.GetName(typeof(Shared.WeatherTypes), SetWeatherType));
            DebugValues.Add("Temp", Temp.ToString());
            DebugValues.Add("Forcast", ForcastDescription.ToString());
            return Shared.CompileDebug(ClassName + " Service", DebugValues);
        }

    }
}
