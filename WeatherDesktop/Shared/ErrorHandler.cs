using SharpRaven;
using System;
using System.Diagnostics;
using System.Text;


namespace WeatherDesktop.Share
{
    static class ErrorHandler
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void Send(Exception x)
        {
            try
            {
                var ravenClient = new RavenClient(Properties.Resources.Sentry_ErrorHandler_Url);
                ravenClient.Capture(new SharpRaven.Data.SentryEvent(x));
            }
            catch (Exception xx) //In the event of an error from RavenClient. Log the Error AND log the Raven Error
            {
                string sSource;
                string sLog;
                string sEvent;

                sSource = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                sLog = "Application";
                sEvent = x.ToString();

                if (!EventLog.SourceExists(sSource)) { EventLog.CreateEventSource(sSource, sLog); }
                var raw = Encoding.ASCII.GetBytes(sEvent);
                var Raw2 = Encoding.ASCII.GetBytes(xx.ToString());
                EventLog.WriteEntry(sSource, x.Message, EventLogEntryType.Warning, 1, 1, raw);
                EventLog.WriteEntry(sSource, xx.Message, EventLogEntryType.Warning, 2, 2, Raw2);
            }
        }
    }
}