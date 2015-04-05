using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Dynamic;
using System.Reflection;

namespace SqlLinq
{
    class DynamicGetMemberBinder : GetMemberBinder
    {
        private readonly static PropertyInfo _indexer = typeof(IDictionary<string, object>).GetProperty("Item");

        public DynamicGetMemberBinder(string name)
            : base(name, true)
        {

        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            var d = target.Value as IDictionary<string, object>;
            if (d == null)
            {
                throw new InvalidOperationException("Target object is not an ExpandoObject");
            }

            return DynamicMetaObject.Create(d, Expression.MakeIndex(Expression.Constant(d), _indexer, new Expression[] { Expression.Constant(this.Name) }));
        }
    }
}
