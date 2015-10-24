using System;
using System.Collections.Generic;
using System.Text;
using Sinan.Extensions;

namespace Sinan.Security.Cryptography
{
    /// <summary>
    /// 生成随机密码
    /// </summary>
    public class Password
    {
        /// <summary>
        /// 随机生成指定长度的密码
        /// </summary>
        /// <param name="passwordLength">密码长度</param>
        /// <param name="randomchars">密码的组成字符集</param>
        /// <returns></returns>
        public static string CreatPassword(int passwordLength,string randomchars)
        {
            int iRandNum;
            char[] pass = new char[passwordLength];
            System.Random rnd = new System.Random();
            for (int i = 0; i < passwordLength; i++)
            {
                iRandNum = rnd.Next(randomchars.Length);
                pass[i] = randomchars[iRandNum];
            }
            return new string(pass);
        }

        /// <summary>
        /// 随机生成指定长度的密码（由a--z2-9组成）
        /// </summary>
        /// <param name="passwordLength"></param>
        /// <returns></returns>
        public static string CreatPassword(int passwordLength)
        {
            return CreatPassword(passwordLength, "abcdefghijklmnopqrstuvwxyz23456789");
        }

        /// <summary>
        ///  随机生成4-16位数的随机密码（由a--z0-9组成）
        /// </summary>
        /// <returns></returns>
        public static string CreatPassword()
        {
            return CreatPassword(NumberRandom.Next(4, 17));
        }
    }
}