using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tencent.OpenSns
{
    public class BuyGoodsReturn
    {
        /// <summary>
        /// 返回码。详见公共返回码说明#OpenAPI V3.0 返回码
        /// </summary>
        public int ret
        {
            get;
            set;
        }

        /// <summary>
        /// 如果错误，返回错误信息
        /// </summary>
        public string msg
        {
            get;
            set;
        }

        /// <summary>
        /// 判断是否有数据丢失。如果应用不使用cache，不需要关心此参数
        /// </summary>
        public int is_lost
        {
            get;
            set;
        }

        /// <summary>
        /// 交易的token号（ret=0时才保存，token长度不超过64个字符）。
        /// </summary>
        public int token
        {
            get;
            set;
        }

        /// <summary>
        /// ret为0的时候，返回真正购买物品的url参数
        /// </summary>
        public int url_params
        {
            get;
            set;
        }
    }
}
