using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace WeatherShared.Handlers
{
    public class WebRequestHandler
    {
        private static readonly WebRequestHandler instance = new WebRequestHandler();
        private WebRequestHandler() { }
        static WebRequestHandler() { }

        public static WebRequestHandler Instance
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

