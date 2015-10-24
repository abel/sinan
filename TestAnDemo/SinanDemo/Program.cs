using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using Sinan.Diagnostics;
using Sinan.FastSocket;
using Sinan.Log;
using Tencent.OpenSns;
using Sinan.Extensions;
using Sinan.Security.Cryptography;


namespace Sinan.Demo
{
    class Program
    {
        const string zero32 = "00000000000000000000000000000000";
        static string opopenid = "WE345ED";
        static StringBuilder sbPad = new StringBuilder(32 * 1000);
        static void testPad()
        {
            sbPad.Clear();
            for (int i = 0; i < 1000; i++)
            {
                sbPad.Append(opopenid.PadLeft(32, '0'));
            }
        }

        static void testzero()
        {
            sbPad.Clear();
            for (int i = 0; i < 1000; i++)
            {
                if (opopenid.Length < 32)
                {
                    sbPad.Append(zero32, 0, 32 - opopenid.Length);
                }
                sbPad.Append(opopenid);
            }
        }

        static void testChar()
        {
            sbPad.Clear();
            for (int i = 0; i < 1000; i++)
            {
                if (opopenid.Length < 32)
                {
                    sbPad.Append('0', 32 - opopenid.Length);
                }
                sbPad.Append(opopenid);
            }
        }

        static void Main()
        {
            string sig = "1D7560DC02F9148B67CEBFFE5570C538";
            string cmd = "noticeList";
            string par = "[]";
            string time = "1341203395";//((long)(UtcTimeExtention.NowTotalSeconds())).ToString();

            string testsig = MD5Helper.MD5Encrypt(cmd + par + time + "192168100203");
            if (testsig != sig)
            {
                Console.WriteLine("验证错误");
                return;
            }

            TestHttpClient();

            //{"Ver" : 0, "_id" : "3D0CA00BCC64CDA40C89341717181B7E16426", "amt" : 500, "billno" : "-APPDJ18203-20120410-1259072131", "openid" : "0000000000000000000000001F32BF75", "payitem" : "G_C000000*10*5", "pid" : 0, "ppc" : "", "providetype" : 0, "sig" : "FxcgbH25MZALt86Hl5rGG8BJgtY=", "state" : 3, "ts" : NumberLong(1334033947), "zoneid" : 7 }
            //{"Ver" : 0, "_id" : "0A0D832B7E45B153F904B129779F3BB228323", "amt" : 500, "billno" : "-APPDJ18203-20120410-1307352624", "openid" : "0000000000000000000000001F32BF75", "payitem" : "G_C000000*10*5", "pid" : 0, "ppc" : "", "providetype" : 0, "sig" : "Gy44bB7mPWqyI+o4nR84punWlV8=", "state" : 3, "ts" : NumberLong(1334034455), "zoneid" : 7 }
            Order o = new Order();
            o.amt = 500;
            o.billno = "-APPDJ18203-20120410-1259072131";
            o.openid = "0000000000000000000000001F32BF75";
            o.payitem = "G_C000000*10*5";
            o.pid = 775;
            o.ppc = "";
            o.providetype = 0;
            o.state = 0;
            o.token = "3D0CA00BCC64CDA40C89341717181B7E16426";
            o.ts = 1334033947;
            o.Ver = 3;
            o.zoneid = 7;

            string text = o.GetSig(@"/cgi-bin/pay");
            if (text == "Gy44bB7mPWqyI+o4nR84punWlV8=")
            {
                Console.WriteLine("成功");
            }


            TestSign();
            return;

            string test = "a*b+recv中+文测试#$%^&*()fg sadfe";
            string st = HttpUtility.UrlEncode(test);
            Console.WriteLine(st);

            string st2 = UrlUtility.UrlEncode(test);
            string st3 = UrlUtility.UrlEncode(test);
            if (st3 != st2)
            {
                Console.WriteLine(st2);
            }

            CodeTimer.Initialize();

            CodeTimer.Time("testPad", 1000, testPad);
            CodeTimer.Time("testzero", 1000, testzero);
            CodeTimer.Time("testChar", 1000, testChar);

            string res = "Sinan.resources.dll";

            int index = res.IndexOf('.');
            if (index >= 0)
            {
                Console.WriteLine(res.Substring(0, index));
                Console.WriteLine(res.Substring(index + 1));
            }

            Assembly assembly = Assembly.LoadFrom(res);
            System.Resources.ResourceManager cn = new System.Resources.ResourceManager("Sinan.Text.zh-CN", assembly);
            string x = cn.GetString("Age");

            System.Resources.ResourceManager tw = new System.Resources.ResourceManager("Sinan.Text.zh-TW", assembly);
            string x2 = tw.GetString("Age");

            //CoverDBHelper.CoverDB();
            Console.WriteLine("按任意键退出......");
            Console.Read();
        }

        private static void TestHttpClient()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("192.168.100.40"), 8200);
            HttpArgs arg = new HttpArgs();
            //arg.Body = "192.168.100.40";
            arg.Host = "192.168.100.40";
            arg.Url = "/gm?sig=32&cmd=noticeList&par=[\"dfs\",23,34]";
            byte[] getResult = HttpClient.Get(ep, arg);

            string xd = System.Text.Encoding.UTF8.GetString(getResult);
            Console.WriteLine(xd);


            HttpClient httpc = new HttpClient("192.168.100.40", 8200);
            string xd2 = httpc.Get(arg.Url);
            bool xd1Txd2 = (xd == xd2);
            Console.WriteLine(xd1Txd2);
        }


        static void TestSign()
        {
            var v = Encoding.GetEncoding(936);
            string meta = Convert.ToBase64String(Encoding.GetEncoding(936).GetBytes("10枚晶币*每10Q点可以兑换10枚晶币，祝您游戏愉快，谢谢您的支持！"));
            BuyGoodsRequest request = new BuyGoodsRequest();
            request.appmode = 2;
            request.device = 0;
            request.goodsmeta = meta;
            request.goodsurl = "http://ww.com.cn/test.png";
            request.openid = "00000000000000000000000000441794";
            request.openkey = "8EABE7C3586EA666327DFA1A2A38F088";
            request.payitem = "x*100*1";
            request.pf = "pengyou";
            request.pfkey = "7cbfc07c376f92b07e3bce209ee1ed47";
            request.zoneid = 5;

            const string host = "119.147.19.43";
            const string uri = "/v3/pay/buy_goods";

            StringBuilder sb = new StringBuilder(1000);
            string quest = request.Build(sb, null).ToString();
            string sign = AppSign.Sign(uri, quest);
            LogWrapper.Warn("签名源:" + quest);
            LogWrapper.Warn("签名结果:" + sign);

            sb.Clear();
            sb.Append(uri + "?");
            request.Build(sb);
            sb.Append("&sig=");
            sb.Append(sign);


            string xd = request.ToString();
            string result2 = "https://" + host + sb.ToString();
            Console.WriteLine(result2);

            HttpArgs a = new HttpArgs();
            a.Host = host;
            a.Url = sb.ToString();
            LogWrapper.Warn("请求内容:" + a.Url);

            IPEndPoint ipadd = new IPEndPoint(IPAddress.Parse(host), 443);
            byte[] bin = SslHttpClient.Get(ipadd, a, null);
            string result = Encoding.UTF8.GetString(bin);
            Console.WriteLine(result);
            LogWrapper.Warn("请求结果:" + result);
        }
    }
}
