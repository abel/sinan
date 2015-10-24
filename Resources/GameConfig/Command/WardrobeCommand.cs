using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// 43xx
    /// </summary>
    public class WardrobeCommand
    {
        /// <summary>
        /// 商城试穿
        /// </summary>
        public const string MallDressing = "mallDressing";
        public const string MallDressingR = "o.mallDressingR";
        /// <summary>
        /// 穿时装
        /// </summary>
        public const string Dressing = "dressing";
        public const string DressingR = "o.dressingR";

        /// <summary>
        /// 取消时装
        /// </summary>
        public const string NoDressing = "noDressing";
        public const string NoDressingR = "o.noDressingR";

        /// <summary>
        /// 时装兑换
        /// </summary>
        public const string FashionExchange = "fashionExchange";
        public const string FashionExchangeR = "o.fashionExchangeR";

    }

    public class WardrobeReturn 
    {
        /// <summary>
        /// 时装已经下架
        /// </summary>
        public const int MallDressing1 = 42101;
        /// <summary>
        /// 参数不正确
        /// </summary>
        public const int MallDressing2 = 42102;
        /// <summary>
        /// 非时装
        /// </summary>
        public const int MallDressing3 = 42103;
        /// <summary>
        /// 会员等级不足,不能试穿该时装
        /// </summary>
        public const int MallDressing4 = 42104;
        /// <summary>
        /// 该时装还没有上线
        /// </summary>
        public const int MallDressing5 = 42105;
        /// <summary>
        /// 该时装已经下架
        /// </summary>
        public const int MallDressing6 = 42106;
        /// <summary>
        /// 试穿失败
        /// </summary>
        public const int MallDressing7 = 42107;
        /// <summary>
        /// 试穿成功
        /// </summary>
        public const int MallDressing8 = 42108;
        /// <summary>
        /// 与你的性别不符不能试穿
        /// </summary>
        public const int MallDressing9 = 42109;
        /// <summary>
        /// 该时装已经穿上
        /// </summary>
        public const int MallDressing10 = 42110;


        /// <summary>
        /// 参数不正确
        /// </summary>
        public const int Dressing1 = 42111;
        /// <summary>
        /// 没有购买该时装
        /// </summary>
        public const int Dressing2 = 42112;
        /// <summary>
        /// 试穿失败
        /// </summary>
        public const int Dressing3 = 42113;
        /// <summary>
        /// 试穿成功
        /// </summary>
        public const int Dressing4 = 42114;
        /// <summary>
        /// 
        /// </summary>
        public const int Dressing5= 42115;
        /// <summary>
        /// 没有穿时装
        /// </summary>
        public const int NoDressing1 = 42116;
        /// <summary>
        /// 取取消失败
        /// </summary>
        public const int NoDressing2 = 42117;
        /// <summary>
        /// 时装成功
        /// </summary>
        public const int NoDressing3 = 42118;
        /// <summary>
        /// 
        /// </summary>
        public const int NoDressing4 = 42119;
        /// <summary>
        /// 
        /// </summary>
        public const int NoDressing5 = 42120;

        /// <summary>
        /// 参数不对
        /// </summary>
        public const int FashionExchange1 = 42121;
        /// <summary>
        /// 时装兑换配置不正确
        /// </summary>
        public const int FashionExchange2 = 42122;
        /// <summary>
        /// 兑换不满足
        /// </summary>
        public const int FashionExchange3 = 42123;
        /// <summary>
        /// 兑换需要的时装不起,不能兑换
        /// </summary>
        public const int FashionExchange4 = 42124;
        /// <summary>
        /// 兑换失败
        /// </summary>
        public const int FashionExchange5 = 42125;
        /// <summary>
        /// 兑换成功
        /// </summary>
        public const int FashionExchange6 = 42126;


        /// <summary>
        /// 时装【{0}】已经下架
        /// </summary>
        public const int DownWardrobe1 = 42127;
    }
}
