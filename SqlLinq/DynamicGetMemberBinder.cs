using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Dynamic;

namespace SqlLinq
{
    class DynamicGetMemberBinder : GetMemberBinder
    {
        public DynamicGetMemberBinder(string name)
            : base(name, true)
        {

        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            var d = target.Value as IDictionary<string, object>;
            if (d == null)
            {
                throw new ArgumentNullException(this.Name);
            }

            MethodInfo indexerMethod = typeof(IDictionary<string, object>).GetMethod("get_Item", new Type[] { typeof(string) });
            var call = Expression.Call(Expression.Constant(d), indexerMethod, Expression.Constant(this.Name));

            return DynamicMetaObject.Create(d, call);
        }
    }
}
