using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Net.NetworkInformation;
using System.Net.Mail;
using System.Net.Mime;

namespace Steganography
{
    public class Utility
    {
        public static string GetLoadingImageFilePath()
        {
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            string fullFilePath = Path.Combine(appPath, "person_icon.png");
            return fullFilePath.Substring(6);
        }

        public string EncryptingText(string password)
        {
            string text = string.Empty;
            byte[] encode = new byte[password.Length];
            encode = Encoding.UTF8.GetBytes(password);
            text = Convert.ToBase64String(encode);
            return text;
        }

        public string DecryptingText(string encryptedPassword)
        {
            string decryptedPassword = string.Empty;
            UTF8Encoding encodedPassword = new UTF8Encoding();
            Decoder Decode = encodedPassword.GetDecoder();
            byte[] toDecodeByte = Convert.FromBase64String(encryptedPassword);
            int characterCount = Decode.GetCharCount(toDecodeByte, 0, toDecodeByte.Length);
            char[] decodedCharacters = new char[characterCount];
            Decode.GetChars(toDecodeByte, 0, toDecodeByte.Length, decodedCharacters, 0);
            decryptedPassword = new String(decodedCharacters);
            return decryptedPassword;
        }

        //to check for internet connection, we ping google's website using their IP. If the website returns success then, it returns true that internet connection exists.
        public static bool InternetConnectionExists()
        {
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingReply pingStatus = ping.Send(System.Net.IPAddress.Parse("208.69.34.231"), 5000);
            if (pingStatus.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

    }
}
