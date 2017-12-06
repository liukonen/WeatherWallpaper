using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherDesktop.Shared
{
    public static class SharedObjects
    {
        public static string InputBox(string Prompt) {return InputBox(Prompt, string.Empty);}
        public static string InputBox(string Prompt, string Title){ return InputBox(Prompt, Title, string.Empty); }
        public static string InputBox(string Prompt, string Title, string DefaultResponse){return Microsoft.VisualBasic.Interaction.InputBox(Prompt, Title, DefaultResponse);}


    }
}
