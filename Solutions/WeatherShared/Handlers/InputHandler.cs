using Essy.Tools.InputBox;

namespace WeatherShared.Handlers
{
    internal class InputHandler
    {

        public static string Input() => InputBox.ShowInputBox("title");

        public static string Input(string title) => InputBox.ShowInputBox(title);

        public static string Input(string title, string DefaultValue) => InputBox.ShowInputBox(title, DefaultValue, false);


    }
}
