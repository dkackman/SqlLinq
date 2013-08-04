using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace SqlLinq.SyntaxTree.Clauses
{
    [SyntaxNode(RuleConstants.RULE_ORDERLIST_ID_COMMA)]
    [SyntaxNode(RuleConstants.RULE_ORDERLIST_ID)]
    public class OrderByItem : NonTerminalNode
    {
        private Lazy<Identifier> _id;

        public OrderByItem()
        {
            _id = new Lazy<Identifier>(() => new Identifier(GetTerminalText("Id").Trim('[', ']')));
        }

        public Identifier Id
        {
            get
            {
                return _id.Value;
            }
        }

        public MethodCallExpression CreateExpression(Expression param, Type tSource)
        {
            Debug.Assert(param != null);
            Debug.Assert(tSource != null);

            return CreateExpression(param, tSource, "OrderBy");
        }

        public MethodCallExpression CreateThenByExpression(Expression param, Type tSource)
        {
            Debug.Assert(param != null);
            Debug.Assert(tSource != null);

            return CreateExpression(param, tSource, "ThenBy");
        }

        private MethodCallExpression CreateExpression(Expression param, Type tSource, string functionName)
        {
            LambdaExpression lambda = CreateLambaExpression(tSource);

            return Expression.Call(typeof(Enumerable), IsDescending ? functionName + "Descending" : functionName, new Type[] { tSource, lambda.ReturnType }, param, lambda);
        }

        protected virtual LambdaExpression CreateLambaExpression(Type tSource)
        {
            Debug.Assert(tSource != null);
            Debug.Assert(!string.IsNullOrEmpty(Id.LookupId));

            return ExpressionFactory.CreateFieldSelectorLambda(tSource, Id.LookupId);
        }

        public bool IsDescending
        {
            get
            {
                return FindChild<NonTerminalNode>(RuleConstants.RULE_ORDERTYPE_DESC) != null;
            }
        }
    }
}
