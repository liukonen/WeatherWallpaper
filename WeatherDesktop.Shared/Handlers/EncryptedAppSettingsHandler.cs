using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace WeatherDesktop.Shared.Handlers
{
    public class EncryptedAppSettingsHandler
    {
        public static string Read(string Key) 
        {
            byte[] entropy = new byte[0]; byte[] decryptedData = new byte[0];

            try
            {
                entropy = Encoding.Unicode.GetBytes(Key);
                string EncryptedString = AppSetttingsHandler.Read(Key);
                decryptedData = ProtectedData.Unprotect(
                    Convert.FromBase64String(EncryptedString), entropy, DataProtectionScope.LocalMachine);
                return Encoding.Unicode.GetString(decryptedData);
            }
            catch { return string.Empty; }
            finally
            {
                Array.Clear(entropy, 0, entropy.Length);
                Array.Clear(decryptedData, 0, decryptedData.Length);
            }
        }

        public static void Write(string Key, string Value) 
        {
            try
            {
                byte[] entropy = Encoding.Unicode.GetBytes(Key);
                var Encrypted = ProtectedData.Protect(Encoding.Unicode.GetBytes(Value), entropy, DataProtectionScope.LocalMachine);
                AppSetttingsHandler.Write(Key, Convert.ToBase64String(Encrypted));
            }
            catch (Exception x)
            { MessageBox.Show(x.Message, "error writing to Config file"); Share.ErrorHandler.LogException(x); }
        }

        public static void Remove(string Key) => AppSetttingsHandler.Remove(Key);


    }
}
