﻿using System.Runtime.InteropServices;
using Microsoft.Win32;
using WeatherDesktop.Properties;

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
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            string sStyle = ((int)style).ToString();
            string Tile = 0.ToString();
            if (style == Style.Tiled) { sStyle = 1.ToString(); Tile = 1.ToString(); }
            key.SetValue(@"WallpaperStyle", sStyle);
            key.SetValue(@"TileWallpaper", Tile);
           NativeMethods.SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        public static System.Drawing.Icon GetWeatherIcon(SharedObjects.WeatherTypes weather, bool isDaytime)
        {
            switch (weather)
            {
                case SharedObjects.WeatherTypes.Clear:
                    if (isDaytime) { return Resources.Clear_day; }
                    else { return Resources.Clear_night; }
                case SharedObjects.WeatherTypes.Cloudy:
                    return Resources.cloudy;
                case SharedObjects.WeatherTypes.Fog:
                    return Resources.fog;
                case SharedObjects.WeatherTypes.Frigid:
                    return Resources.Frigid;
                case SharedObjects.WeatherTypes.Hot:
                    return Resources.hot;
                case SharedObjects.WeatherTypes.PartlyCloudy:
                    if (isDaytime) { return Resources.PartlyCloudy_day; }
                    else { return Resources.PartlyCloudy_night; }
                case SharedObjects.WeatherTypes.Rain:
                    return Resources.raindrop;
                case SharedObjects.WeatherTypes.Snow:
                    return Resources.snowflake;
                case SharedObjects.WeatherTypes.ThunderStorm:
                    return Resources.Thunderstorm;
                case SharedObjects.WeatherTypes.Windy:
                    return Resources.wind;
                case SharedObjects.WeatherTypes.Dust:
                case SharedObjects.WeatherTypes.Haze:
                case SharedObjects.WeatherTypes.Smoke:
                default:
                    return Resources.windsock;
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

