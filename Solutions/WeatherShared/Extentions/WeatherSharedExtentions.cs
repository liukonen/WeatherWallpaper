using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WeatherShared.Extentions
{
   static public class WeatherSharedExtentions
    {

        public static bool BetweenTimespans(this TimeSpan Value, TimeSpan Lower, TimeSpan Higher) =>
            Lower < Value && Value < Higher;

        public static BitArray ToBitArray(this int item) => new BitArray(new int[] { item });

        public static int ToInt(this BitArray Array)
        {
            int[] array = new int[1];
            Array.CopyTo(array, 0);
            return array[0];
        }


        public static string CompileDebug(this Dictionary<string, string> ItemsTodisplay)
        {
            var builder = new StringBuilder(Environment.NewLine);
            foreach (var item in ItemsTodisplay)
            {
                builder.AppendLine($"{item.Key}: {item.Value}");
            }
            return builder.Append(Environment.NewLine).ToString();
        }

    }
}
