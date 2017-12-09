using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using SharpRaven;
using System.Diagnostics;
using System.Diagnostics.Eventing;

namespace WeatherDesktop.Share
{
   static class ErrorHandler
    {

        public static void AbsouluteException(Exception x)
        {
            string sSource;
            string sLog;
            string sEvent;

            sSource = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            sLog = "Application";
            sEvent = x.ToString();

            if (!EventLog.SourceExists(sSource))
                EventLog.CreateEventSource(sSource, sLog);
            var raw = Encoding.ASCII.GetBytes(x.ToString());
            EventLog.WriteEntry(sSource, x.Message, EventLogEntryType.Error, 1, 1, raw);
        }

        


        public static void LogException(Exception x)
        {
               string sSource;
                string sLog;
                string sEvent;

                sSource = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                sLog = "Application";
                sEvent = x.ToString();

                if (!EventLog.SourceExists(sSource))
                    EventLog.CreateEventSource(sSource, sLog);
                var raw = Encoding.ASCII.GetBytes(x.ToString());
                EventLog.WriteEntry(sSource, x.Message, EventLogEntryType.Warning, 1, 1, raw);           
        }


    }
}

