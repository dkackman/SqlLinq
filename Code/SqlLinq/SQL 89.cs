enum SymbolConstants : int
{
    SYMBOL_EOF = 0, // (EOF)
    SYMBOL_ERROR = 1, // (Error)
    SYMBOL_COMMENT = 2, // Comment
    SYMBOL_NEWLINE = 3, // NewLine
    SYMBOL_WHITESPACE = 4, // Whitespace
    SYMBOL_MINUSMINUS = 5, // '--'
    SYMBOL_TIMESDIV = 6, // '*/'
    SYMBOL_DIVTIMES = 7, // '/*'
    SYMBOL_MINUS = 8, // '-'
    SYMBOL_EXCLAMEQ = 9, // '!='
    SYMBOL_LPAREN = 10, // '('
    SYMBOL_RPAREN = 11, // ')'
    SYMBOL_TIMES = 12, // '*'
    SYMBOL_COMMA = 13, // ','
    SYMBOL_DIV = 14, // '/'
    SYMBOL_PLUS = 15, // '+'
    SYMBOL_LT = 16, // '<'
    SYMBOL_LTEQ = 17, // '<='
    SYMBOL_LTGT = 18, // '<>'
    SYMBOL_EQ = 19, // '='
    SYMBOL_GT = 20, // '>'
    SYMBOL_GTEQ = 21, // '>='
    SYMBOL_ALL = 22, // ALL
    SYMBOL_AND = 23, // AND
    SYMBOL_AS = 24, // AS
    SYMBOL_ASC = 25, // ASC
    SYMBOL_AVG = 26, // Avg
    SYMBOL_BETWEEN = 27, // BETWEEN
    SYMBOL_BOOLEANLITERAL = 28, // BooleanLiteral
    SYMBOL_BY = 29, // BY
    SYMBOL_COUNT = 30, // Count
    SYMBOL_DATELITERAL = 31, // DateLiteral
    SYMBOL_DESC = 32, // DESC
    SYMBOL_DISTINCT = 33, // DISTINCT
    SYMBOL_FROM = 34, // FROM
    SYMBOL_FUNCTION = 35, // Function
    SYMBOL_GROUP = 36, // GROUP
    SYMBOL_HAVING = 37, // HAVING
    SYMBOL_ID = 38, // Id
    SYMBOL_IN = 39, // IN
    SYMBOL_INNER = 40, // INNER
    SYMBOL_INTEGERLITERAL = 41, // IntegerLiteral
    SYMBOL_IS = 42, // IS
    SYMBOL_JOIN = 43, // JOIN
    SYMBOL_LEFT = 44, // LEFT
    SYMBOL_LIKE = 45, // LIKE
    SYMBOL_MAX = 46, // Max
    SYMBOL_MIN = 47, // Min
    SYMBOL_NOT = 48, // NOT
    SYMBOL_NULL = 49, // NULL
    SYMBOL_ON = 50, // ON
    SYMBOL_OR = 51, // OR
    SYMBOL_ORDER = 52, // ORDER
    SYMBOL_REALLITERAL = 53, // RealLiteral
    SYMBOL_RIGHT = 54, // RIGHT
    SYMBOL_SELECT = 55, // SELECT
    SYMBOL_STDEV = 56, // StDev
    SYMBOL_STDEVP = 57, // StDevP
    SYMBOL_STRINGLITERAL = 58, // StringLiteral
    SYMBOL_SUM = 59, // Sum
    SYMBOL_VAR = 60, // Var
    SYMBOL_VARP = 61, // VarP
    SYMBOL_WHERE = 62, // WHERE
    SYMBOL_ADDEXP = 63, // <Add Exp>
    SYMBOL_AGGREGATE = 64, // <Aggregate>
    SYMBOL_ANDEXP = 65, // <And Exp>
    SYMBOL_COLUMNALIAS = 66, // <Column Alias>
    SYMBOL_COLUMNITEM = 67, // <Column Item>
    SYMBOL_COLUMNLIST = 68, // <Column List>
    SYMBOL_COLUMNSOURCE = 69, // <Column Source>
    SYMBOL_COLUMNS = 70, // <Columns>
    SYMBOL_EXPRLIST = 71, // <Expr List>
    SYMBOL_EXPRESSION = 72, // <Expression>
    SYMBOL_FROMCLAUSE = 73, // <From Clause>
    SYMBOL_GROUPCLAUSE = 74, // <Group Clause>
    SYMBOL_HAVINGCLAUSE = 75, // <Having Clause>
    SYMBOL_IDLIST = 76, // <Id List>
    SYMBOL_IDMEMBER = 77, // <Id Member>
    SYMBOL_INLIST = 78, // <In List>
    SYMBOL_JOIN2 = 79, // <Join>
    SYMBOL_JOINCHAIN = 80, // <Join Chain>
    SYMBOL_MULTEXP = 81, // <Mult Exp>
    SYMBOL_NEGATEEXP = 82, // <Negate Exp>
    SYMBOL_NOTEXP = 83, // <Not Exp>
    SYMBOL_ORDERCLAUSE = 84, // <Order Clause>
    SYMBOL_ORDERLIST = 85, // <Order List>
    SYMBOL_ORDERTYPE = 86, // <Order Type>
    SYMBOL_PREDEXP = 87, // <Pred Exp>
    SYMBOL_QUERY = 88, // <Query>
    SYMBOL_RESTRICTION = 89, // <Restriction>
    SYMBOL_SELECTSTM = 90, // <Select Stm>
    SYMBOL_TUPLE = 91, // <Tuple>
    SYMBOL_VALUE = 92, // <Value>
    SYMBOL_VALUELIST = 93, // <Value List>
    SYMBOL_WHERECLAUSE = 94  // <Where Clause>
};

enum RuleConstants : int
{
    RULE_QUERY = 0, // <Query> ::= <Select Stm>
    RULE_SELECTSTM_SELECT = 1, // <Select Stm> ::= SELECT <Columns> <From Clause> <Where Clause> <Group Clause> <Having Clause> <Order Clause>
    RULE_COLUMNS_TIMES = 2, // <Columns> ::= <Restriction> '*'
    RULE_COLUMNS = 3, // <Columns> ::= <Restriction> <Column List>
    RULE_COLUMNLIST_COMMA = 4, // <Column List> ::= <Column Item> ',' <Column List>
    RULE_COLUMNLIST = 5, // <Column List> ::= <Column Item>
    RULE_COLUMNITEM = 6, // <Column Item> ::= <Column Source>
    RULE_COLUMNSOURCE = 7, // <Column Source> ::= <Aggregate>
    RULE_COLUMNSOURCE_ID = 8, // <Column Source> ::= Id
    RULE_COLUMNSOURCE2 = 9, // <Column Source> ::= <Aggregate> <Column Alias>
    RULE_COLUMNSOURCE_ID2 = 10, // <Column Source> ::= Id <Column Alias>
    RULE_COLUMNALIAS_AS_ID = 11, // <Column Alias> ::= AS Id
    RULE_RESTRICTION_ALL = 12, // <Restriction> ::= ALL
    RULE_RESTRICTION_DISTINCT = 13, // <Restriction> ::= DISTINCT
    RULE_RESTRICTION = 14, // <Restriction> ::= 
    RULE_AGGREGATE_COUNT_LPAREN_TIMES_RPAREN = 15, // <Aggregate> ::= Count '(' '*' ')'
    RULE_AGGREGATE_COUNT_LPAREN_RPAREN = 16, // <Aggregate> ::= Count '(' <Expression> ')'
    RULE_AGGREGATE_COUNT_LPAREN_DISTINCT_RPAREN = 17, // <Aggregate> ::= Count '(' DISTINCT <Expression> ')'
    RULE_AGGREGATE_COUNT_LPAREN_ALL_RPAREN = 18, // <Aggregate> ::= Count '(' ALL <Expression> ')'
    RULE_AGGREGATE_AVG_LPAREN_RPAREN = 19, // <Aggregate> ::= Avg '(' <Expression> ')'
    RULE_AGGREGATE_MIN_LPAREN_RPAREN = 20, // <Aggregate> ::= Min '(' <Expression> ')'
    RULE_AGGREGATE_MAX_LPAREN_RPAREN = 21, // <Aggregate> ::= Max '(' <Expression> ')'
    RULE_AGGREGATE_STDEV_LPAREN_RPAREN = 22, // <Aggregate> ::= StDev '(' <Expression> ')'
    RULE_AGGREGATE_STDEVP_LPAREN_RPAREN = 23, // <Aggregate> ::= StDevP '(' <Expression> ')'
    RULE_AGGREGATE_SUM_LPAREN_RPAREN = 24, // <Aggregate> ::= Sum '(' <Expression> ')'
    RULE_AGGREGATE_VAR_LPAREN_RPAREN = 25, // <Aggregate> ::= Var '(' <Expression> ')'
    RULE_AGGREGATE_VARP_LPAREN_RPAREN = 26, // <Aggregate> ::= VarP '(' <Expression> ')'
    RULE_FROMCLAUSE_FROM = 27, // <From Clause> ::= FROM <Id List> <Join Chain>
    RULE_JOINCHAIN = 28, // <Join Chain> ::= <Join> <Join Chain>
    RULE_JOINCHAIN2 = 29, // <Join Chain> ::= 
    RULE_JOIN_INNER_JOIN_ON_ID_EQ_ID = 30, // <Join> ::= INNER JOIN <Id List> ON Id '=' Id
    RULE_JOIN_LEFT_JOIN_ON_ID_EQ_ID = 31, // <Join> ::= LEFT JOIN <Id List> ON Id '=' Id
    RULE_JOIN_RIGHT_JOIN_ON_ID_EQ_ID = 32, // <Join> ::= RIGHT JOIN <Id List> ON Id '=' Id
    RULE_JOIN_JOIN_ON_ID_EQ_ID = 33, // <Join> ::= JOIN <Id List> ON Id '=' Id
    RULE_WHERECLAUSE_WHERE = 34, // <Where Clause> ::= WHERE <Expression>
    RULE_WHERECLAUSE = 35, // <Where Clause> ::= 
    RULE_GROUPCLAUSE_GROUP_BY = 36, // <Group Clause> ::= GROUP BY <Id List>
    RULE_GROUPCLAUSE = 37, // <Group Clause> ::= 
    RULE_ORDERCLAUSE_ORDER_BY = 38, // <Order Clause> ::= ORDER BY <Order List>
    RULE_ORDERCLAUSE = 39, // <Order Clause> ::= 
    RULE_ORDERLIST_ID_COMMA = 40, // <Order List> ::= Id <Order Type> ',' <Order List>
    RULE_ORDERLIST_ID = 41, // <Order List> ::= Id <Order Type>
    RULE_ORDERLIST_FUNCTION = 42, // <Order List> ::= Function <Order Type>
    RULE_ORDERTYPE_ASC = 43, // <Order Type> ::= ASC
    RULE_ORDERTYPE_DESC = 44, // <Order Type> ::= DESC
    RULE_ORDERTYPE = 45, // <Order Type> ::= 
    RULE_HAVINGCLAUSE_HAVING = 46, // <Having Clause> ::= HAVING <Expression>
    RULE_HAVINGCLAUSE = 47, // <Having Clause> ::= 
    RULE_EXPRESSION_OR = 48, // <Expression> ::= <And Exp> OR <Expression>
    RULE_EXPRESSION = 49, // <Expression> ::= <And Exp>
    RULE_ANDEXP_AND = 50, // <And Exp> ::= <Not Exp> AND <And Exp>
    RULE_ANDEXP = 51, // <And Exp> ::= <Not Exp>
    RULE_NOTEXP_NOT = 52, // <Not Exp> ::= NOT <Pred Exp>
    RULE_NOTEXP = 53, // <Not Exp> ::= <Pred Exp>
    RULE_PREDEXP_BETWEEN_AND = 54, // <Pred Exp> ::= <Add Exp> BETWEEN <Add Exp> AND <Add Exp>
    RULE_PREDEXP_NOT_BETWEEN_AND = 55, // <Pred Exp> ::= <Add Exp> NOT BETWEEN <Add Exp> AND <Add Exp>
    RULE_PREDEXP_IS_NOT_NULL = 56, // <Pred Exp> ::= <Value> IS NOT NULL
    RULE_PREDEXP_IS_NULL = 57, // <Pred Exp> ::= <Value> IS NULL
    RULE_PREDEXP_LIKE_STRINGLITERAL = 58, // <Pred Exp> ::= <Add Exp> LIKE StringLiteral
    RULE_PREDEXP_NOT_LIKE_STRINGLITERAL = 59, // <Pred Exp> ::= <Add Exp> NOT LIKE StringLiteral
    RULE_PREDEXP_IN = 60, // <Pred Exp> ::= <Add Exp> IN <In List>
    RULE_PREDEXP_NOT_IN = 61, // <Pred Exp> ::= <Add Exp> NOT IN <In List>
    RULE_PREDEXP_EQ = 62, // <Pred Exp> ::= <Add Exp> '=' <Add Exp>
    RULE_PREDEXP_LTGT = 63, // <Pred Exp> ::= <Add Exp> '<>' <Add Exp>
    RULE_PREDEXP_EXCLAMEQ = 64, // <Pred Exp> ::= <Add Exp> '!=' <Add Exp>
    RULE_PREDEXP_GT = 65, // <Pred Exp> ::= <Add Exp> '>' <Add Exp>
    RULE_PREDEXP_GTEQ = 66, // <Pred Exp> ::= <Add Exp> '>=' <Add Exp>
    RULE_PREDEXP_LT = 67, // <Pred Exp> ::= <Add Exp> '<' <Add Exp>
    RULE_PREDEXP_LTEQ = 68, // <Pred Exp> ::= <Add Exp> '<=' <Add Exp>
    RULE_PREDEXP = 69, // <Pred Exp> ::= <Add Exp>
    RULE_ADDEXP_PLUS = 70, // <Add Exp> ::= <Add Exp> '+' <Mult Exp>
    RULE_ADDEXP_MINUS = 71, // <Add Exp> ::= <Add Exp> '-' <Mult Exp>
    RULE_ADDEXP = 72, // <Add Exp> ::= <Mult Exp>
    RULE_ADDEXP2 = 73, // <Add Exp> ::= <Aggregate>
    RULE_MULTEXP_TIMES = 74, // <Mult Exp> ::= <Mult Exp> '*' <Negate Exp>
    RULE_MULTEXP_DIV = 75, // <Mult Exp> ::= <Mult Exp> '/' <Negate Exp>
    RULE_MULTEXP = 76, // <Mult Exp> ::= <Negate Exp>
    RULE_NEGATEEXP_MINUS = 77, // <Negate Exp> ::= '-' <Value>
    RULE_NEGATEEXP = 78, // <Negate Exp> ::= <Value>
    RULE_VALUE = 79, // <Value> ::= <Tuple>
    RULE_VALUE_ID = 80, // <Value> ::= Id
    RULE_VALUE_INTEGERLITERAL = 81, // <Value> ::= IntegerLiteral
    RULE_VALUE_REALLITERAL = 82, // <Value> ::= RealLiteral
    RULE_VALUE_STRINGLITERAL = 83, // <Value> ::= StringLiteral
    RULE_VALUE_NULL = 84, // <Value> ::= NULL
    RULE_VALUE_BOOLEANLITERAL = 85, // <Value> ::= BooleanLiteral
    RULE_VALUE_DATELITERAL = 86, // <Value> ::= DateLiteral
    RULE_VALUE_FUNCTION = 87, // <Value> ::= Function
    RULE_TUPLE_LPAREN_RPAREN = 88, // <Tuple> ::= '(' <Select Stm> ')'
    RULE_TUPLE_LPAREN_RPAREN2 = 89, // <Tuple> ::= '(' <Expr List> ')'
    RULE_INLIST_LPAREN_RPAREN = 90, // <In List> ::= '(' <Value List> ')'
    RULE_VALUELIST_COMMA = 91, // <Value List> ::= <Value> ',' <Value List>
    RULE_VALUELIST = 92, // <Value List> ::= <Value>
    RULE_EXPRLIST_COMMA = 93, // <Expr List> ::= <Expression> ',' <Expr List>
    RULE_EXPRLIST = 94, // <Expr List> ::= <Expression>
    RULE_IDLIST_COMMA = 95, // <Id List> ::= <Id Member> ',' <Id List>
    RULE_IDLIST = 96, // <Id List> ::= <Id Member>
    RULE_IDMEMBER_ID = 97, // <Id Member> ::= Id
    RULE_IDMEMBER_ID_ID = 98  // <Id Member> ::= Id Id
};