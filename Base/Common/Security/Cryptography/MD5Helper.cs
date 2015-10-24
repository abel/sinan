using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Sinan.Security.Cryptography
{
    /// <summary>
    /// MD5加密
    /// </summary>
    public class MD5Helper
    {
        byte[] m_Head;
        Encoding m_encoding;

        public MD5Helper(byte[] head)
        {
            if (head == null || head.Length == 0)
            {
                m_Head = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            }
            else
            {
                m_Head = head;
            }
            m_encoding = System.Text.Encoding.UTF8;
        }

        #region
        /// <summary>
        /// MD5加密字符串
        /// </summary>
        /// <param name="data">加密内容</param>
        /// <returns></returns>
        public static string MD5Encrypt(string data)
        {
            byte[] b = System.Text.Encoding.UTF8.GetBytes(data);
            return MD5Encrypt(b);
        }

        /// <summary>
        /// MD5加密字符串
        /// </summary>
        ///<param name="data">加密内容</param>
        /// <returns></returns>
        public static string MD5Encrypt(byte[] data)
        {
            //计算MD5
            byte[] hashvalue = (new MD5CryptoServiceProvider()).ComputeHash(data);
            //计算结果转换为字符串
            StringBuilder sb = new StringBuilder(hashvalue.Length << 1);
            for (int i = 0; i < hashvalue.Length; i++)
            {
                sb.Append(hashvalue[i].ToString("X2"));
            }
            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// 增强的Md5,可防字典破解
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string MD5EncryptPro(string data)
        {
            //a的密码F3D06C3BAB78E423D6757CE8173E0207
            int len = m_encoding.GetByteCount(data);
            int headCount = m_Head.Length;
            byte[] b = new byte[len + headCount];
            System.Buffer.BlockCopy(m_Head, 0, b, 0, headCount);
            m_encoding.GetBytes(data, 0, data.Length, b, headCount);
            return MD5Encrypt(b);
        }
    }
}
