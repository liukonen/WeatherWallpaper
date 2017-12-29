using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using WeatherDesktop.Properties;
using System.Windows.Forms;

namespace WeatherDesktop.Shared
{
    public class Translation
    {
      
        const string cDocName = "Languages.json";
        
        #region Constant Value Properties

        public string About { get { return Share.QuickTranslation.Translate(Resources.tAbout); } }
        public string WarningZipNotFound { get { return Share.QuickTranslation.Translate(Resources.warning_Zip_Not_Found); } }
        public string Exit { get { return Share.QuickTranslation.Translate(Resources.tExit); } }
        public string Settings { get { return Share.QuickTranslation.Translate(Resources.tSettings); } }
        public string Global { get { return Share.QuickTranslation.Translate(Resources.tGlobal); } }
        public string Images { get { return Share.QuickTranslation.Translate(Resources.tImages); } }
        public string Themes { get { return Share.QuickTranslation.Translate(Resources.tThemes); } }
        public string PluginFolder { get { return Share.QuickTranslation.Translate(Resources.tPluginFolder); } }

        public string BlackListHours { get { return Share.QuickTranslation.Translate(Resources.tBlackListHours); } }
        public string BlackListDays { get { return Share.QuickTranslation.Translate(Resources.tBlackListDays); } }
        public string BlackList { get { return Share.QuickTranslation.Translate(Resources.tBlackList); } }
        public string Weather { get { return Share.QuickTranslation.Translate(Resources.tWeather); } }
        public string SunRiseSet { get { return Share.QuickTranslation.Translate(Resources.tSunRiseSet); } }
        public string Error { get { return Share.QuickTranslation.Translate(Resources.tError); } }
        public string Version { get { return Share.QuickTranslation.Translate(Resources.tVersion); } }
        public string Copyright { get { return Share.QuickTranslation.Translate(Resources.tCopyright); } }
        public string DebugInfo { get { return Share.QuickTranslation.Translate(Resources.tDebugInfo); } }
        public string BlackListDaysHeader { get { return Share.QuickTranslation.Translate(Resources.tBlackListDaysHeader); } }
        public string BlackListHoursHeader { get { return Share.QuickTranslation.Translate(Resources.tBlackListHoursHeader); } }
        public string Language { get { return Share.QuickTranslation.Translate(Resources.tLanguages); } }

        public string BlackListHoursInputMessage { get { return Share.QuickTranslation.Translate(Resources.tBlacklistHoursInputMessage); } }
        public string BlackListDaysInputMessage { get { return Share.QuickTranslation.Translate(Resources.tBlacklistDaysInputMessage); } }
        public string Day { get { return Share.QuickTranslation.Translate(Resources.tDay); } }
        public string Night { get { return Share.QuickTranslation.Translate(Resources.tNight); } }
        #endregion



        public Translation()
        {
            string[] preload = new string[] { Resources.tAbout, Resources.warning_Zip_Not_Found, Resources.tExit, Resources.tSettings,
                Resources.tGlobal, Resources.tImages, Resources.tPluginFolder, Resources.tBlackListDays, Resources.tBlackListHours, Resources.tBlackList,
            Resources.tWeather, Resources.tSunRiseSet, Resources.tError, Resources.tVersion, Resources.tCopyright, Resources.tDebugInfo, Resources.tLanguages,
            Resources.tDay, Resources.tNight, Resources.tBlackListDaysHeader, Resources.tBlackListHoursHeader, Resources.ThemeImportTheme, Resources.ThemeLoadTheme, Resources.ThemeSaveTheme};
            Share.QuickTranslation.PreloadValues(preload);
            Share.QuickTranslation.PreloadLargerValues(Resources.tBlacklistDaysInputMessage, Resources.tBlacklistHoursInputMessage, Resources.tGenericError, Resources.ThemeWarning);

        }



    }
}
