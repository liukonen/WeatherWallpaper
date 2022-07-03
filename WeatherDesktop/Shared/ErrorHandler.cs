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

                string sSource;
                string sLog;
                string sEvent;

                sSource = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                sLog = "Application";
                sEvent = x.ToString();

                if (!EventLog.SourceExists(sSource)) { EventLog.CreateEventSource(sSource, sLog); }
                var raw = Encoding.ASCII.GetBytes(sEvent);
                EventLog.WriteEntry(sSource, x.Message, EventLogEntryType.Warning, 1, 1, raw);
        }
    }
}