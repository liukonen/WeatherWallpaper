using System.Windows.Forms;

namespace WeatherDesktop.Interface
{
    public interface ISharedInterface
    {
        ISharedResponse Invoke();
        string Debug();
        MenuItem[] SettingsItems();
       System.Exception ThrownException();
        //anything that would normally go in the constructor
        void Load();
    }
}
