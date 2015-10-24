using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Cache
{
    public sealed class WeakReference<T> : System.WeakReference
        where T : class
    {
        public WeakReference(T target = null)
            : base(target, false)
        {
        }

        public new T Target
        {
            get { return (T)(base.Target); }
            set { base.Target = value; }
        }
    }

}
