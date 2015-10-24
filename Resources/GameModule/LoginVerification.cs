using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Sinan.Extensions;
using Sinan.FastJson;
using Sinan.FastSocket;
using Sinan.Log;
using Sinan.Security;
using Sinan.Security.Cryptography;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 登录验证
    /// </summary>
    public class LoginVerification
    {
        string m_md5Key; // = ConfigurationManager.AppSettings["MD5Key"];
        string m_desKey; // = ConfigurationManager.AppSettings["DESKey"];

        public LoginVerification(string loginKey, string desKey)
        {
            this.m_md5Key = loginKey;
            this.m_desKey = desKey;
        }

        /// <summary>
        /// 解码Token(腾讯方式)
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public Variant DecryptTencentToken(string[] tokens)
        {
            string test = MD5Helper.MD5Encrypt(m_md5Key + tokens[1] + tokens[2]);
            if (test != tokens[0].ToUpper())
            {
                return null;
            }

            Variant token = new Variant(2);
            token["uid"] = tokens[1];
            token["time"] = double.Parse(tokens[2]);
            if (tokens.Length > 3)
            {
                token["key"] = tokens[3];
                if (tokens.Length > 4)
                {
                    switch (tokens[4])
                    {
                        case "qzone":
                            token["domain"] = Domain.Qzone;
                            break;
                        case "pengyou":
                            token["domain"] = Domain.Pengyou;
                            break;
                        case "weibo":
                            token["domain"] = Domain.Weibo;
                            break;
                        case "qqgame":
                            token["domain"] = Domain.QQGame;
                            break;
                    }
                    if (tokens.Length > 5)
                    {
                        int zoneid;
                        if (int.TryParse(tokens[5], out zoneid))
                        {
                            token["sid"] = zoneid;
                        }
                        if (tokens.Length > 7)
                        {
                            int t;
                            if (int.TryParse(tokens[7], out  t))
                            {
                                token["is_yellow_vip"] = t;
                            }
                            if (tokens.Length > 8)
                            {
                                if (int.TryParse(tokens[8], out  t))
                                {
                                    token["is_yellow_year_vip"] = t;
                                }
                                if (tokens.Length > 9)
                                {
                                    if (int.TryParse(tokens[9], out  t))
                                    {
                                        token["yellow_vip_level"] = t;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return token;
        }

        /// <summary>
        /// 解码Token(快速登录方式)
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public Variant DecryptFastToken(string token)
        {
            string dToken = DESHelper.Decrypt(token, m_desKey).Trim('\'');
            return JsonConvert.DeserializeObject<Variant>(dToken);
        }

        /// <summary>
        /// 解码Token(趣游戏登录/充值)
        /// </summary>
        /// <param name="authBase64"></param>
        /// <returns></returns>
        public Variant DecryptGamewaveToken(string authBase64, string sig)
        {
            string test = MD5Helper.MD5Encrypt(authBase64 + m_md5Key);
            if (test != sig.ToUpper())
            {
                return null;
            }
            string auth = StringEncoding.Base64StringDecode(authBase64);
            Variant token = new Variant();
            string[] kvs = auth.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var kv in kvs)
            {
                int index = kv.IndexOf('=');
                if (index > -1)
                {
                    string key = kv.Substring(0, index);
                    string value = kv.Substring(index + 1);
                    token.Add(key, value);
                }
            }
            token["auth"] = auth;
            return token;
        }

        public Variant DecryptGameflierTokey()
        {
            //http://openid.gameflier.com/WebAuth/dotoken
            //?oauth_obj=authtest
            //&oauth_consumer_key=21ed2fa6120a3b591dca1982dcb6acd3
            //&oauth_signature_method=HmacSHA1
            //&oauth_signature=13d7f862f7af158ba29d8201f9869eb3942c1600
            //&oauth_timestamp=1312523999119
            //&oauth_noice=ejdif20eff901
            //&oauth_version=v1.0


            //失敗後返回值json格式：
            //{"res":"10043","message":"making access token failed.."}
            //=>res:錯誤代碼
            //=>message:錯誤原因

            const string oauth_obj = "authtest";
            const string oauth_consumer_key = "21ed2fa6120a3b591dca1982dcb6acd3";
            const string oauth_consumer_secret = "30ed2fa6000a3b591dca1222dcb6acd3";
            //加密用的隨機字串
            string oauth_noice = Password.CreatPassword(8);
            long oauth_timestamp = (long)(UtcTimeExtention.NowTotalSeconds());

            String encodeStr = String.Format("{0}.{1}A{2}={3}", oauth_obj, oauth_consumer_key, oauth_timestamp, oauth_noice);

            HMACSHA1 hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(oauth_consumer_secret));
            byte[] hashValue = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(encodeStr));

            StringBuilder bui = new StringBuilder();
            for (int i = 0; i < hashValue.Length; i++)
            {
                bui.Append(String.Format("{0:x2}", hashValue[i]));
            }
            String oauth_signature = bui.ToString();
            bui.Clear();
            bui.Append("/WebAuth/dotoken");
            bui.Append("?oauth_obj=");
            bui.Append(oauth_obj);
            bui.Append("&oauth_consumer_key=");
            bui.Append(oauth_consumer_key);
            bui.Append("&oauth_signature_method=HmacSHA1");
            bui.Append("&oauth_signature=");
            bui.Append(oauth_signature);
            bui.Append("&oauth_timestamp=");
            bui.Append(oauth_timestamp);
            bui.Append("&oauth_noice=");
            bui.Append(oauth_noice);
            bui.Append("&oauth_version=v1.0");
            string url = bui.ToString();
            Variant v = Route(url);


            //成功後返回值json格式：
            //{"res":"1","oauth_obj":"authtest","oauth_consumer_key":"21ed2fa6120a3b591dca1982dcb6acd3","access_token":"df2027191af43de1e24e6d77de3916aa.1312524543265_athkra1a"}
            //=>res: 1代表成功
            //=>oauth_obj:貴方的遊戲類別名稱
            //=>oauth_consumer_key:貴方的oauth_consumer_key
            //=>access_token:此次session，auth認證所需要的token(重要)
            int res = v.GetIntOrDefault("res", -1);
            if (res == 1)
            {
                string access_token = v.GetStringOrDefault("access_token");
            }
            return v;
        }

        HttpClient httpc;

        /// <summary>
        /// 路由到游戏(发货)服务器
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public Variant Route(string url)
        {
            if (httpc == null)
            {
                httpc = new HttpClient("openid.gameflier.com", 80);
            }
            try
            {
                string result = httpc.Get(url);
                return JsonConvert.DeserializeObject<Variant>(result);
            }
            catch (System.Exception ex)
            {
                LogWrapper.Error(url + ex);
            }
            return null;
        }
    }
}
