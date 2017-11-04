using System.Windows.Forms;

namespace WeatherDesktop.Interface
{
    public interface ISharedResponse { }

    public interface ISharedInterface
    {
        ISharedResponse Invoke();
        string Debug();
        MenuItem[] SettingsItems();
    }

    public interface ILatLongInterface
    {
        double Latitude();
        double Longitude();
        bool worked();

    }
    public interface ISharedWeatherinterface : ISharedInterface { }
    public interface IsharedSunRiseSetInterface : ISharedInterface { }
}
