﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WeatherDesktop.Shared.Extentions
{
    public static class Extentions
    {
        public static int ToInt(this BitArray Array)
        {
            var response = new int[1];
            Array.CopyTo(response, 0);
            return response[0];
        }

        public static BitArray ToBitArray(this int item) => new BitArray(new int[] { item });

        public static string CompileDebug(this Dictionary<string, string> ItemsTodisplay)
        {
            var builder = new StringBuilder(Environment.NewLine);
            foreach (var item in ItemsTodisplay)
            {
                builder.AppendLine($"{item.Key}: {item.Value}");
            }
            return builder.Append(Environment.NewLine).ToString();
        }

        public static bool Between(this TimeSpan test, TimeSpan Lower, TimeSpan Higher) => Lower < test && test < Higher;

        public static DateTime NextEvent(this TimeSpan span) 
            => (span > DateTime.Now.TimeOfDay) ? DateTime.Now.Date.Add(span):
             DateTime.Now.AddDays(1).Date.Add(span);
     
        public static BitArray SetRange(this BitArray array, IEnumerable<int> indexes, Boolean value) 
        {
            foreach (var item in indexes)
            {
                array[item] = value;
            }
            return array;
        }

        public static IEnumerable<int> SelectedIndexs(this BitArray array) 
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i]) yield return i;
            }
        }

        public static void AddOrUpdate<T>(this Dictionary<string,T> item, string key, T value) 
        {
            if (item.ContainsKey(key)) { item[key] = value; } else { item.Add(key, value); }
        }

    }
}
