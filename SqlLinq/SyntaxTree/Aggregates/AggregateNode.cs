using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Diagnostics;

using SqlLinq.SyntaxTree.Expressions;
using SqlLinq.SyntaxTree.Expressions.Functions;

namespace SqlLinq.SyntaxTree.Aggregates
{
    /// <summary>
    /// Base class for other aggregate nodes
    /// </summary>
    /// <remarks>This is one case where using the parser's TrimReductions feature
    /// makes this code more complicated. TrimReductions will remove nodes from the tree that
    /// are not directly expressed in the input even though they are yielded by the state tables.
    /// For a statement like "SELECT AVG(field) FROM source" no Column Source node ends up in the tree.
    /// However for "SELECT field1, AVG(field2)" or "SELECT AVG(field) AS alias" the aggregate
    /// node will be the child of a column source node. So in order to deal with this difference in the resulting tree
    /// AggregateNode inherits from ColumnSource and can also be the child of ColumnSource</remarks>
    public abstract class AggregateNode : ColumnSource
    {
        protected AggregateNode()
        {
        }

        public string Name { get; protected set; }

        public override string ColumnAlias
        {
            get
            {
                if (Parent is ColumnSource)
                    return ((ColumnSource)Parent).ColumnAlias;

                return base.ColumnAlias;
            }
        }

        protected override Identifier CreateIdentifier()
        {
            if (DoNotDereferenceFields)
                return new Identifier(Name + "Value");

            return new Identifier(Name + SourceFieldName);
        }

        protected string SourceFieldName
        {
            get
            {
                NodeWithId id = FindChild<NodeWithId>();
                Debug.Assert(id != null);
                return id.Id.LocalId;
            }
        }
        public virtual bool DoNotDereferenceFields
        {
            get
            {
                return FindChild<Value>() != null;
            }
        }

        internal override IEnumerable<Identifier> GetFields()
        {
            return FindDescendants<NodeWithId>().Select(n => n.Id);
        }

        internal MethodCallExpression GetCallExpression(Type tSource, Expression param)
        {
            // if the target of the aggrgegate is itself a function
            if (DoNotDereferenceFields)
            {
                // get the method that calculates the aggregate from the source collection
                MethodInfo method = GetEvaluationMethod(param.Type);
                if (method.IsGenericMethodDefinition)
                    method = method.MakeGenericMethod(tSource);

                return Expression.Call(method, param);
            }

            return GetPropertyOrFieldAggregateExpression(tSource, param);
        }

        protected virtual MethodCallExpression GetPropertyOrFieldAggregateExpression(Type tSource, Expression param)
        {
            // otherwise the target of the aggregate is a property on the source type such as Avg(Age)
            Expression lambda = ExpressionFactory.CreateFieldSelectorLambda(tSource, SourceFieldName);

            return Expression.Call(EvaluatatorType, Name, new Type[] { tSource }, param, lambda);
        }

        protected virtual Type EvaluatatorType
        {
            get
            {
                return typeof(Enumerable);
            }
        }

        protected virtual MethodInfo GetEvaluationMethod(Type paramType)
        {
            return EvaluatatorType.GetMethod(Name, new Type[] { paramType });
        }
    }
}
