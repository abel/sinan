using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.FastConfig
{
    public interface IConfigManager
    {
        void Load(string path);
        void Unload(string path);
    }
}
