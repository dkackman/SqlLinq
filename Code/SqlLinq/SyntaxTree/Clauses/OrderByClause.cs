using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;

namespace SqlLinq.SyntaxTree.Clauses
{
    [SyntaxNode(RuleConstants.RULE_ORDERCLAUSE_ORDER_BY)]
    public class OrderByClause : NonTerminalNode
    {
        public OrderByClause()
        {
        }

        public Func<IEnumerable<T>, IEnumerable<T>> CreateEvaluator<T>()
        {
            ParameterExpression param = Expression.Parameter(typeof(IEnumerable<T>), "arg");

            IEnumerable<OrderByItem> items = OrderByItems; // this does a recursive search - cache it since we are using it twice
            OrderByItem first = items.First();
            Debug.Assert(first != null);

            // since ordering after the first uses the ThenBy* methods first create the OrderBy call
            // and then all of the subsequent ThenBy valls
            MethodCallExpression call = first.CreateExpression(param, typeof(T));
            
            foreach (OrderByItem orderby in items.Skip(1)) // skip the first one because that was used above
                call = orderby.CreateThenByExpression(call, typeof(T));

            return Expression.Lambda<Func<IEnumerable<T>, IEnumerable<T>>>(call, param).Compile();
        }

        public IEnumerable<OrderByItem> OrderByItems
        {
            get
            {
                return FindDescendants<OrderByItem>();
            }
        }
    }
}
