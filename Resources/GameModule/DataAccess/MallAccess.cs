using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.Entity;
using Sinan.Util;
using System.IO;
using System.Collections;
using System.Net;
using System.Xml;
using System.Web;

namespace Sinan.GameModule
{
    
    public class MallAccess
    {
        static string _path;
        /// <summary>
        /// 商城上架列表
        /// </summary>
        static HashSet<string> hs = new HashSet<string>();
        public static HashSet<string> HS 
        {
            get { return hs; }
        }


        static Dictionary<string,Variant> partlist = new Dictionary<string, Variant>();
        /// <summary>
        /// 活动列有
        /// </summary>
        public static Dictionary<string, Variant> PartList 
        {
            get { return partlist; }
        }

        public static void Load(string path) 
        {
            _path = path;
            using (StreamReader sr = new StreamReader(_path, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    string soleid = sr.ReadLine();
                    if (string.IsNullOrEmpty(soleid))
                        continue;
                    GameConfig gc = GameConfigAccess.Instance.FindOneById(soleid);
                    if (gc == null)
                        continue;
                    //非商城物品
                    if (gc.MainType != "Mall")
                        continue;
                    if (!hs.Contains(soleid))
                    {
                        hs.Add(soleid);
                    }
                }
            }
        }

        /// <summary>
        /// 更新商城信息
        /// </summary>
        /// <param name="list"></param>
        public static bool UpdateMell(IList list)
        {
            try
            {
                //更新商城信息
                hs.Clear();
                StringBuilder sb = new StringBuilder();
                foreach (string soleid in list)
                {
                    GameConfig gc = GameConfigAccess.Instance.FindOneById(soleid);
                    if (gc == null) 
                        continue;
                    //非商城物品
                    if (gc.MainType != "Mall")
                        continue;

                    if (!hs.Contains(soleid))
                    {
                        hs.Add(soleid);
                        sb.Append(soleid + "\r\n");
                    }
                }

                using (FileStream fs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(sb.ToString());
                }
                return true;
            }
            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// 活动列表更新
        /// </summary>
        /// <param name="list"></param>
        public static void UpdatePartList(IList list)
        {
            partlist.Clear();

            //活动列表
            foreach (Variant v in list) 
            {
                string id = v.GetStringOrDefault("ID");
                partlist.Add(id, v);
            }
        }

        /// <summary>
        /// 充值活动
        /// </summary>
        public static List<Variant> CoinPart() 
        {
            List<string> parttype = new List<string>() { "CoinSupp", "CoinAchieve" };
            List<Variant> cp = new List<Variant>();
            Dictionary<string, Variant> dic = partlist;
            if (dic != null)
            {
                foreach (var k in dic)
                {
                    Variant v = k.Value;
                    if (v == null)
                        continue;
                    string subtype = v.GetStringOrDefault("SubType");
                    if (parttype.Contains(subtype))
                    {
                        cp.Add(v);
                    }
                }
            }
            return cp;
        }

        /// <summary>
        /// 取得活动基本信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Variant PartInfo(string id)
        {
            Variant v;
            partlist.TryGetValue(id, out v);
            return v;
        }

        /// <summary>
        /// WebService,GET请求
        /// </summary>
        /// <param name="url">WebService地址</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="v">参数</param>
        public static XmlNodeList QueryGetWebService(string url, string methodName, Variant v)
        {
            using (WebClient wc = new WebClient())
            {
                StringBuilder sb = new StringBuilder();
                string ur = url.TrimEnd('/');
                sb.Append(ur + "/");
                sb.Append(methodName + "?");
                foreach (var item in v)
                {
                    sb.Append(item.Key + "=" + item.Value + "&");
                }
                string info = sb.ToString().TrimEnd('&');
                Stream stream = null;
                try
                {
                    stream = wc.OpenRead(info);
                }
                catch
                {
                    return null;
                }
                if (stream == null)
                {
                    return null;
                }

                using (StreamReader sr = new StreamReader(stream, Encoding.UTF8))
                {
                    string str = sr.ReadToEnd();
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(str);
                    return xmldoc.ChildNodes;
                }
            }
        }

        /// <summary>
        /// WebService,POST请求
        /// </summary>
        /// <param name="url">WebService地址</param>
        /// <param name="method">方法名称</param>
        /// <param name="v">参数</param>
        /// <returns></returns>
        public static XmlNodeList QueryPostWebService(string url, string method, Variant v)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url + "/" + method);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Credentials = CredentialCache.DefaultCredentials;
                request.Timeout = 5000;
                StringBuilder sb = new StringBuilder();
                foreach (var item in v)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append("&");
                    }
                    sb.Append(HttpUtility.UrlEncode(item.Key.ToString()) + "=" + HttpUtility.UrlEncode(item.Value.ToString()));
                }

                byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
                request.ContentLength = data.Length;
                Stream stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();

                WebResponse wr = request.GetResponse();
                using (StreamReader sr = new StreamReader(wr.GetResponseStream(), Encoding.UTF8))
                {
                    string srd = sr.ReadToEnd();
                    XmlDocument xmldoc = new XmlDocument();
                    xmldoc.LoadXml(srd);
                    return xmldoc.ChildNodes;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
