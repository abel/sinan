using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Tencent.OpenSns
{
    /// <summary>
    /// 应用签名
    /// </summary>
    public static class AppSign
    {
        /* 签名验证
         * appid: 33758
         * 源串：GET&%2Fcgi-bin%2Ftemp.py&amt%3D80%26appid%3D33758%26billno%3D%252DAPPDJT18700%252D20120210%252D1428215572%26openid%3Dtest001%26payamt_coins%3D%26payitem%3D323003%2A8%2A1%26providetype%3D0%26pubacct_payamt_coins%3D%26token%3D53227955F80B805B50FFB511E5AD51E025360%26ts%3D1328855301%26version%3Dv3%26zoneid%3D
         * 密钥为：deee17c&
         * 生成的签名为： 4AwdrUL1eD7zkmfeiiFedThpgms=
         * 签名URL编码:   4AwdrUL1eD7zkmfeiiFedThpgms%3D
        */

        /// <summary>
        /// 石器宝贝应用的唯一标识
        /// </summary>
        public const string appid = "32965";
        public const string appkey = "2ec4deb744734a14b2311c3ff537036b";
        public const string appname = "app32965";

        readonly static byte[] key;
        static AppSign()
        {
            key = Encoding.ASCII.GetBytes(appkey + "&");
        }

        static public string Sign(string value)
        {
            byte[] bin = Encoding.UTF8.GetBytes(value);
            return Sign(bin, 0, bin.Length);
        }

        static public string Sign(Sinan.Collections.BytesSegment value)
        {
            return Sign(value.Array, value.Offset, value.Count);
        }

        static public string Sign(byte[] value, int offset, int count)
        {
            HMACSHA1 hmac = new HMACSHA1(key, false);
            byte[] bin = hmac.ComputeHash(value, offset, count);
            string sign = Convert.ToBase64String(bin);
            return sign;
        }

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="uri">URI路径,不含host,不包含?</param>
        /// <param name="value">请求的参数(已编码)</param>
        /// <returns></returns>
        static public string Sign(string uri, string value)
        {
            //源串.
            StringBuilder source = new StringBuilder(4 + uri.Length * 3 + value.Length * 2);
            source.Append("GET&");
            source.AppendUrlString(uri);
            source.Append('&');
            source.AppendUrlString(value);
            string temp = source.ToString();
            return Sign(temp);
        }
    }
}
