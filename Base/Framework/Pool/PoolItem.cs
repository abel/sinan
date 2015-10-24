using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Pool
{
    class PoolItem<T> where T : IDynamicObject,new()
    {
        private T _object;
        private bool _using;
        private Object _createParam;

        public PoolItem(Object param)
        {
            _createParam = param;
            Create();
        }

        private void Create()
        {
            _using = false;
            _object = new T();
            _object.Create(_createParam);
        }

        public void Recreate()
        {
            _object.Release();
            Create();
        }

        public void Release()
        {
            _object.Release();
        }

        public Object InnerObject
        {
            get { return _object.GetInnerObject(); }
        }

        public int InnerObjectHashcode
        {
            get { return InnerObject.GetHashCode(); }
        }

        public bool IsValidate
        {
            get { return _object.IsValidate(); }
        }

        public bool Using
        {
            get { return _using; }
            set { _using = value; }
        }
    }// class PoolItem
}
