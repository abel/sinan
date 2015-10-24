using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using MongoDB.Bson.Serialization.Attributes;
using Sinan.Extensions;
using Tencent.OpenSns;

namespace Sinan.Log
{
    public class ShipInfo
    {
        public string GoodID;
        public int Price;
        public int Num;
    }

    public enum ShipState
    {
        /// <summary>
        /// 客户生成token,进行绑定
        /// </summary>
        Create = 0,

        /// <summary>
        /// 接收到腾讯的发货通知
        /// </summary>
        ReceQQ = 1,

        /// <summary>
        /// 成功回复腾讯发货通知.
        /// </summary>
        ResponseQQ = 2,

        /// <summary>
        /// 成功通知游戏服务器准备分货
        /// </summary>
        RouteGame = 3,

        /// <summary>
        /// 游戏服务器接收成功
        /// </summary>
        GameRece = 4,

        /// <summary>
        /// 服务服务器发货成功
        /// </summary>
        GameShip = 5,

        /// <summary>
        /// 错误()
        /// </summary>
        Error = 100,
    }

    /// <summary>
    /// 订单信息
    /// </summary>
    [Serializable]
    [BsonIgnoreExtraElementsAttribute]
    public class Order
    {
        /// <summary>
        /// 版本号
        /// </summary>
        [BsonIgnore]
        public string version = "v3";

        /// <summary>
        /// QQ号码转化得到的ID
        /// </summary>
        public string openid
        {
            get;
            set;
        }

        /// <summary>
        /// unix时间戳（从UTC时间1970年1月1日00:00:00到当前时刻的秒数）
        /// </summary>
        public long ts
        {
            get;
            set;
        }

        /// <summary>
        /// 请使用ID*price*num的格式
        /// ID表示物品ID，price表示单价,num表示建议的购买数量。
        /// </summary>
        public string payitem
        {
            get;
            set;
        }

        /// <summary>
        /// 支付的总金额
        /// (注意，这里以0.1Q点为单位。即如果总金额为18Q点，则这里显示的数字是180)
        /// </summary>
        public int amt
        {
            get;
            set;
        }

        /// <summary>
        /// 应用调用v3/pay/buy_goods接口成功返回的交易号token_id
        /// </summary>
        [BsonId]
        public string token
        {
            get;
            set;
        }

        /// <summary>
        /// 支付流水号（64个字符长度。该字段和openid合起来是唯一的）。
        /// (趣游版为订单号跟token相同)
        /// </summary>
        public string billno
        {
            get;
            set;
        }

        /// <summary>
        /// 多区多服应用某一个大区的ID号。
        /// 如果是非多区多服应用，这里默认返回0。
        /// </summary>
        public int zoneid
        {
            get;
            set;
        }

        /// <summary>
        /// 发货类型，0表示道具购买，1表示营销赠送。
        /// (趣游版表示订单类型)
        /// </summary>
        public int providetype
        {
            get;
            set;
        }

        ///// <summary>
        ///// 扣取的游戏币总数
        ///// 新接入支付的应用只允许扣取Q点，因此这里不会返回该参数值
        ///// </summary>
        //public int payamt_coins
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// pubacct_payamt_coins
        /// 扣取的抵用券总金额（抵用券是指腾讯赠送的Q点）。
        /// 单位为1Q点。可以为空。
        /// </summary>
        public string ppc
        {
            get;
            set;
        }

        /// <summary>
        /// 请求串的签名，由需要签名的参数生成
        /// 签名方法详见腾讯腾讯开放平台应用签名参数sig的说明。
        /// </summary>
        public string sig
        {
            get;
            set;
        }

        /// <summary>
        /// 玩家ID
        /// </summary>
        public int pid
        {
            get;
            set;
        }

        /// <summary>
        /// (趣游用为Money)
        /// </summary>
        public double money
        {
            get;
            set;
        }

        /// <summary>
        /// 订单状态
        /// 0:客户生成token,进行绑定
        /// 1:接收到腾讯的发货通知.
        /// 2:成功回复腾讯发货通知(已扣款).
        /// 3:通知游戏服务器发货,返回发货成功(2-->3)
        /// 4:通知游戏服务器发货,返回发货失败(2-->4)
        /// 5:通知腾讯已成功,但可能超时(1-->5) 需人工核对后,将此状态由5改回2
        /// 6:通知对方已成功,但签名有错误(1-->6) 需人工核实
        /// 
        /// 11:游戏服务收到发货通知(0-->11状态为0时才可以补全,补全后状态设置为10)
        /// 12:游戏服务器准备发货(11-->12)
        /// 13:游戏服务器发货成功(12-->13)
        /// 14:游戏服务器发货失败(12-->14)
        /// </summary>
        public int state
        {
            get;
            set;
        }

        /// <summary>
        /// 版本号
        /// </summary>
        public int Ver
        {
            get;
            set;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Created
        {
            get;
            set;
        }

        /// <summary>
        /// 原始地址
        /// </summary>
        public string Url
        {
            get;
            set;
        }

        [BsonIgnore]
        public long Check;

        public int Coin
        {
            get;
            set;
        }

        public ShipInfo[] GetShipInfos()
        {
            string[] gs = payitem.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            ShipInfo[] infos = new ShipInfo[gs.Length];
            for (int i = 0; i < gs.Length; i++)
            {
                string[] x = gs[i].Split('*');
                ShipInfo info = new ShipInfo();
                info.GoodID = x[0];
                info.Price = int.Parse(x[1]);
                info.Num = int.Parse(x[2]);
                infos[i] = info;
            }
            return infos;
        }

        public string GetSig(string uri)
        {
            StringBuilder sb = new StringBuilder(1024 - 256);
            Build(sb, UrlUtility.UriEncode);
            return Tencent.OpenSns.AppSign.Sign(uri, sb.ToString());
        }

        /// <summary>
        /// 构建字符串表示形式
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="urlEncode">签名时为flase,发送时为ture</param>
        /// <returns></returns>
        public StringBuilder Build(StringBuilder sb, Func<string, string> encode = null)
        {
            #region 排序后键
            //amt
            //appid
            //billno
            //openid
            //payamt_coins
            //payitem
            //providetype
            //pubacct_payamt_coins
            //token
            //ts
            //version
            //zoneid
            #endregion
            sb.Append("amt=");
            sb.Append(this.amt);
            sb.Append("&appid=");
            sb.Append(Tencent.OpenSns.AppSign.appid);
            sb.Append("&billno=");
            sb.Append(encode != null ? encode(billno) : billno);
            sb.Append("&openid=");
            sb.Append(this.openid);
            //新接入支付的应用只允许扣取Q点，因此这里不会返回该参数值
            sb.Append("&payamt_coins=");

            sb.Append("&payitem=");
            sb.Append(encode != null ? encode(payitem) : payitem);

            sb.Append("&providetype=");
            sb.Append(this.providetype.ToString());
            sb.Append("&pubacct_payamt_coins=");
            sb.Append(this.ppc);
            sb.Append("&token=");
            sb.Append(this.token);
            sb.Append("&ts=");
            sb.Append(this.ts.ToString());
            sb.Append("&version=");
            sb.Append(version);
            sb.Append("&zoneid=");
            sb.Append(this.zoneid.ToString());
            return sb;
        }

        public bool CheckSig(string uri)
        {
            string sig = GetSig(uri);
            return sig == this.sig;
        }

        public static Order CreateShipment(Dictionary<string, string> querise)
        {
            Order log = new Order();
            log.Created = DateTime.UtcNow;
            log.amt = int.Parse(querise.GetStringOrDefault("amt"));
            log.billno = querise.GetStringOrDefault("billno");
            log.openid = querise.GetStringOrDefault("openid");
            log.payitem = querise.GetStringOrDefault("payitem");
            log.version = querise.GetStringOrDefault("version") ?? "v3";

            int providetype;
            if (int.TryParse(querise.GetStringOrDefault("providetype"), out providetype))
            {
                log.providetype = providetype;
            }

            log.ppc = querise.GetStringOrDefault("pubacct_payamt_coins");
            log.sig = querise.GetStringOrDefault("sig");
            log.token = querise.GetStringOrDefault("token");

            long ts;
            if (long.TryParse(querise.GetStringOrDefault("ts"), out ts))
            {
                log.ts = ts;
            }

            string zone = querise.GetStringOrDefault("zoneid");
            if (!string.IsNullOrEmpty(zone))
            {
                if (zone[0] == 'S' || zone[0] == 's')
                {
                    log.zoneid = int.Parse(zone.Substring(1));
                }
                else
                {
                    log.zoneid = int.Parse(zone);
                }
            }
            return log;
        }
    }

    public enum OrderResult
    {
        /// <summary>
        /// 未知
        /// </summary>
        Nono = 0,

        /// <summary>
        /// 成功
        /// </summary>
        Success = 1,

        /// <summary>
        /// 无效服务器编号
        /// </summary>
        InvalidSid = 10,

        /// <summary>
        /// 无效玩家帐号
        /// </summary>
        InvalidUid = 11,

        /// <summary>
        /// 订单已存在
        /// </summary>
        OrderExists = 12,

        /// <summary>
        /// 无效订单类型
        /// </summary>
        InvalidOtype = 13,

        /// <summary>
        /// 无效时间戳
        /// </summary>
        InvalidTime = 14,

        /// <summary>
        /// 充值金额错误
        /// </summary>
        AmtError = 15,

        /// <summary>
        /// 虚拟币数量错误
        /// </summary>
        CoinError = 16,

        /// <summary>
        /// 校验码错误
        /// </summary>
        SignError = 17,

        /// <summary>
        /// 其它错误
        /// </summary>
        OtherError = 18,
    }

}
