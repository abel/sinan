using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tencent.OpenSns
{
    public static class UrlUtility
    {
        static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 0x30);
            }
            return (char)((n - 10) + 0x41);
        }

        #region Url
        static bool IsUrlSafeChar(char ch)
        {
            if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9')))
            {
                return true;
            }
            switch (ch)
            {
                case '-':
                case '_':
                case '.':
                    //case '!':
                    //case '(':
                    //case ')':
                    //case '*':
                    return true;
            }
            return false;
        }

        public static StringBuilder AppendUrlString(this StringBuilder sb, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            for (int j = 0; j < bytes.Length; j++)
            {
                ApendUrlChar(sb, (char)bytes[j]);
            }
            return sb;
        }

        private static void ApendUrlChar(StringBuilder sb, char c)
        {
            byte num = (byte)c;
            if (IsUrlSafeChar(c))
            {
                sb.Append(c);
            }
            else
            {
                sb.Append((char)0x25);
                sb.Append(IntToHex((num >> 4) & 15));
                sb.Append(IntToHex(num & 15));
            }
        }

        /// <summary>
        /// Url编码
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder(str.Length << 1);
            return AppendUrlString(sb, str).ToString();
        }
        #endregion

        #region  Uri
        static bool IsUriSafeChar(char ch)
        {
            if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9')))
            {
                return true;
            }
            switch (ch)
            {
                //case '-':
                //case '_':
                //case '.':
                case '*':
                case '!':
                case '(':
                case ')':
                    return true;
            }
            return false;
        }

        public static StringBuilder AppendUriString(this StringBuilder sb, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            for (int j = 0; j < bytes.Length; j++)
            {
                ApendUriChar(sb, (char)bytes[j]);
            }
            return sb;
        }

        private static void ApendUriChar(StringBuilder sb, char c)
        {
            byte num = (byte)c;
            if (IsUriSafeChar(c))
            {
                sb.Append(c);
            }
            else
            {
                sb.Append((char)0x25);
                sb.Append(IntToHex((num >> 4) & 15));
                sb.Append(IntToHex(num & 15));
            }
        }

        /// <summary>
        /// Uri编码
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string UriEncode(string str)
        {
            StringBuilder sb = new StringBuilder(str.Length << 1);
            return AppendUriString(sb, str).ToString();
        }
        #endregion

        #region Base64
        /// <summary>
        /// Base64编码
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Base 64 decoded string</returns>
        public static string Base64StringDecode(string val)
        {
            StringBuilder sb = new StringBuilder(val.Length << 2);
            return AppendBase64String(sb, val).ToString();
        }

        /// <summary>
        /// 以Base64的格式添加
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static StringBuilder AppendBase64String(this StringBuilder sb, string val)
        {
            if (val != null)
            {
                string encodedDataAsBytes = Convert.ToBase64String(Encoding.UTF8.GetBytes(val));
                for (int i = 0; i < encodedDataAsBytes.Length; i++)
                {
                    ApendUrlChar(sb, encodedDataAsBytes[i]);
                }
            }
            return sb;
        }
        #endregion

    }
}
