using System;
using System.Diagnostics;
using System.Text;


namespace WeatherDesktop.Share
{
    static class ErrorHandler
    {
        public static void Send(Exception x)
        {
            try
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
            catch (Exception ex) 
            {
                System.IO.File.AppendAllText("loggerException.log", ex.ToString() + Environment.NewLine);
                System.IO.File.AppendAllText("Log.log", x.ToString() + Environment.NewLine);
            }
        }
    }
}