using System;
using System.Linq;
using System.Linq.Expressions;

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

                NodeWithId groupByField = GroupByClause.GroupByItems.First();
                string key = groupByField.Id.LookupId;

                if (!Columns.GetFieldList().Contains(key, StringComparer.OrdinalIgnoreCase))
                    throw new SqlException(string.Format("The query contains the GROUP BY field '{0}' that is not included in the result list.", key));
            }

            if ((Columns.Distinct || GroupByClause != null) && OrderByClause != null)
            {
                var selectFields = Columns.GetFieldList();
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
