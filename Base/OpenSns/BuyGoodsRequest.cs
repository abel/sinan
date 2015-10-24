using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tencent.OpenSns
{
    /// <summary>
    /// 用户发起的购买请求,将此请求发送给腾讯,用于获取交易的token号
    /// </summary>
    public class BuyGoodsRequest : OpenCommon
    {
        public BuyGoodsRequest()
        {
            ts = (long)Sinan.Extensions.UtcTimeExtention.NowTotalSeconds();
            this.appid = AppSign.appid;
        }

        /// <summary>
        /// 表示平台的信息加密串。由平台直接传给应用，应用原样传给平台即可
        /// </summary>
        public string pfkey
        {
            get;
            set;
        }

        /// <summary>
        /// 表示建议购买的所有物品的总价格，即物品的单价*建议数量
        /// </summary>
        public int amt
        {
            get;
            set;
        }

        /// <summary>
        /// 开发者生成的时间
        /// </summary>
        public long ts
        {
            get;
            set;
        }

        /// <summary>
        /// 设备类型， 0表示web，1表示手机，默认为0
        /// </summary>
        public int device
        {
            get;
            set;
        }

        /// <summary>
        /// 请使用ID*price*num的格式，批量购买套餐物品则用“;”分隔，
        /// 长度必须小于512字符，字符串中不能包含"|"特殊字符
        /// </summary>
        public string payitem
        {
            get;
            set;
        }

        /// <summary>
        /// 1表示用户不可以修改物品数量，2表示用户可以选择购买物品的数量。
        /// 默认为2；批量购买套餐时，必须等于1。
        /// </summary>
        public int appmode
        {
            get;
            set;
        }

        /// <summary>
        /// 物品信息，格式必须是“name*des”，批量购买套餐时也只能有1个道具名称和1个描述，
        /// 即给出该套餐的名称和描述。长度必须小于256字符，必须使用utf8编码。
        /// 发送请求前请对goodsmeta进行base64 encode；目前goodsmeta超过76个字符后不能添加回车字符
        /// </summary>
        public string goodsmeta
        {
            get;
            set;
        }

        /// <summary>
        /// 物品的图片url，用户购买物品的确认支付页面将显示该物品图片。
        /// 长度小于512字符,注意图片规格为：116 * 116 px
        /// </summary>
        public string goodsurl
        {
            get;
            set;
        }

        /// <summary>
        /// 分区ID，应用可以在发货server配置页面自助化添加，平台会自动分配本ID。
        /// 回调发货的时候，根据ID调用应用对应的发货server IP，实现分区发货。
        /// 如果在配置页面配置分区发货，则必须传递本字段。如果没有配置则无需传递本字段
        /// </summary>
        public int zoneid
        {
            get;
            set;
        }

        /// <summary>
        /// 构建字符串表示形式
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="encode">对值进行编码.为空是不编码</param>
        /// <returns></returns>
        public StringBuilder Build(StringBuilder sb, Func<string, string> encode = null)
        {
            this.amt = 30;
            //无userip
            //sb.Append("amt=");
            //sb.Append(this.amt);

            sb.Append("appid=");
            sb.Append(this.appid);

            sb.Append("&appmode=");
            sb.Append(this.appmode);
            sb.Append("&device=");
            sb.Append(this.device);

            //if (!string.IsNullOrEmpty(format))
            //{
            //    sb.Append("&format=");
            //    sb.Append(this.format);
            //}
            //else
            //{
            //    sb.Append("&format=json");
            //}

            sb.Append("&goodsmeta=");
            sb.Append(encode != null ? encode(goodsmeta) : goodsmeta);

            sb.Append("&goodsurl=");
            sb.Append(encode != null ? encode(goodsurl) : goodsurl);

            sb.Append("&openid=");
            sb.Append(this.openid);
            sb.Append("&openkey=");
            sb.Append(this.openkey);

            sb.Append("&payitem=");
            sb.Append(encode != null ? encode(payitem) : payitem);

            sb.Append("&pf=");
            sb.Append(this.pf);
            sb.Append("&pfkey=");
            sb.Append(this.pfkey);
            sb.Append("&ts=");
            sb.Append(ts);
            //if (!string.IsNullOrEmpty(userip))
            //{
            //    sb.Append("&userip=");
            //    sb.Append(this.userip);
            //}
            return sb;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(512);
            return this.Build(sb, null).ToString();
        }
    }
}
