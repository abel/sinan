using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (39XX)
    /// </summary>
    public class TitleCommand
    {
        /// <summary>
        /// 获取所有称号信息
        /// </summary>
        public const string GetTitles = "getTitles";
        public const string GetTitlesR = "t.getTitlesR";

        /// <summary>
        /// 获取单个称号信息
        /// </summary>
        public const string GetTitle = "getTitle";
        public const string GetTitleR = "t.getTitleR";

        /// <summary>
        /// 设置称号
        /// </summary>
        public const string SetTitle = "setTitle";
        public const string SetTitleR = "t.setTitleR";

        /// <summary>
        /// 获取自己的称号
        /// </summary>
        public const string MyTitle = "myTitle";
        public const string MyTitleR = "t.myTitleR";
    }
}
