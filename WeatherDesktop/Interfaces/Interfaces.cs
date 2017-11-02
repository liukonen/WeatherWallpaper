﻿using System.Windows.Forms;

namespace WeatherDesktop.Interface
{
    public interface ISharedResponse { }

    public interface ISharedInterface
    {
        ISharedResponse Invoke();
        string Debug();
        MenuItem[] SettingsItems();
    }

    public interface ISharedWeatherinterface : ISharedInterface { }
    public interface IsharedSunRiseSetInterface : ISharedInterface { }
}
