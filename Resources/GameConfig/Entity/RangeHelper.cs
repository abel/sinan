using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Sinan.Util;

namespace Sinan.Entity
{
    public class RangeHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="range"></param>
        /// <param name="fourfold">是否扩大选择区</param>
        /// <returns></returns>
        public static Rectangle NewRectangle(Variant range, bool fourfold = false)
        {
            if (range == null) return Rectangle.Empty;
            int x = range.GetIntOrDefault("X");
            int y = range.GetIntOrDefault("Y");
            int w = range.GetIntOrDefault("W");
            int h = range.GetIntOrDefault("H");
            if (fourfold)
            {
                return new Rectangle(x - w, y - h, w << 1, h << 1);
            }
            return new Rectangle(x, y, w, h);
        }

        public static Point NewPoint(Variant point)
        {
            if (point == null) return Point.Empty;
            int x = point.GetIntOrDefault("X");
            int y = point.GetIntOrDefault("Y");
            return new Point(x, y);
        }
    }
}
