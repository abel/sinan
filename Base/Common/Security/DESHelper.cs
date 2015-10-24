using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Sinan.Security.Cryptography;

namespace Sinan.Security
{
    /// <summary>
    /// DES加密解密帮助类
    /// </summary>
    public class DESHelper
    {
        static string defaultKey;
        //static readonly Byte[] defaultIV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        static DESHelper()
        {
            //取cpu序列号 
            defaultKey = "!s@i#n$a%n^&*";
            defaultKey = defaultKey.Substring(0, 8);
        }

        static byte[] processKey(string key)
        {
            if (String.IsNullOrEmpty(key))
            {
                key = defaultKey;
            }
            byte[] data = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] hashvalue = (new MD5CryptoServiceProvider()).ComputeHash(data);
            Byte[] byKey = new Byte[8];
            for (int i = 0; i < byKey.Length; i++)
            {
                byKey[i] = hashvalue[i];
            }
            //System.Buffer.BlockCopy(hashvalue, 0, byKey, 0, 8);
            return byKey;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="input">要加密的字符串</param>
        /// <param name="strEncrKey">加密密钥</param>
        /// <returns>加密后的字符串</returns>
        public static string EncryptPro(string input, string strEncrKey)
        {
            Byte[] byKey = processKey(strEncrKey);
            return Encrypt(input, byKey);
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="input">要加密的字符串</param>
        /// <param name="byKey">加密密钥.8个字节的数组.64位</param>
        /// <returns>加密后的字符串</returns>
        private static string Encrypt(string input, Byte[] byKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.Zeros;
            Byte[] inputByteArray = Encoding.UTF8.GetBytes(input);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, byKey), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    byte[] buffer = ms.ToArray();
                    return Convert.ToBase64String(buffer);
                }
            }
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="input">要解密的字符串</param>
        /// <param name="sDecrKey">密钥</param>
        /// <returns>原始字符串</returns>
        public static string DecryptPro(String input, String sDecrKey)
        {
            Byte[] byKey = processKey(sDecrKey);
            return Decrypt(input, byKey);
        }

        ///// <summary>
        ///// 解密
        ///// </summary>
        ///// <param name="input">要解密的字符串</param>
        ///// <param name="byKey">密钥</param>
        ///// <returns>原始字符串</returns>
        //public static string Decrypt(String input, Byte[] byKey)
        //{
        //    Byte[] inputByteArray = Convert.FromBase64String(input);
        //    DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        //    des.Mode = CipherMode.ECB;
        //    des.Padding = PaddingMode.Zeros;
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, byKey), CryptoStreamMode.Write))
        //        {
        //            cs.Write(inputByteArray, 0, inputByteArray.Length);
        //            cs.FlushFinalBlock();
        //            byte[] buffer = ms.ToArray();
        //            return System.Text.Encoding.UTF8.GetString(buffer);
        //        }
        //    }
        //}


        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="input">明文字符串</param> 
        /// <param name="skey">密码</param>
        /// <returns>已加密16进制字符串</returns>
        public static string Encrypt(string input, string skey)
        {
            int length = input.Length % 8;
            if (length > 0)
                input = input.PadRight(input.Length + (8 - length));

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.Zeros;
            des.Key = ASCIIEncoding.ASCII.GetBytes(skey);

            byte[] inputByteArray = ASCIIEncoding.ASCII.GetBytes(input);

            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);

            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            cs.Close();

            StringBuilder sb = new StringBuilder();
            foreach (byte n in ms.ToArray())
                sb.AppendFormat("{0:x2}", n);
            return sb.ToString();
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="input">加密字符串(16进制)</param>
        /// <param name="skey">密码</param>
        /// <returns>解密的明文</returns>
        public static string Decrypt(string input, string skey)
        {
            return Decrypt(input, ASCIIEncoding.ASCII.GetBytes(skey));
        }


        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="input">加密字符串(16进制)</param>
        /// <param name="skey">密码</param>
        /// <returns>解密的明文</returns>
        public static string Decrypt(string input, byte[] skey)
        {
            byte[] inputByteArray = new byte[input.Length / 2];
            for (int x = 0; x < input.Length / 2; x++)
            {
                int i = (Convert.ToInt32(input.Substring(x * 2, 2), 16));
                inputByteArray[x] = (byte)i;
            }

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.Zeros;
            des.Key = skey;
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return System.Text.Encoding.UTF8.GetString(ms.ToArray());
            }
        }

    }
}