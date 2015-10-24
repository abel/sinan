using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;

namespace Sinan.Extensions
{
    /// <summary>
    /// 数据检验类
    /// </summary>
    public static partial class StringValidate
    {
        private static Regex RegNumber = new Regex("^[0-9]+$");
        private static Regex RegNumberSign = new Regex("^[+-]?[0-9]+$");
        private static Regex RegDecimal = new Regex("^[0-9]+[.]?[0-9]+$");
        //等价于^[+-]?\d+[.]?\d+$
        private static Regex RegDecimalSign = new Regex("^[+-]?[0-9]+[.]?[0-9]+$");
        //w 英文字母或数字的字符串，和 [a-zA-Z0-9] 语法一样 
        private static Regex RegEmail = new Regex("^[\\w-]+@[\\w-]+\\.(com|net|org|edu|mil|tv|biz|info)$");
        private static Regex RegCHZN = new Regex("[\u4e00-\u9fa5]");
        //数字和字母
        private static Regex RegLetterAndNumber = new Regex("^[A-Za-z0-9]+$");
        private static Regex RegDateTime = new Regex(@"((0?[13578]|10|12)(-|\/)((0[0-9])|([12])([0-9]?)|(3[01]?))(-|\/)((\d{4})|(\d{2}))|(0?[2469]|11)(-|\/)((0[0-9])|([12])([0-9]?)|(3[0]?))(-|\/)((\d{4}|\d{2})))");

        #region 数字字符串检查
        /// <summary>
        /// 是否是数字和字母组成的字符串
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static bool IsAllLetterOrNumber(this string inputData)
        {
            return RegLetterAndNumber.Match(inputData).Success;
        }

        /// <summary>
        /// 检查字符串的长度是否在最大长度和最小长度间
        /// </summary>
        /// <param name="inputdata">string 要检验的字符串</param>
        /// <param name="minLength">最小长度</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns></returns>
        public static bool CheckLength(this string inputdata, int minLength, int maxLength)
        {
            if (inputdata.Length > maxLength || inputdata.Length < minLength)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 是否数字字符串
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsNumber(this string inputData)
        {
            return RegNumber.Match(inputData).Success;
        }
        /// <summary>
        /// 是否数字字符串可带正负号
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsNumberSign(this string inputData)
        {
            return RegNumberSign.Match(inputData).Success;
        }
        /// <summary>
        /// 是否是浮点数
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsDecimal(this string inputData)
        {
            return RegDecimal.Match(inputData).Success;
        }
        /// <summary>
        /// 是否是浮点数可带正负号
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsDecimalSign(this string inputData)
        {
            return RegDecimalSign.Match(inputData).Success;
        }
        #endregion

        #region 中文检测
        /// <summary>
        /// 检测是否有中文字符
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static bool IsHasCHZN(this string inputData)
        {
            return RegCHZN.Match(inputData).Success;
        }

        #endregion

        #region 邮件地址
        /// <summary>
        /// 是否是浮点数可带正负号
        /// </summary>
        /// <param name="inputData">输入字符串</param>
        /// <returns></returns>
        public static bool IsEmail(this string inputData)
        {
            return RegEmail.Match(inputData).Success;
        }
        #endregion

        #region 日期格式检验
        /// <summary>
        /// 是否是日期格式的字符串
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static bool IsDateTime(this string inputData)
        {
            return RegDateTime.Match(inputData).Success;
        }
        #endregion
    }
}
