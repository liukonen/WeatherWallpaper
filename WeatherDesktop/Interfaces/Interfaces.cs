using System.Windows.Forms;

namespace WeatherDesktop.Interface
{
    public interface ISharedResponse { }

    public interface ISharedInterface
    {
        ISharedResponse Invoke();
        string Debug();
        MenuItem[] SettingsItems();
        //bool status();
       System.Exception ThrownException();

        //anything that would normally go in the constructor
        void Load();
    }

    public interface ILatLongInterface
    {
        double Latitude();
        double Longitude();
        bool worked();

    }
    public interface ISharedWeatherinterface : ISharedInterface { }
    public interface IsharedSunRiseSetInterface : ISharedInterface { }

    public interface IClassName
    {
        string ClassName { get; }
    }

}
