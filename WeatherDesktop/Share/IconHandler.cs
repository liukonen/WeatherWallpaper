using System;
using System.Windows.Forms;
using WeatherDesktop.Shared.Handlers;
using System.Linq;
using System.Collections.Generic;

namespace WeatherDesktop.Share
{
    public enum IconThemeStyle { Light = 0, Dark = 1, Automatic = 2, Reverse = 3 }

    internal class IconHandler
    {
        const string AppPropertyName = "IconMode";

        public static IconThemeStyle Style
        {
            get => Enum.TryParse<IconThemeStyle>
                (AppSetttingsHandler.Read(AppPropertyName), out IconThemeStyle style) ? style : IconThemeStyle.Light;
            set => AppSetttingsHandler.Write(AppPropertyName, ((int)value).ToString());
        }

        public static bool IsLightMode(IconThemeStyle style, bool isNightMode)
        {
            switch (style)
            {
                case IconThemeStyle.Automatic: return (isNightMode);
                case IconThemeStyle.Reverse: return (!isNightMode);
                case IconThemeStyle.Dark: return false;
                default: return true;
            }
        
        }
    }
}
