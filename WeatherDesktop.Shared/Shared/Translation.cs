using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;

namespace WeatherDesktop.Share
{
     class Translation
    {
        const string cDocName = "Languages.json";
        const string cGoogleAPIS = "https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl={0}&dt=t&q={1}";

        #region Globals
        private string _lang;
        private Dictionary<string, string> InMemoryLanguage;
        #endregion

        #region Interface

        public string CurrentLanguage { get { return _lang; } }

        /// <summary>
        /// Reassignes the Language value
        /// </summary>
        /// <param name="TwoLetterISOLanguageName"></param>
        public void ChangeLanguage(string TwoLetterISOLanguageName) { Initalize(TwoLetterISOLanguageName); }

        /// <summary>
        /// Takes in a array of strings for faster caching. 
        /// </summary>
        /// <param name="ValuesToAdd"> Strings can not contain "," since the value is passed in as a CSV</param>
        public void preloadValues(string[] ValuesToAdd)
        {
            List<string[]> Collections = new List<string[]>();
            ConcurrentDictionary<string, string> cValue = new ConcurrentDictionary<string, string>();

            int counter = 0;
            List<string> Items = new List<string>();

            foreach (var element in ValuesToAdd)
            {
                if (!InMemoryLanguage.ContainsKey(element))
                {
                    //limit strings to 200 char
                    if (counter + element.Length > 200)
                    {
                        Collections.Add(Items.ToArray());
                        Items.Clear();
                        counter = 0;
                    }
                    counter += element.Length; Items.Add(element);
                }
            }
            Collections.Add(Items.ToArray());

            //Call out the door
            Parallel.ForEach(Collections, (group) =>
            {
                foreach (var item in TranslateArray(group))
                {
                    cValue.TryAdd(item.Key, item.Value);
                }
            });


            bool HasChanged = (cValue.Count > 0);
            foreach (var item in cValue)
            {
                HasChanged = true;
                InMemoryLanguage.Add(item.Key, item.Value);
            }
            //Update Saved cache with new records
            if (HasChanged) { Update(); }


        }

        /// <summary>
        /// Calls the google API for each record, but batch updates / saves
        /// </summary>
        /// <param name="values"></param>
        public void preloadLargerValues(params string[] values)
        {
            ConcurrentDictionary<string, string> items = new ConcurrentDictionary<string, string>();
            Parallel.ForEach(values, (group) =>
            {
                if (!InMemoryLanguage.ContainsKey(group))
                {
                    items.TryAdd(group, RawTranslate(_lang, group));
                }
            });
            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    InMemoryLanguage.Add(item.Key, item.Value);
                }
                Update();
            }
        }

        /// <summary>
        /// Grabs from cache, or does a live call and updates cache
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public string Translate(string request)
        {
            if (InMemoryLanguage.ContainsKey(request)) { return InMemoryLanguage[request]; }
            string Value = RawTranslate(_lang, request);
            InMemoryLanguage.Add(request, Value);
            Update();
            return Value;
        }

        #region Constructors

        public Translation() { Initalize(System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName); }

        public Translation(string TwoLetterISOLanguageName) { Initalize(TwoLetterISOLanguageName); }

        #endregion

        /// <summary>
        /// Converts translated Weather text back to English, matching Enum value
        /// </summary>
        /// <param name="Weather"></param>
        /// <returns></returns>
        public string ReverseTranslateWeather(string Weather)
        {
            return (from KeyValuePair<string, string> item in InMemoryLanguage where item.Value == Weather select item.Key).First();
        }

        /// <summary>
        ///  Converts translated Day or Night text back to English, matching Enum value
        /// </summary>
        /// <param name="DayNight"></param>
        /// <returns></returns>
        public string ReverseTranlateDayNight(string DayNight)
        {
            return (from KeyValuePair<string, string> item in InMemoryLanguage where item.Value == DayNight select item.Key).First();
        }

        /// <summary>
        /// Translates the Weather Enum type to a translated text version
        /// </summary>
        /// <param name="WeatherType"></param>
        /// <returns></returns>
        public string TranslatedWeatherType(Share.SharedObjects.WeatherTypes WeatherType)
        {
            string Name = Enum.GetName(typeof(Share.SharedObjects.WeatherTypes), WeatherType);
            return ReadDictionary(Name);
        }
        #endregion



        /// <summary>
        /// Returns a dictionary of all Supported Lanuguages Key - Description, value - Two Char Key code.
        /// </summary>
        public static Dictionary<string, string> SupportedLanguages = new Dictionary<string, string>()
        {
             {"Afrikaans", "af"},{"shqiptar", "sq"},{"አማርኛ", "am"},{"عربى", "ar"},{"հայերեն", "hy"},{"Azeerbaijani", "az"},{"Euskal", "eu"},{"беларускі", "be"},
             {"বাঙালি", "bn"},{"Bosanski", "bs"},{"български", "bg"},{"Català", "ca"},{"Cebuano", "ceb"},{"简体中文）", "zh-CN"},{"中國傳統的）", "zh-TW"},{"Corsu", "co"},
             {"Hrvatski", "hr"},{"čeština", "cs"},{"dansk", "da"},{"Nederlands", "nl"},{"English", "en"},{"Esperanto", "eo"},{"Eesti keel", "et"},{"Suomalainen", "fi"},
             {"français", "fr"},{"Frysk", "fy"},{"Galego", "gl"},{"ქართული", "ka"},{"Deutsche", "de"},{"Ελληνικά", "el"},{"ગુજરાતી", "gu"},{"Kreyòl Ayisyen", "ht"},
             {"Hausa", "ha"},{"ʻŌlelo Hawaiʻi", "haw"},{"עִברִית", "iw"},{"हिंदी", "hi"},{"Hmoob", "hmn"},{"Magyar", "hu"},{"Íslensku", "is"},{"Igbo", "ig"},
             {"bahasa Indonesia", "id"},{"Gaeilge", "ga"},{"italiano", "it"},{"日本語", "ja"},{"Wong Jawa", "jw"},{"ಕನ್ನಡ", "kn"},{"Қазақша", "kk"},{"ភាសាខ្មែរ", "km"},
             {"한국어", "ko"},{"Kurdî", "ku"},{"Кыргызча", "ky"},{"ລາວ", "lo"},{"Latine", "la"},{"Latviešu", "lv"},{"Lietuviškai", "lt"},{"lëtzebuergesch", "lb"},
             {"Македонски", "mk"},{"Malagasy", "mg"},{"Melayu", "ms"},{"മലയാളം", "ml"},{"Malti", "mt"},{"Maori", "mi"},{"मराठी", "mr"},{"Монгол хэл", "mn"},
             {"မြန်မာ {ဗမာ}", "my"},{"नेपाली", "ne"},{"norsk", "no"},{"Nyanja {Chichewa}", "ny"},{"پښتو", "ps"},{"فارسی", "fa"},{"Polskie", "pl"},{"Português {Portugal, Brasil}", "pt"},
             {"ਪੰਜਾਬੀ", "pa"},{"Română", "ro"},{"русский", "ru"},{"Samoa", "sm"},{"Gàidhlig na h-Alba", "gd"},{"Српски", "sr"},{"Sesotho", "st"},{"Shona", "sn"},
             {"سنڌي", "sd"},{"සිංහල {සිංහල}", "si"},{"slovenský", "sk"},{"Slovenščina", "sl"},{"Somali", "so"},{"Español", "es"},{"Sunda", "su"},{"Kiswahili", "sw"},
             {"svenska", "sv"},{"Tagalog {Filipino}", "tl"},{"Тоҷикӣ", "tg"},{"தமிழ்", "ta"},{"తెలుగు", "te"},{"ไทย", "th"},{"Türk", "tr"},{"Українська", "uk"},
             {"اردو", "ur"},{"O'zbek", "uz"},{"Tiếng Việt", "vi"},{"Cymraeg", "cy"},{"isiXhosa", "xh"},{"ייִדיש", "yi"},{"Yorùbá", "yo"},{"Zulu", "zu"}
        };

        #region Private Methods
        
        private void Update()
        {
            var Lan = Languages;
            Lan[_lang] = InMemoryLanguage;
            Languages = Lan;
        }

        //https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl=fr&dt=t&q=[exit, hello, goodbye]&q=Settings&q=about
        private static string RawTranslate(string ToLang, string InputText)
        {
            if (ToLang.ToLower() != "en")
            {
                string Response = SharedObjects.CompressedCallSite(string.Format(cGoogleAPIS, ToLang, InputText));
                int StartIndex = Response.IndexOf('"') + 1;
                int EndIndex = Response.IndexOf('"' + ",");
                return Response.Substring(StartIndex, EndIndex - StartIndex);
            }
            else { return InputText; }
            //[[["sortie, bonjour, au revoir","exit, hello, goodbye",null,null,3]],null,"en"]
        }

        private Dictionary<string, Dictionary<string, string>> Languages
        {

            get
            {
                try
                {
                    if (System.IO.File.Exists(cDocName))
                    {
                        System.Diagnostics.Debug.WriteLine(File.ReadAllText(cDocName));
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(cDocName));
                    }
                }
                catch (Exception x) { System.Diagnostics.Debug.WriteLine(x.ToString()); }
                return new Dictionary<string, Dictionary<string, string>>();

            }

            set
            {
                File.WriteAllText(cDocName, Newtonsoft.Json.JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.Indented));
            }
        }

        private void Initalize(string TwoLetterISOLanguageName)
        {
            _lang = TwoLetterISOLanguageName; 
             var Lan = Languages;

            if (Lan.ContainsKey(TwoLetterISOLanguageName)) { InMemoryLanguage = Lan[TwoLetterISOLanguageName]; }
            else
            {

                InMemoryLanguage = CreateNewLanguage(TwoLetterISOLanguageName);
                Lan.Add(TwoLetterISOLanguageName, InMemoryLanguage);
                Languages = Lan;
            }
        }

        private string ReadDictionary(string value)
        {
            bool refreshDictionary = false;
            try
            {
                if (!InMemoryLanguage.ContainsKey(value))
                { /*add new translation*/
                    string Translate = RawTranslate(_lang, value);
                    InMemoryLanguage.Add(value, Translate);
                    refreshDictionary = true;
                }
                if (refreshDictionary) { Languages[_lang] = InMemoryLanguage; }

                return InMemoryLanguage[value];
            }
            catch { return value; }
        }


        private Dictionary<string, string> CreateNewLanguage(string TwoLetterISOLanguageName)
        {
            Dictionary<string, string> value = new Dictionary<string, string>();
            System.Collections.Concurrent.ConcurrentDictionary<string, string> cValue = new System.Collections.Concurrent.ConcurrentDictionary<string, string>();


            //preload Constants
            List<string[]> Groups = new List<string[]>();

            int counter = 0;
            List<string> WeatherGroup = new List<string>();

            foreach (var element in System.Enum.GetValues(typeof(Share.SharedObjects.WeatherTypes)))
            {
                string Name = Enum.GetName(typeof(Share.SharedObjects.WeatherTypes), element);
               
                WeatherGroup.Add(Name);
                if (counter == 10)
                {
                    Groups.Add(WeatherGroup.ToArray());
                    WeatherGroup.Clear();
                }

            }
            Groups.Add(WeatherGroup.ToArray());

            //Groups.Add(new string[] { Properties.Resources.tZip, Properties.Resources.tChangeZipMenu, Properties.Resources.tError, Properties.Resources.tPEZ });

            Parallel.ForEach(Groups, (group) =>
            {
                foreach (var item in TranslateWeatherArray(group))
                {
                    cValue.TryAdd(item.Key, item.Value);
                }
            });
            value = cValue.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var SharedItems = TranslateArray(new string[] { Properties.Resources.tZip, Properties.Resources.tChangeZipMenu, Properties.Resources.tError, Properties.Resources.tPEZ });
            foreach (var item in SharedItems)
            {
                value.Add(item.Key, item.Value);
            }
            value.Add(Properties.Resources.tValidateZip, RawTranslate(_lang, Properties.Resources.tValidateZip));

            return value;
        }

        private Dictionary<string, string> TranslateWeatherArray(string[] ItemsToTranslate)
        {
            Dictionary<string, string> value = new Dictionary<string, string>();

            List<string> FixedNames = new List<string>();
                foreach (var item in ItemsToTranslate)
                {
                if (_lang != "en") { FixedNames.Add(TranslateName(item)); }
                else { FixedNames.Add(item); }
            }
            string items = String.Join(", ", FixedNames.ToArray());
            string ReturnItems = RawTranslate(_lang, items);
            string[] ReturnArray = ReturnItems.Split(',');

            int counter = 0;
            foreach (var item in ItemsToTranslate)
            {
                value.Add(item.Trim(), ReturnArray[counter].Trim());
                counter++;
            }
            return value;
        }

        private Dictionary<string, string> TranslateArray(string[] ItemsToTranslate)
        {
            Dictionary<string, string> value = new Dictionary<string, string>();
            string items = String.Join(", ", ItemsToTranslate);
            string ReturnItems = RawTranslate(_lang, items);
            string[] ReturnArray = ReturnItems.Split(',');

            int counter = 0;
            foreach (var item in ItemsToTranslate)
            {
                value.Add(item.Trim(), ReturnArray[counter].Trim());
                counter++;
            }
            return value;
        }

        private string TranslateName(string element)
        {
            return System.Text.RegularExpressions.Regex.Replace(element, "(\\B[A-Z])", " $1");
        }
        #endregion
    }

    /// <summary>
    /// Can be used by external Services to reduce overhead of having multiples of the same object open
    /// </summary>
    public static class QuickTranslation
    {
        public static string Translate(string Text)
        {
            Translation T = GetTranslationObject();
            string Value = T.Translate(Text);
            SetTranslation(T);
            return Value;
            
        }
        private static Translation GetTranslationObject()
        {
            const string CacheLocation = "TranslationObject";
            System.Runtime.Caching.MemoryCache cache = System.Runtime.Caching.MemoryCache.Default;
            Translation T;
            if (cache.Contains(CacheLocation))
            { T = (Translation)cache[CacheLocation]; }
            else
            {
                string lang = SharedObjects.AppSettings.ReadSetting("Language");
                if (string.IsNullOrWhiteSpace(lang)){ lang = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName; }
                T = new Translation(lang);
                cache.Add(CacheLocation, T, DateTime.Now.AddDays(1));
            }
            return T;
        }

        private static void SetTranslation(Translation value)
        {
            const string CacheLocation = "TranslationObject";

            System.Runtime.Caching.MemoryCache cache = System.Runtime.Caching.MemoryCache.Default;
            if (cache.Contains(CacheLocation)) { cache[CacheLocation] = value; }
            else { cache.Add(CacheLocation, value, DateTime.Now.AddDays(1)); }
        }

        public static void PreloadValues(string[] ValuesToAdd)
        {
            Translation value = GetTranslationObject();
            value.preloadValues(ValuesToAdd);
            SetTranslation(value);
        }

        public static void PreloadLargerValues(params string[] values)
        {
            Translation value = GetTranslationObject();
            value.preloadLargerValues(values);
            SetTranslation(value);
        }

        public static void ChangeLanguage(string TwoLetterISOLanguageName)
        {
            Translation value = GetTranslationObject();
            value.ChangeLanguage(TwoLetterISOLanguageName);
            SharedObjects.AppSettings.AddUpdateAppSettings("Language", TwoLetterISOLanguageName);
            SetTranslation(value);

        }

        public static string ReverseTranslateWeather(string Weather)
        {
            Translation value = GetTranslationObject();
            return value.ReverseTranslateWeather(Weather);
            
        }

        public static string ReverseTranlateDayNight(string DayNight)
        {
            Translation value = GetTranslationObject();
            return value.ReverseTranlateDayNight(DayNight);
        }

        public static string TranslatedWeatherType(Share.SharedObjects.WeatherTypes WeatherType)
        {
            Translation value = GetTranslationObject();
            return value.TranslatedWeatherType(WeatherType);
        }
        public static Dictionary<string, string> SupportedLanguages
        {
            get { return Translation.SupportedLanguages; }
        }

    }

}
