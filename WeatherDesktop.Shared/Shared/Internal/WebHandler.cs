using System.IO;
using System.Net;


namespace WeatherDesktop.Shared.Handlers
{
    internal class WebHandler
    {
        private static readonly WebHandler instance = new WebHandler();
        private WebHandler() { }
        static WebHandler() { }

        public static WebHandler Instance
        {
            get { return instance; }
        }

        public string CallSite(string Url) => CallSite(Url, string.Empty);

        public string CallSite(string Url, string UserAgent)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            if (!string.IsNullOrWhiteSpace(UserAgent)) { request.UserAgent = UserAgent; }
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.AllowAutoRedirect = true;

            string ResponseString = string.Empty;
            using (WebResponse response = request.GetResponse())
                ResponseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            return ResponseString;
        }
    }
}

