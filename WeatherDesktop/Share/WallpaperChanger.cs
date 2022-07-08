using System.Runtime.InteropServices;
using Microsoft.Win32;
using WeatherDesktop.Properties;
using System.Collections.Generic;
using System.Drawing;

namespace WeatherDesktop.Share
{
    public sealed class Wallpaper
    {
        Wallpaper() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        
        public enum Style : int { Tiled = 0, Centered = 1, Stretched = 2 }

        public static void Set(string path, Style style)
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            var sStyle = ((int)style).ToString();
            var Tile = 0.ToString();
            if (style == Style.Tiled) { sStyle = 1.ToString(); Tile = 1.ToString(); }
            key.SetValue(@"WallpaperStyle", sStyle);
            key.SetValue(@"TileWallpaper", Tile);
           NativeMethods.SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        public static Icon GetWeatherIcon(SharedObjects.WeatherTypes weather, bool isDaytime, bool isLightTheme) 
            => (isLightTheme) ? LightIcons(weather, isDaytime) : DarkIcons(weather, isDaytime);            
        

        private static Icon LightIcons(SharedObjects.WeatherTypes weather, bool isDaytime) 
        {
            switch (weather) 
            {
                case SharedObjects.WeatherTypes.Clear: return (isDaytime) ? Resources.light_Clear_day : Resources.light_Clear_night;
                case SharedObjects.WeatherTypes.Cloudy: return Resources.light_cloudy;
                case SharedObjects.WeatherTypes.Fog: return Resources.light_fog;
                case SharedObjects.WeatherTypes.Frigid: return Resources.light_Frigid;
                case SharedObjects.WeatherTypes.Hot: return Resources.light_hot;
                case SharedObjects.WeatherTypes.PartlyCloudy: return (isDaytime) ? Resources.light_PartlyCloudy_day : Resources.light_PartlyCloudy_night;
                case SharedObjects.WeatherTypes.Rain: return Resources.light_raindrop;
                case SharedObjects.WeatherTypes.Snow: return Resources.light_snowflake;
                case SharedObjects.WeatherTypes.ThunderStorm: return Resources.light_Thunderstorm;
                case SharedObjects.WeatherTypes.Windy: return Resources.light_wind;
                //case SharedObjects.WeatherTypes.Dust:
                //case SharedObjects.WeatherTypes.Haze:
                //case SharedObjects.WeatherTypes.Smoke:
                default: return Resources.light_windsock;
            }
        }

        private static Icon DarkIcons(SharedObjects.WeatherTypes weather, bool isDaytime)
        {
            switch (weather)
            {
                case SharedObjects.WeatherTypes.Clear: return (isDaytime) ? Resources.Dark_Clear_day : Resources.Dark_Clear_night;
                case SharedObjects.WeatherTypes.Cloudy: return Resources.Dark_cloudy;
                case SharedObjects.WeatherTypes.Fog: return Resources.Dark_fog;
                case SharedObjects.WeatherTypes.Frigid: return Resources.Dark_Frigid;
                case SharedObjects.WeatherTypes.Hot: return Resources.Dark_Hot;
                case SharedObjects.WeatherTypes.PartlyCloudy: return (isDaytime) ? Resources.Dark_PartlyCloudy_day : Resources.Dark_PartlyCloudy_night;
                case SharedObjects.WeatherTypes.Rain: return Resources.Dark_raindrop;
                case SharedObjects.WeatherTypes.Snow: return Resources.Dark_snowflake;
                case SharedObjects.WeatherTypes.ThunderStorm: return Resources.Dark_Thunderstorm;
                case SharedObjects.WeatherTypes.Windy: return Resources.Dark_wind;
                //case SharedObjects.WeatherTypes.Dust:
                //case SharedObjects.WeatherTypes.Haze:
                //case SharedObjects.WeatherTypes.Smoke:
                default: return Resources.Dark_windsock;
            }
        }


    }

   


    internal static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I4)]
       
        internal static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
    }
}

