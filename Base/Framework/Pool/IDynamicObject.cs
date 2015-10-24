using System;

namespace Sinan.Pool
{
    public interface IDynamicObject
    {
        void Create(Object param);
        Object GetInnerObject();
        bool IsValidate();
        void Release();
    }
}
