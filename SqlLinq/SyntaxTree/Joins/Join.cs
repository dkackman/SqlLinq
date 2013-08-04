using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using SqlLinq.SyntaxTree.Expressions;

namespace SqlLinq.SyntaxTree.Joins
{
    public abstract class Join : NonTerminalNode
    {
        private Lazy<Identifier> _leftId;
        private Lazy<Identifier> _rightId;

        protected Join()
        {
            _leftId = new Lazy<Identifier>(() => new Identifier(FindChild<TerminalNode>(4).Text));
            _rightId = new Lazy<Identifier>(() => new Identifier(FindChild<TerminalNode>(6).Text));
            //Enumerable.Join(,,,,)
        }

        internal Func<IEnumerable<TOuter>, IEnumerable<TInner>, IEnumerable<JoinResultSelector<TOuter, TInner>>> CreateJoinFunction<TOuter, TInner>()
        {
            Type keyType = typeof(TOuter);
            if (!OuterKey.Equals("this", StringComparison.OrdinalIgnoreCase))
                keyType = typeof(TOuter).GetFieldType(OuterKey);

            var outer = Expression.Parameter(typeof(TOuter), "outerKey");
            var inner = Expression.Parameter(typeof(TInner), "innerKey");

            var outerKeySelector = CreateKeySelector<TOuter>(OuterKey, keyType, outer);
            var innerKeySelector = CreateKeySelector<TInner>(InnerKey, keyType, inner);

            var _new = Expression.New(typeof(JoinResultSelector<TOuter, TInner>).GetConstructor(new Type[] { typeof(object), typeof(object) }), outer, inner);

            var resultSelector = Expression.Lambda(_new, outer, inner);

            var outerData = Expression.Parameter(typeof(IEnumerable<TOuter>), "outer");   // the outer input data
            var innerData = Expression.Parameter(typeof(IEnumerable<TInner>), "inner");   // the inner input data

            var call = Expression.Call(typeof(Enumerable), "Join", new Type[] { typeof(TOuter), typeof(TInner), keyType, typeof(JoinResultSelector<TOuter, TInner>) }, outerData, innerData, outerKeySelector, innerKeySelector, resultSelector);

            return Expression.Lambda<Func<IEnumerable<TOuter>, IEnumerable<TInner>, IEnumerable<JoinResultSelector<TOuter, TInner>>>>(call, outerData, innerData).Compile();
        }

        private static Expression CreateKeySelector<T>(string name, Type keyType, ParameterExpression param)
        {
            // special case for when the key is the data item, not a field of the data item
            if (name.Equals("this", StringComparison.OrdinalIgnoreCase) || name.Equals("that", StringComparison.OrdinalIgnoreCase))
                return ExpressionFactory.CreateIdentitySelector<T, T>();

            return ExpressionFactory.CreateFieldSelectorLambda(param, name, keyType);
        }

        public string OuterName
        {
            get
            {
                return this.FindChild<NodeWithId>().Id.LookupId;
            }
        }

        public string OuterKey
        {
            get
            {
                if (RightId.Qualifier.Equals(OuterName, StringComparison.CurrentCultureIgnoreCase) == false)
                    return RightId.LocalId;

                return LeftId.LocalId;
            }
        }

        public string InnerKey
        {
            get
            {
                if (RightId.Qualifier.Equals(OuterName, StringComparison.CurrentCultureIgnoreCase))
                    return RightId.LocalId;

                return LeftId.LocalId;
            }
        }

        public Identifier LeftId
        {
            get
            {
                return _leftId.Value;
            }
        }

        public Identifier RightId
        {
            get
            {
                return _rightId.Value;
            }
        }
    }
}
