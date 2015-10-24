using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Sinan.Extensions
{
    public static class StringEncoding
    {
        /// <summary>
        /// Encodes to Base64
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Base 64 Encoded string</returns>
        public static string Base64StringEncode(this string val)
        {
            byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(val);
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
        /// <summary>
        /// Decodes a Base64 encoded string
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Base 64 decoded string</returns>
        public static string Base64StringDecode(this string val)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(val);
            string returnValue = ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
            return returnValue;
        }
        /// <summary>
        /// Left pads the passed string using the HTML non-breaking space ( ) for the total number of spaces. 
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="totalSpaces">Total number of spaces to add</param>
        /// <returns>string after adding the Html Spaces(&nbsp)</returns>
        public static string PadLeftHtmlSpaces(this string val, int totalSpaces)
        {
            string space = "&nbsp;";
            return StringConver.PadLeft(val, space, val.Length + (totalSpaces * space.Length));
        }
        /// <summary>
        /// Right pads the passed string using the HTML non-breaking space ( ) for the total number of spaces. 
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="totalSpaces">total number of spaces to add</param>
        /// <returns>string after adding the Html Spaces(&nbsp)</returns>
        public static string PadRightHtmlSpaces(this string val, int totalSpaces)
        {
            string space = "&nbsp;";
            return StringConver.PadRight(val, space, val.Length + (totalSpaces * space.Length));
        }


        /// <summary>
        /// Encrypts a string to using MD5 algorithm
        /// </summary>
        /// <param name="val"></param>
        /// <returns>string representation of the MD5 encryption</returns>
        public static string MD5String(this string val)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(val));

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        /// <summary>
        /// Verifies the string against the encrypted value for equality
        /// </summary>
        /// <param name="val"></param>
        /// <param name="hash">The encrypted value of the string</param>
        /// <returns>true is the given string is equal to the string encrypted</returns>
        public static bool VerifyMD5String(this string val, string hash)
        {
            string hashOfInput = MD5String(val);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return 0 == comparer.Compare(hashOfInput, hash) ? true : false;
        }
        /// <summary>
        /// Converts all spaces to HTML non-breaking spaces
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string SpaceToNbsp(this string val)
        {
            string space = "&nbsp;";
            return val.Replace(" ", space);
        }
        /// <summary>
        /// Removes all HTML tags from the passed string.
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string StripTags(this string val)
        {
            Regex stripTags = new Regex("<(.|\n)+?>");
            return stripTags.Replace(val, string.Empty);
        }
        /// <summary>
        /// Converts each new line (\n) and carriage return (\r) symbols to the HTML <br /> tag.
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static string NewLineToBreak(this string val)
        {
            Regex regEx = new Regex(@"[\n|\r]+");
            return regEx.Replace(val, "<br/>");
        }


        /// <summary>
        /// 字符串压缩
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>得到压缩流</returns>
        public static Stream GZipCompress(string str)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            MemoryStream msReturn;
            using (MemoryStream msTemp = new MemoryStream())
            {
                using (GZipStream gz = new GZipStream(msTemp, CompressionMode.Compress, true))
                {
                    gz.Write(buffer, 0, buffer.Length);
                    gz.Close();
                    msReturn = new MemoryStream(msTemp.GetBuffer(), 0, (int)msTemp.Length);
                }
            }
            return msReturn;
        }

        /// <summary>
        /// 字符串解压
        /// </summary>
        /// <param name="stream">字符流</param>
        /// <returns>得到解压后的字符串</returns>
        public static string GZipDecompress(Stream stream)
        {
            byte[] buffer = new byte[100];
            int length = 0;
            using (GZipStream gz = new GZipStream(stream, CompressionMode.Decompress))
            {
                using (MemoryStream msTemp = new MemoryStream())
                {
                    while ((length = gz.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        msTemp.Write(buffer, 0, length);
                    }
                    return Encoding.UTF8.GetString(msTemp.ToArray());
                }
            }
        }

        /// <summary>
        /// NET系统方法HttpUtility.UrlEncode会将‘=’编码成‘%3d’，而不是%3D，
        /// 导致加密签名一直通不过验证，请开发者注意检查。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string FixUrlEncode(this string url)
        {
            char[] newurl = url.ToCharArray();
            for (int i = 0; i < newurl.Length - 2; i++)
            {
                if (newurl[i] == '%')
                {
                    char f = newurl[i + 1];
                    if (f >= 'a' && f <= 'f')
                    {
                        newurl[i + 1] = (char)(f - ('a' - 'A'));
                    }
                    char s = newurl[i + 2];
                    if (s >= 'a' && s <= 'f')
                    {
                        newurl[i + 2] = (char)(s - ('a' - 'A'));
                    }
                    i += 2;
                }
            }
            return new string(newurl);
        }

        ///// <summary>
        ///// 签名验证时，要求对字符串中除了(-)，(_)，(.)之外的所有非字母数字字符都替换成百分号(%)后跟两位十六进制数。
        ///// 十六进制数中字母必须为大写。
        ///// </summary>
        ///// <param name="url"></param>
        ///// <returns></returns>
        //public static string UrlEncode(this string url)
        //{
        //    StringBuilder sb = new StringBuilder(url.Length * 2);
        //    foreach (char c in url)
        //    {
        //        if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '_' || c == '.')
        //        {
        //            sb.Append(c);
        //        }
        //        else
        //        {
        //            sb.Append('%');
        //            sb.Append(((byte)c).ToString("X2"));
        //        }
        //    }
        //    return sb.ToString();
        //}
    }
}
