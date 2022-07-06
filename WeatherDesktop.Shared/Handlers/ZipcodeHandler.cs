using System;
using System.Windows.Forms;

namespace WeatherDesktop.Shared.Handlers
{
    public class ZipcodeHandler
    {
        public static MenuItem ZipMenuItem
        {
            get => new MenuItem(Properties.Resources.ZipCodeMenuItem, ChangeZipClick); 
        }

        public static void ChangeZipClick(object sender, EventArgs e) => GetZip();
        

        /// <summary>
        /// Opens a dialog box, saves zip and returns value. if cancel, closes app gracefully... will retry if not a number
        /// </summary>
        /// <returns></returns>
        public static string GetZip()
        {
            var zip = string.Empty;
            var locker = new object();
            lock (locker)
            {
                while (!int.TryParse(zip, out int zipparse))
                {
                    zip = InputHandler.InputBox(Properties.Resources.ZipCodeHandlerMessage, 
                        Properties.Resources.ZipCodeHandlerTitle, Rawzip);
                    if (string.IsNullOrWhiteSpace(zip))
                    {
                        if (MessageBox.Show(Properties.Resources.ZipCodeErrorMessage, 
                            Properties.Resources.ZipCodeErrorTitle, 
                            MessageBoxButtons.RetryCancel, 
                            MessageBoxIcon.Error) == DialogResult.Cancel)
                        {
                            Application.Exit();
                        }
                    }
                }
            }
            Rawzip = zip;
            return zip;
        }

        public static string Rawzip
        {
            get => EncryptedAppSettingsHandler.Read("zipcode");
            
            set => EncryptedAppSettingsHandler.Write("zipcode", value);
            
        }

        public static string TryGetZip()
            => (string.IsNullOrWhiteSpace(Rawzip)) ? GetZip(): Rawzip;   
    }
}
