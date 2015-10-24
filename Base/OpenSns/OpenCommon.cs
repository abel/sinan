using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tencent.OpenSns
{
    public class OpenCommon
    {
        /// <summary>
        /// QQ号码转化得到的ID。用户的key，访问OpenAPI时必需
        /// </summary>
        public string openid
        {
            get;
            set;
        }

        /// <summary>
        /// session key，访问OpenAPI时必需
        /// </summary>
        public string openkey
        {
            get;
            set;
        }

        /// <summary>
        /// 应用的唯一ID。可以通过appid查找APP基本信息。
        /// </summary>
        public string appid
        {
            get;
            set;
        }

        /// <summary>
        /// 请求串的签名
        /// </summary>
        public string sig
        {
            get;
            set;
        }

        /// <summary>
        /// 表示应用的来源平台。值列表包括但不仅限于如下：
        /// qzone：空间；pengyou：朋友；qplus：Q+；tapp：微博；qqgame：QQGame；3366：3366。
        /// 后缀加上_m代表来自手机，如：pengyou_m：手机朋友
        /// </summary>
        public string pf
        {
            get;
            set;
        }

        /// <summary>
        /// 定义API返回的数据格式。
        /// 
        /// 取值说明：为xml时表示返回的格式是xml；为json时表示返回的格式是json。
        /// 注意：json、xml为小写，否则将不识别。format不传或非xml，则返回json格式数据
        /// </summary>
        public string format
        {
            get;
            set;
        }

        /// <summary>
        /// 用户的IP
        /// </summary>
        public string userip
        {
            get;
            set;
        }



      
    }
}
