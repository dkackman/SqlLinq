using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

using SqlLinq.SyntaxTree.Aggregates;
using SqlLinq.SyntaxTree.Expressions;
using SqlLinq.SyntaxTree.Clauses;

namespace SqlLinq.SyntaxTree
{
    [SyntaxNode(RuleConstants.RULE_SELECTSTM_SELECT)]
    public class SelectNode : NonTerminalNode
    {
        public SelectNode()
        {
        }

        internal bool IsScalarHint
        {
            get
            {
                return Columns.ColumnSources.Count() == 1 && Columns.IsAggregateOnlyQuery;
            }
        }

        public Columns Columns
        {
            get
            {
                // SELECT <field list> and SELECT * have slightly different structures
                return FindChild<Columns>(RuleConstants.RULE_COLUMNS) ?? FindChild<Columns>(RuleConstants.RULE_COLUMNS_TIMES);
            }
        }

        public FromClause FromClause
        {
            get
            {
                return FindChild<FromClause>();
            }
        }

        public WhereClause WhereClause
        {
            get
            {
                return FindChild<WhereClause>(RuleConstants.RULE_WHERECLAUSE_WHERE);
            }
        }

        public OrderByClause OrderByClause
        {
            get
            {
                return FindChild<OrderByClause>();
            }
        }

        public GroupByClause GroupByClause
        {
            get
            {
                return FindChild<GroupByClause>();
            }
        }

        public HavingClause HavingClause
        {
            get
            {
                return FindChild<HavingClause>(RuleConstants.RULE_HAVINGCLAUSE_HAVING);
            }
        }

        internal override void CheckSyntax()
        {
            if (GroupByClause != null)
            {
                if (!Columns.Aggregates.Any())
                    throw new SqlException("At least one aggregate function must be present along with a GROUP BY clause.\n");

                var groupByKeys = GroupByClause.GroupByItems.Select(g => g.Id.LookupId);
                var nonAggregateSelectKeys = Columns.ColumnSources.Where(c => !(c is AggregateNode)).Select(c => c.Id.LookupId);

                var difference = new HashSet<string>(groupByKeys, StringComparer.OrdinalIgnoreCase);
                difference.SymmetricExceptWith(nonAggregateSelectKeys);
                if (difference.Count > 0)
                    throw new SqlException(string.Format("The query contains the field '{0}' that is not matched between the select list and group by clause.", difference.First()));
            }
            else if (Columns.Aggregates.Any() && !Columns.IsAggregateOnlyQuery)
                throw new SqlException("Your query contains fields and aggregates but no GROUP BY clause.\n");

            if ((Columns.Distinct || GroupByClause != null) && OrderByClause != null)
            {
                var selectFields = Columns.FieldList;
                foreach (OrderByItem item in OrderByClause.OrderByItems)
                {
                    if (!selectFields.Contains(item.Id.LookupId, StringComparer.OrdinalIgnoreCase))
                        throw new SqlException(string.Format("The query specifies DISTINCT or GROUP BY but contains an ORDER BY field '{0}' that is not included in the result list.", item.Id.LookupId));
                }
            }

            base.CheckSyntax();
        }
    }
}
