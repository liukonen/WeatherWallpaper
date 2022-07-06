using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using System.ComponentModel.Composition;
using WeatherDesktop.Interface;
using WeatherDesktop.Share;
using WeatherDesktop.Shared.Extentions;

namespace InternalService
{
    [Export(typeof(ISharedWeatherinterface))]
    [ExportMetadata("ClassName", "Mock_Weather")]
    class Mock_Weather :  ISharedWeatherinterface
    {
        const string ClassName  = "Mock_Weather";

        SharedObjects.WeatherTypes activeWeathertype;
        string ForcastDescription = "Mock Weather Forcast.";
        int Temp = 98;

        public Exception ThrownException() { return null; }

       SharedObjects.WeatherTypes SetWeatherType
        {
            get { return activeWeathertype; }
            set { activeWeathertype = value; }
        }
        public void Load() { }
        public MenuItem[] SettingsItems()
        {
            List<MenuItem> returnValue = new List<MenuItem>
            {
                new MenuItem("Change Temp", ChangeTempEvent),
                new MenuItem("Change Forcast", ChangeForcastEvent)
            };
            List<MenuItem> WeatherTypes = new List<MenuItem>();
            foreach (var item in System.Enum.GetValues(typeof(SharedObjects.WeatherTypes)))
            {
                WeatherTypes.Add(new MenuItem(Enum.GetName(typeof(SharedObjects.WeatherTypes), item), ChangeWeatherType));
            }
            returnValue.Add(new MenuItem("Change Type", WeatherTypes.ToArray()));
            return returnValue.ToArray();
        }

        public ISharedResponse Invoke()
        {
            return new WeatherResponse { Temp = Temp, ForcastDescription = ForcastDescription, WType = SetWeatherType};
        }

        private void ChangeTempEvent(object sender, EventArgs e)
        {
          string sTemp =   Interaction.InputBox("Change Temp", ClassName, Temp.ToString());
            if (!int.TryParse(sTemp, out Temp)) { MessageBox.Show("Could not parse Temp"); }
        }

        private void ChangeForcastEvent(object sender, EventArgs e)
        {
            ForcastDescription = Interaction.InputBox("Change Forcast", ClassName, ForcastDescription);
        }

        private void ChangeWeatherType(object sender, EventArgs e)
        {
            SetWeatherType = (SharedObjects.WeatherTypes)Enum.Parse(typeof(SharedObjects.WeatherTypes), ((MenuItem)sender).Text);
        }

        public string Debug() =>
            new Dictionary<string, string>
            {
                { "ActiveWeatherType", Enum.GetName(typeof(SharedObjects.WeatherTypes), SetWeatherType) },
                { "Temp", Temp.ToString() },
                { "Forcast", ForcastDescription.ToString() }
            }.CompileDebug();
        

    }
}
