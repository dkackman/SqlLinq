using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

using SqlLinq.SyntaxTree;
using SqlLinq.SyntaxTree.Expressions;

namespace SqlLinq
{
    public static class SqlExpression
    {
        public static Func<T, object> Compile<T>(string expression)
        {
            try
            {
                var parser = new SqlParser();
                if (!parser.Parse("SELECT * FROM this WHERE " + expression))
                {
                    throw new SqlException(string.Format(CultureInfo.CurrentCulture, "SQL parse error:\n\t{0}\nin expression\n\t{1}", parser.ErrorString, expression));
                }
                var node = ((SelectNode)parser.SyntaxTree).WhereClause.FindChild<ExpressionNode>();
                var source = Expression.Parameter(typeof(IEnumerable<T>), "source");
                var item = Expression.Parameter(typeof(T), "item");
                var function = Expression.Lambda<Func<IEnumerable<T>, T, object>>(Expression.Convert(node.CreateExpression(source, item), typeof(object)), source, item).Compile();

                return t => function(Enumerable.Empty<T>(), t);
            }
            catch (Exception e) when (!(e is SqlException))
            {
                throw new SqlException(string.Format(CultureInfo.CurrentCulture, "SQL compile error:\n\t{0}\nin expression\n\t{1}", e.Message, expression), e);
            }
        }
    }
}
