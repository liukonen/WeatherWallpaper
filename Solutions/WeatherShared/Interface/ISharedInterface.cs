using System;
using System.Windows.Forms;

namespace WeatherShared.Interface
{
    public interface ISharedInterface
    {
        ISharedResponse Invoke();

        string Debug();
        
        MenuItem[] SettingsItems();
       
        Exception ThrownException();

        void Load();
    }
}
