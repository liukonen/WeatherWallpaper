using Microsoft.VisualBasic;

namespace WeatherDesktop.Shared.Handlers
{
    public class InputHandler
    {
        public static string InputBox(string Prompt) 
            => InputBox(Prompt, string.Empty, string.Empty); 
        public static string InputBox(string Prompt, string Title) 
            => InputBox(Prompt, Title, string.Empty); 
        public static string InputBox(string Prompt, string Title, string DefaultResponse)
            => Interaction.InputBox(Prompt, Title, DefaultResponse); 

    }
}
