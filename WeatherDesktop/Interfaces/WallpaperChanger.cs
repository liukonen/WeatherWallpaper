/*
// original taken from https://stackoverflow.com/questions/1061678/change-desktop-wallpaper-using-code-in-net
original credit: Neil N and Eran
modified
 */
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace WeatherDesktop.Interfaces
{
    public sealed class Wallpaper
    {
        Wallpaper() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int{ Tiled =0, Centered =1, Stretched =2}

        public static void Set(string path, Style style)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            string sStyle = ((int)style).ToString();
            string Tile = 0.ToString();
            if (style == Style.Tiled) { sStyle = 1.ToString(); Tile = 1.ToString(); }
            key.SetValue(@"WallpaperStyle", sStyle);
            key.SetValue(@"TileWallpaper", Tile);
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
