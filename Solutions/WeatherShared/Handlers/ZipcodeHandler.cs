using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WeatherShared.Handlers
{
    public class ZipcodeHandler
    {
        public static MenuItem ZipMenuItem
        {
            get => new MenuItem("Change Zip Code", ChangeZipClick); 
        }

        public static void ChangeZipClick(object sender, EventArgs e) => GetZip();
        

        /// <summary>
        /// Opens a dialog box, saves zip and returns value. if cancel, closes app gracefully... will retry if not a number
        /// </summary>
        /// <returns></returns>
        public static string GetZip()
        {
            string zip = string.Empty;
            object locker = new object();
            lock (locker)
            {
                while (!int.TryParse(zip, out int zipparse))
                {
                    zip = InputHandler.Input("Please enter your zip code.", Rawzip);
                    if (string.IsNullOrWhiteSpace(zip))
                    {
                        if (MessageBox.Show("Application needs your zip code. try again, or close", "error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
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
            get => AppSettingsHandler.ReadSettingEncrypted("zipcode");
            
            set => AppSettingsHandler.AddupdateAppSettingsEncrypted("zipcode", value);
        }

        public static string TryGetZip() => (string.IsNullOrWhiteSpace(Rawzip)) ? GetZip() : Rawzip;


    }
}
