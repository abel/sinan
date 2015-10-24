using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Sinan.Extensions;
using Sinan.Util;
using Sinan.Data;
using Sinan.GameModule;

namespace Sinan.Entity
{
    /// <summary>
    /// 暗雷(跟场景关联)
    /// </summary>
    public class HideApc : VisibleApc
    {
        /// <summary>
        /// 所在场景
        /// </summary>
        public string SceneID
        {
            get;
            private set;
        }

        /// <summary>
        /// 矩形范围
        /// </summary>
        public Rectangle Range
        {
            get;
            set;
        }

        public HideApc(Variant config)
            : base(config)
        {
            ID = config.GetStringOrDefault("_id");
            Variant v = config.GetVariantOrDefault("Value");
            if (v != null)
            {
                SceneID = v.GetStringOrDefault("SceneID");
                Range = RangeHelper.NewRectangle(v["Range"] as Variant, true);
            }
        }

    }
}
