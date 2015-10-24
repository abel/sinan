using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Data
{
    /// <summary>
    /// 支持自动更新的对象接口.
    /// </summary>
    public interface IPersistable
    {
        bool Changed { get; set; }
        bool Save();
    }
}
