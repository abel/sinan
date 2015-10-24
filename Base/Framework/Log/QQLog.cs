using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Sinan.Log
{
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class QQLog : LogBaseEx
    {
        /// <summary>
        /// 被操作用户 UID
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? touid
        {
            get;
            set;
        }

        /// <summary>
        /// 被操作用户OpenID
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string toopenid
        {
            get;
            set;
        }

        /// <summary>
        /// 操作用户的等级
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? level
        {
            get;
            set;
        }

        /// <summary>
        /// 操作来源
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string source
        {
            get;
            set;
        }

        /// <summary>
        /// 用户操作物品ID
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string itemid
        {
            get;
            set;
        }

        /// <summary>
        /// 用户操作物品ID 的分类
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string itemtype
        {
            get;
            set;
        }

        /// <summary>
        /// 用户操作物品数量
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? itemcnt
        {
            get;
            set;
        }

        /// <summary>
        /// 经验值变化值
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? modifyexp
        {
            get;
            set;
        }

        /// <summary>
        /// 经验值总量
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public long? totalexp
        {
            get;
            set;
        }

        /// <summary>
        /// 虚拟晶币变化值
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? modifycoin
        {
            get;
            set;
        }

        /// <summary>
        /// 虚拟晶币总量
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public long? totalcoin
        {
            get;
            set;
        }

        /// <summary>
        /// 游戏币变化值
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? modifyfee
        {
            get;
            set;
        }

        /// <summary>
        /// 游戏币总量
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public long? totalfee
        {
            get;
            set;
        }

        /// <summary>
        /// 在线时长(秒?)
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? onlinetime
        {
            get;
            set;
        }

        /// <summary>
        /// 安全 key 校验结果
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? keycheckret
        {
            get;
            set;
        }

        /// <summary>
        /// 安全信息上报扩展数据，
        /// 为 binary 类型，对于 key-value 类型的上报，
        /// 为binary 类型的 16 进制字符串表示。
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string safebuf
        {
            get;
            set;
        }

        /// <summary>
        /// 备注 / 用户留言，
        /// 该字段编码时需要长度信息并将该信息编码进来
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public string remark
        {
            get;
            set;
        }

        /// <summary>
        /// 用户在线数量
        /// </summary>
        [BsonIgnoreIfDefaultAttribute]
        public int? user_num
        {
            get;
            set;
        }

        static HashSet<string> needs = new HashSet<string>();
        static QQLog()
        {
            string full = "version=&appid=&userip=&svrip=&time=&domain=&worldid=&optype=&actionid=&opuid=&opopenid=&touid=&toopenid=&level=&source=&itemid=&itemtype=&itemcnt=&modifyexp=&totalexp=&modifycoin=&totalcoin=&modifyfee=&totalfee=&onlinetime=&key=&keycheckret=&safebuf=&remark=&user_num=";
            var ps = full.Split('&');
            foreach (var p in ps)
            {
                needs.Add(p.TrimEnd('='));
            }
        }

        static bool CheckLog(Type t, string x)
        {
            string[] items = x.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            HashSet<string> newX = new HashSet<string>();
            foreach (var item in items)
            {
                int index = item.IndexOf('=');
                if (index > 0)
                {
                    string key = item.Substring(0, index);
                    if (!newX.Add(key))
                    {
                        Console.WriteLine(t.Name + "Duplicate key:" + key);
                    }
                }
            }
            if (newX.Count != needs.Count)
            {
                return false;
            }
            foreach (string v in needs)
            {
                if (!newX.Contains(v))
                {
                    Console.WriteLine(t.Name + "Key lose:" + v);
                    return false;
                }
            }
            if (needs.Count != newX.Count)
            {
                Console.WriteLine(t.Name + "Key lose!");
            }
            return true;
        }

        /// <summary>
        /// 检查宣言的日志类格式
        /// </summary>
        public static void CheckLog()
        {
            StringBuilder sb = new StringBuilder(1024 * 4);
            Assembly a = Assembly.GetExecutingAssembly();
            foreach (Type t in a.GetExportedTypes())
            {
                try
                {
                    if ((!t.IsAbstract) && (!t.IsGenericType) && t.IsSubclassOf(typeof(Sinan.Log.LogBase)))
                    {
                        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                        var defaultConstructor = t.GetConstructor(bindingFlags, null, new Type[0], null);
                        LogBase log = defaultConstructor.Invoke(null) as LogBase;
                        if (log != null)
                        {
                            sb.Clear();
                            QQLog.CheckLog(log.GetType(), log.ToString(sb));
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        const string zero32 = "00000000000000000000000000000000";
        public override string ToString(StringBuilder sb)
        {
            AppendHead(sb);

            sb.Append("&opuid=");
            sb.Append(opuid);
            sb.Append("&opopenid=");
            if (!string.IsNullOrEmpty(opopenid))
            {
                //sb.Append(opopenid.PadLeft(32, '0'));
                if (opopenid.Length < 32)
                {
                    sb.Append(zero32, 0, 32 - opopenid.Length);
                }
                sb.Append(opopenid);
            }

            sb.Append("&key=");
            sb.Append(key);
            sb.Append("&modifyfee=");
            sb.Append(modifyfee);

            sb.Append("&touid=");
            sb.Append(touid);
            sb.Append("&toopenid=");
            if (!string.IsNullOrEmpty(toopenid))
            {
                //sb.Append(toopenid.PadLeft(32, '0')); 
                if (toopenid.Length < 32)
                {
                    sb.Append(zero32, 0, 32 - toopenid.Length);
                }
                sb.Append(toopenid);
            }

            sb.Append("&level=");
            sb.Append(level);
            sb.Append("&source=");
            sb.Append(source);
            sb.Append("&itemid=");
            sb.Append(itemid);
            sb.Append("&itemtype=");
            sb.Append(itemtype);
            sb.Append("&itemcnt=");
            sb.Append(itemcnt);
            sb.Append("&modifyexp=");
            sb.Append(modifyexp);
            sb.Append("&totalexp=");
            sb.Append(totalexp);
            sb.Append("&modifycoin=");
            sb.Append(modifycoin);
            sb.Append("&totalcoin=");
            sb.Append(totalcoin);
            sb.Append("&totalfee=");
            sb.Append(totalfee);
            sb.Append("&remark=");
            sb.Append(remark);

            sb.Append("&onlinetime=");
            sb.Append(onlinetime);
            sb.Append("&keycheckret=");
            sb.Append(keycheckret);
            sb.Append("&safebuf=");
            sb.Append(safebuf);
            sb.Append("&user_num=");
            sb.Append(user_num);

            AppendReserve(sb);
            return sb.ToString();
        }
    }
}
