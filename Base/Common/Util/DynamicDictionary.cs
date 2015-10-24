using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Sinan.Common.Util
{
    public class DynamicDictionary<T> : IDynamicMetaObjectProvider
        where T : class,IDictionary<string, Object>, new()
    {
        private T storage = new T();
        public int Count
        { get { return storage.Count; } }

        public DynamicMetaObject GetMetaObject(System.Linq.Expressions.Expression parameter)
        {
            return new DynamicDictionaryMetaObject<T>(parameter, this);
        }

        public Object SetDictionaryEntry(string key, Object value)
        {
            storage[key] = value;
            return value;
        }

        public Object GetDictionaryEntry(string key)
        {
            Object result;
            storage.TryGetValue(key, out result);
            return result;
        }
    }

    class DynamicDictionaryMetaObject<T> : DynamicMetaObject
        where T : class,IDictionary<string, Object>, new()
    {
        public DynamicDictionaryMetaObject(System.Linq.Expressions.Expression parameter, Object obj)
            : base(parameter, BindingRestrictions.Empty, obj)
        {
        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            const string methodName = "SetDictionaryEntry";
            BindingRestrictions restrictions = BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType);
            Expression[] args = new Expression[]
            {
                Expression.Constant(binder.Name),
                Expression.Convert(value.Expression, typeof(Object)),
            };

            Expression self = Expression.Convert(this.Expression, this.LimitType);
            Expression methodCall = Expression.Call(self, typeof(DynamicDictionary<T>).GetMethod(methodName), args);

            DynamicMetaObject setDictionaryEntry = new DynamicMetaObject(methodCall, restrictions);
            return setDictionaryEntry;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            const string methodName = "GetDictionaryEntry";
            BindingRestrictions restrictions = BindingRestrictions.GetTypeRestriction(this.Expression, this.LimitType);
            Expression[] args = new Expression[] { Expression.Constant(binder.Name) };

            Expression self = Expression.Convert(this.Expression, this.LimitType);
            Expression methodCall = Expression.Call(self, typeof(DynamicDictionary<T>).GetMethod(methodName), args);

            DynamicMetaObject getDictionaryEntry = new DynamicMetaObject(methodCall, restrictions);
            return getDictionaryEntry;
        }

        private DynamicMetaObject ReturnIdentity(string name)
        {
            return new DynamicMetaObject(this.Expression, BindingRestrictions.GetTypeRestriction(this.Expression, typeof(DynamicDictionary<T>)));
        }
    }

}
