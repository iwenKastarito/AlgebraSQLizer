using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AlgebraSQLizer
{
    public partial class Dash : Window
    {
        private bool isDrawerOpen = false;
        private bool _isUpdating = false;

        public Dash()
        {
            InitializeComponent();
            RelationalAlgebraTextBox.TextChanged += RelationalAlgebraTextBox_TextChanged;
            QueryTextBox.TextChanged += QueryTextBox_TextChanged;
        }





        public static string ExtractIdentifier(string input)
        {
            // Define the regular expression pattern to match text between [*[ and ]*]
            string pattern = @"\[\*\[(.*?)\]\*\]";

            // Use Regex.Match to find the match in the input string
            Match match = Regex.Match(input, pattern);

            // If a match is found, return the captured group
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                // If no match is found, return the entire string itself
                return input;
            }
        }



















        private void SlideButton_Click(object sender, RoutedEventArgs e)
        {
            Storyboard storyboard = (Storyboard)FindResource(isDrawerOpen ? "SlideOut" : "SlideIn");
            storyboard.Begin();
            isDrawerOpen = !isDrawerOpen;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;
            if (!string.IsNullOrEmpty(message))
            {
                Border messageBorder = new Border
                {
                    BorderBrush = Brushes.Purple,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(5),
                    Padding = new Thickness(5),
                    Background = Brushes.DarkGray
                };
                TextBlock messageTextBlock = new TextBlock
                {
                    Text = "Sender: " + message,
                    Foreground = Brushes.White,
                    FontSize = 14,
                    TextWrapping = TextWrapping.Wrap
                };
                messageBorder.Child = messageTextBlock;
                MessagesContainer.Children.Add(messageBorder);
                ScrollViewer scrollViewer = (ScrollViewer)ChatDrawer.Children[0];
                scrollViewer.ScrollToBottom();
                MessageTextBox.Clear();
            }
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Shift)
            {
                int caretIndex = MessageTextBox.CaretIndex;
                MessageTextBox.Text = MessageTextBox.Text.Insert(caretIndex, Environment.NewLine);
                MessageTextBox.CaretIndex = caretIndex + Environment.NewLine.Length;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                SendButton_Click(sender, e);
                e.Handled = true;
            }
        }

        private void RelationalAlgebraTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            string relationalAlgebraInput = RelationalAlgebraTextBox.Text;
            try
            {
                string sqlQuery = ConvertRelationalAlgebraToSQL(relationalAlgebraInput);
                QueryTextBox.Text = sqlQuery;
            }
            catch (Exception ex)
            {
                QueryTextBox.Text = "Error in Relational Algebra: " + ex.Message;
            }

            _isUpdating = false;
        }

        private void QueryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            string sqlInput = QueryTextBox.Text;
            try
            {
                string relationalAlgebra = ConvertSQLToRelationalAlgebra(sqlInput);
                RelationalAlgebraTextBox.Text = relationalAlgebra + " <||> " +Scanner(sqlInput);
            }
            catch (Exception ex)
            {
                RelationalAlgebraTextBox.Text = "Error in SQL Query: " + ex.Message;
            }

            _isUpdating = false;
        }

        // Conversion from Relational Algebra to SQL
        public static string ConvertRelationalAlgebraToSQL(string algebra)
        {
            // Patterns for different relational operations
            var selectPattern = @"σ\(([^)]+)\)\(([^)]+)\)";
            var projectPattern = @"π\(([^)]+)\)\(([^)]+)\)";
            var unionPattern = @"∪\(([^)]+),\s*([^)]+)\)";
            var differencePattern = @"−\(([^)]+),\s*([^)]+)\)";
            var intersectionPattern = @"∩\(([^)]+),\s*([^)]+)\)";
            var crossProductPattern = @"×\(([^)]+),\s*([^)]+)\)";
            var joinPattern = @"⨝\(([^)]+)\s*,\s*([^)]+)\s*\)";
            var renamePattern = @"ρ\(([^)]+),\s*([^)]+)\)";
            var groupByPattern = @"γ\(([^)]+)\)\(([^)]+)\)";

            string result = algebra;

            // Check if it's a selection (σ)
            var match = Regex.Match(algebra, selectPattern);
            if (match.Success)
            {
                var condition = match.Groups[1].Value;
                var table = match.Groups[2].Value;
                return $"SELECT * FROM {table} WHERE {condition}";
            }

            // Check if it's a projection (π)
            match = Regex.Match(algebra, projectPattern);
            if (match.Success)
            {
                var attributes = match.Groups[1].Value;
                var table = match.Groups[2].Value;
                return $"SELECT {attributes} FROM {table}";
            }

            // Check if it's a union (∪)
            match = Regex.Match(algebra, unionPattern);
            if (match.Success)
            {
                var table1 = match.Groups[1].Value;
                var table2 = match.Groups[2].Value;
                return $"SELECT * FROM {table1} UNION SELECT * FROM {table2}";
            }

            // Check if it's a set difference (−)
            match = Regex.Match(algebra, differencePattern);
            if (match.Success)
            {
                var table1 = match.Groups[1].Value;
                var table2 = match.Groups[2].Value;
                return $"SELECT * FROM {table1} EXCEPT SELECT * FROM {table2}";
            }

            // Check if it's an intersection (∩)
            match = Regex.Match(algebra, intersectionPattern);
            if (match.Success)
            {
                var table1 = match.Groups[1].Value;
                var table2 = match.Groups[2].Value;
                return $"SELECT * FROM {table1} INTERSECT SELECT * FROM {table2}";
            }

            // Check if it's a cross product (×)
            match = Regex.Match(algebra, crossProductPattern);
            if (match.Success)
            {
                var table1 = match.Groups[1].Value;
                var table2 = match.Groups[2].Value;
                return $"SELECT * FROM {table1}, {table2}";
            }

            // Check if it's a join (⨝)
            match = Regex.Match(algebra, joinPattern);
            if (match.Success)
            {
                var table1 = match.Groups[1].Value;
                var table2 = match.Groups[2].Value;
                return $"SELECT * FROM {table1} INNER JOIN {table2} ON condition"; // Replace 'condition' as needed
            }

            // Check if it's a rename (ρ)
            match = Regex.Match(algebra, renamePattern);
            if (match.Success)
            {
                var newName = match.Groups[1].Value;
                var table = match.Groups[2].Value;
                return $"SELECT * FROM {table} AS {newName}";
            }

            // Check if it's a group by (γ)
            match = Regex.Match(algebra, groupByPattern);
            if (match.Success)
            {
                var columns = match.Groups[1].Value;
                var table = match.Groups[2].Value;
                return $"SELECT {columns}, COUNT(*) FROM {table} GROUP BY {columns}";
            }

            return result;
        }



        public static string ConvertSQLToRelationalAlgebra(string sql)
        {
            // Step 1: Tokenize the SQL query
            string tokenizedSQL = Scanner(sql);

            try
            {
                // Step 2: Parse the tokenized SQL query
                SQLParser parser = new SQLParser(tokenizedSQL);
                ASTNode ast = parser.Parse();  // This gives us the Abstract Syntax Tree (AST)

                // Step 3: Convert the AST to relational algebra
                string relationalAlgebra = ConvertASTToRelationalAlgebra(ast);

                // Step 4: Return the relational algebra expression
                return relationalAlgebra;
            }
            catch (Exception ex)
            {
                // In case of error, return a meaningful error message
                return "Error in SQL Query: " + ex.Message;
            }
        }




        private static string ConvertASTToRelationalAlgebra(ASTNode node)
        {
            if (node == null) return string.Empty;

            switch (node.Type)
            {
                case "Query":
                    // Handle the "Query" node, which combines the SELECT, FROM, WHERE, etc.
                    string selectClause = ConvertASTToRelationalAlgebra(node.Children.Count > 0 ? node.Children[0] : null); // Columns (SELECT)
                    string fromClause = ConvertASTToRelationalAlgebra(node.Children.Count > 1 ? node.Children[1] : null); // Table (FROM)
                    string whereClause = node.Children.Count > 2 ? ConvertASTToRelationalAlgebra(node.Children[2]) : string.Empty; // WHERE
                    string groupByClause = node.Children.Count > 3 ? ConvertASTToRelationalAlgebra(node.Children[3]) : string.Empty; // GROUP BY
                    return $"{selectClause} {fromClause} {whereClause} {groupByClause}".Trim();

                case "Columns":
                    // Handle the columns (projection)
                    return $"π {string.Join(", ", node.Children.Select(c => ConvertASTToRelationalAlgebra(c)))}";

                case "Column":
                    // Handle individual columns (used in SELECT)
                    return ExtractIdentifier(node.Value);  // Return the column name (e.g., "ID", "*")

                case "Table":
                    // Handle the table name (FROM)
                    return "(" + ExtractIdentifier(node.Value) + ")";  // Just return the table name (e.g., "STUD")

                case "Condition":
                    // Handle the condition token (like "age > 5")
                    // Strip the condition[*[ and *] to extract the condition content
                    string condition = ExtractIdentifier(node.Value);
                    //condition = condition.Substring(10, condition.Length - 13);  // Remove "condition[*[" and "*]"
                    return $"σ({condition})";
                //return $"σ({ConvertASTToRelationalAlgebra(node.Children[0])})";

                case "Expression":
                    // Handle expressions (e.g., age > 30, ID = 5)
                    return ExtractIdentifier(node.Value);

                case "Join":
                    // Handle JOIN operations
                    string leftTable = ConvertASTToRelationalAlgebra(node.Children[0]);
                    string rightTable = ConvertASTToRelationalAlgebra(node.Children[1]);
                    string joinCondition = ConvertASTToRelationalAlgebra(node.Children[2]);
                    return $"{leftTable} ⋈ {rightTable} ON {joinCondition}";

                // Handle other node types (like WHERE, SELECT, etc.)
                default:
                    return node.ToString(); // Fallback to the node's string representation (only if necessary)
            }
        }



        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Additional handling when Submit button is clicked, if needed
            MessageBox.Show("Submitted: " + RelationalAlgebraTextBox.Text);
        }
        private void Window_MouseDown(object sender, RoutedEventArgs e)
        {

        }
        // Scanner method to tokenize the SQL query
        public static string Scanner(string query)
        {
            Dictionary<string, string> keywordSymbols = new Dictionary<string, string>()
    {
        { "UNION", "Symbol[*[∪]*]" },
        { "INTERSECT", "Symbol[*[∩]*]" },
        { "MINUS", "Symbol[*[−]*]" },
        { "TIMES", "Symbol[*[×]*]" },
        { "RESTRICT", "Symbol[*[σ]*]" },
        { "PROJECT", "Symbol[*[π]*]" },
        { "JOIN", "Symbol[*[⋈]*]" },
        { "SELECT", "Keyword[*[SELECT]*]" },
        { "FROM", "Keyword[*[FROM]*]" },
        { "WHERE", "Keyword[*[WHERE]*]" },
        { "AND", "Keyword[*[AND]*]" },
        { "OR", "Keyword[*[OR]*]" },
        { "NOT", "Keyword[*[NOT]*]" },
        { "AS", "Keyword[*[AS]*]" },
        { "ON", "Keyword[*[ON]*]" },
        { "GROUP BY", "Keyword[*[GROUP BY]*]" },
        { "HAVING", "Keyword[*[HAVING]*]" },
        { "ORDER", "Keyword[*[ORDER]*]" },
        { "LIMIT", "Keyword[*[LIMIT]*]" },
        { "DISTINCT", "Keyword[*[DISTINCT]*]" },
        { "COUNT", "Keyword[*[COUNT(]*]" }
    };

            List<string> tokens = new List<string>();
            string pattern = @"\b(UNION|INTERSECT|MINUS|TIMES|RESTRICT|PROJECT|JOIN|SELECT|FROM|WHERE|AND|OR|NOT|AS|ON|GROUP BY|HAVING|ORDER|LIMIT|DISTINCT|COUNT)\b" +
                             @"|([A-Za-z_][A-Za-z0-9_\.]*)" +
                             @"|([=<>!]+|\*|,|\(|\))" +
                             @"|(\d+(\.\d+)?)";

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            bool inGroupBy = false;
            bool inNewName = false;
            bool isWhere = false;
            StringBuilder conditionBuilder = new StringBuilder(); // To accumulate the condition

            foreach (Match match in regex.Matches(query))
            {
                string token = match.Value.ToUpper();

                if (keywordSymbols.ContainsKey(token))
                {
                    if (isWhere)
                    {
                        // If we're in a WHERE condition, first add the accumulated condition.
                        if (conditionBuilder.Length > 0)
                        {
                            tokens.Add($"condition[*[{conditionBuilder.ToString().Trim()}]*]");
                            conditionBuilder.Clear(); // Reset the condition accumulator
                        }
                    }
                    tokens.Add(keywordSymbols[token]);

                    // If we hit a keyword after WHERE, stop accumulating the condition
                    if (token == "WHERE")
                    {
                        isWhere = true;
                    }
                    else
                    {
                        isWhere = false;
                    }

                    // If we encounter other keywords like AS, etc., stop collecting as condition
                    if (token == "AS")
                    {
                        inNewName = true;
                    }
                }
                else if (Regex.IsMatch(token, @"^[A-Za-z_][A-Za-z0-9_\.]*$"))
                {
                    if (isWhere)
                    {
                        // Accumulate tokens into the condition while we're in WHERE clause
                        conditionBuilder.Append(token + " ");
                    }
                    else if (inGroupBy)
                    {
                        tokens.Add($"table[*[{token}]*]");
                        inGroupBy = false;
                    }
                    else if (inNewName)
                    {
                        tokens.Add($"newname[*[{token}]*]");
                        inNewName = false;
                    }
                    else
                    {
                        tokens.Add($"Identifier[*[{token}]*]");
                    }
                }
                else if (Regex.IsMatch(token, @"^[=<>!]+|\*|,|\(|\)$"))
                {
                    if (isWhere)
                    {
                        // Add operators to the condition
                        conditionBuilder.Append(token + " ");
                    }
                    else
                    {
                        tokens.Add($"Operator[*[{token}]*]");
                    }
                }
                else if (Regex.IsMatch(token, @"^\d+(\.\d+)?$"))
                {
                    if (isWhere)
                    {
                        // Add numbers to the condition
                        conditionBuilder.Append(token + " ");
                    }
                    else
                    {
                        tokens.Add($"Number[*[{token}]*]");
                    }
                }
            }

            // After the loop, add any remaining condition if we're still in WHERE
            if (isWhere && conditionBuilder.Length > 0)
            {
                tokens.Add($"condition[*[{conditionBuilder.ToString().Trim()}]*]");
            }

            return string.Join(" ", tokens);
        }















        public class SQLParser
        {
            private List<string> tokens;
            private int currentTokenIndex = 0;

            public SQLParser(string query)
            {
                tokens = new List<string>(query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            private string CurrentToken()
            {
                if (currentTokenIndex < tokens.Count)
                    return tokens[currentTokenIndex];
                return null;
            }

            private string NextToken()
            {
                currentTokenIndex++;
                if (currentTokenIndex < tokens.Count)
                    return tokens[currentTokenIndex];
                return null;
            }

            private bool Accept(string expectedToken)
            {
                if (CurrentToken() == expectedToken)
                {
                    NextToken();
                    return true;
                }
                return false;
            }

            public ASTNode Parse()
            {
                ASTNode query = ParseQuery();
                return OptimizeQuery(query);
            }

            private ASTNode ParseQuery()
            {
                if (!Accept("Keyword[*[SELECT]*]"))
                    throw new Exception("Expected SELECT");

                // Parse columns in the SELECT clause
                ASTNode columns = ParseColumns();

                if (!Accept("Keyword[*[FROM]*]"))
                    throw new Exception("Expected FROM");

                // Parse the table(s) in the FROM clause
                ASTNode tables = ParseTables();

                // After parsing the table, check for optional clauses like WHERE, GROUP BY, etc.
                ASTNode whereCondition = null;
                if (Accept("Keyword[*[WHERE]*]"))
                {
                    whereCondition = ParseCondition();
                }

                // Additional optional clauses like GROUP BY, HAVING, etc.
                ASTNode groupByCondition = null;
                if (Accept("Keyword[*[GROUP BY]*]"))
                {
                    groupByCondition = ParseColumns();  // Assuming columns are listed for GROUP BY
                }

                // Return the final parsed query AST
                return new ASTNode("Query", columns, tables, whereCondition, groupByCondition);
            }

            private ASTNode ParseColumns()
            {
                List<ASTNode> columns = new List<ASTNode>();

                // Loop through and collect columns, could be "*" or specific column names
                do
                {
                    columns.Add(ParseColumn());
                } while (Accept("Operator[*[,]*]"));  // Accept commas if multiple columns

                return new ASTNode("Columns", columns.ToArray());
            }

            private ASTNode ParseColumn()
            {
                // Check if it's "*" (all columns) or a specific column
                if (Accept("Operator[*[*]*]"))
                {
                    return new ASTNode("Column", "*");
                }
                else if (CurrentToken() != null && CurrentToken().StartsWith("Identifier[*["))
                {
                    string columnName = CurrentToken();
                    NextToken();
                    return new ASTNode("Column", columnName);
                }

                throw new Exception("Expected Column");
            }

            private ASTNode ParseTables()
            {
                // Expect a table name after FROM
                if (CurrentToken() == null || !CurrentToken().StartsWith("Identifier[*["))
                    throw new Exception("Expected Table after FROM");

                string tableName = CurrentToken();
                NextToken();
                return new ASTNode("Table", tableName);
            }

            private ASTNode ParseCondition()
            {
                // Check if the current token is a "condition" token like condition[*[age > 5]*]
                string conditionToken = CurrentToken();

                if (conditionToken != null && conditionToken.StartsWith("condition[*["))
                {
                    // If it's a condition, consume the token
                    NextToken();  // Consume the condition[*[ token

                    // Now, we need to collect the entire condition (age > 5, for example) until we encounter the closing "*]"
                    string conditionContent = conditionToken.Substring(10, conditionToken.Length - 13); // Strip the "condition[*[" and "*]"

                    // Loop through and collect any additional parts of the condition until we reach the end of the condition
                    while (CurrentToken() != null && !CurrentToken().EndsWith("*]"))
                    {
                        conditionContent += " " + CurrentToken();  // Append the next part of the condition
                        NextToken();  // Move to the next token
                    }

                    // Now we have the complete condition (e.g., "age > 5")
                    if (CurrentToken() != null && CurrentToken().EndsWith("*]"))
                    {
                        NextToken();  // Consume the closing "*]"
                    }

                    // Return the whole condition as a single node in the AST
                    return new ASTNode("Condition", conditionContent);  // Store the full condition as a string
                }

                throw new Exception("Expected a valid condition");
            }


            private ASTNode ParseExpression()
            {
                ASTNode left = ParseTerm();

                while (Accept("Operator[*[+]*]") || Accept("Operator[*[-]*]"))
                {
                    string operatorType = CurrentToken();
                    NextToken();
                    ASTNode right = ParseTerm();
                    left = new ASTNode(operatorType, left, right);
                }

                return left;
            }

            private ASTNode ParseTerm()
            {
                ASTNode left = ParseFactor();

                while (Accept("Operator[*[×]*]") || Accept("Operator[*[/]*]"))
                {
                    string operatorType = CurrentToken();
                    NextToken();
                    ASTNode right = ParseFactor();
                    left = new ASTNode(operatorType, left, right);
                }

                return left;
            }

            private ASTNode ParseFactor()
            {
                if (CurrentToken() != null && CurrentToken().StartsWith("Number[*["))
                {
                    string number = CurrentToken();
                    NextToken();
                    return new ASTNode("Number", number);
                }
                else if (CurrentToken() != null && CurrentToken().StartsWith("Identifier[*["))
                {
                    return ParseColumn();  // Could be a column name in an expression
                }
                else if (Accept("Operator[*[(]*]"))
                {
                    ASTNode expression = ParseExpression();
                    if (!Accept("Operator[*[)]*]"))
                        throw new Exception("Expected closing parenthesis");
                    return expression;
                }

                throw new Exception("Expected Factor");
            }

            // Optimization method to push down selection before projection
            private ASTNode OptimizeQuery(ASTNode node)
            {
                if (node.Type == "Query")
                {
                    // Identify components of the query
                    ASTNode columns = node.Children[0];  // Columns node (SELECT)
                    ASTNode tables = node.Children[1];   // Tables node (FROM)
                    ASTNode where = node.Children.Count > 2 ? node.Children[2] : null; // WHERE clause, optional
                    ASTNode groupBy = node.Children.Count > 3 ? node.Children[3] : null; // GROUP BY clause, optional

                    // Apply predicate pushdown if the WHERE condition exists
                    if (where != null)
                    {
                        // Reorder to place the WHERE condition before the SELECT (projection)
                        return new ASTNode("Query", where, columns, tables, groupBy);
                    }
                    // If no WHERE condition, just return as-is
                    return new ASTNode("Query", columns, tables, where, groupBy);
                }

                // Return the node as-is if no optimization is needed
                return node;
            }
        }

        // AST Node structure
        public class ASTNode
        {
            public string Type { get; }
            public List<ASTNode> Children { get; }
            public string Value { get; }  // Add a separate value field for simple values.

            // Constructor for nodes with children
            public ASTNode(string type, params ASTNode[] children)
            {
                Type = type;
                Children = new List<ASTNode>(children);  // Add children directly
            }

            // Constructor for nodes with a value (no children)
            public ASTNode(string type, string value)
            {
                Type = type;
                Children = new List<ASTNode>();  // No children for this node
                Value = value;  // Store the value directly
            }

            public override string ToString()
            {
                if (Children.Count > 0)
                {
                    return $"{Type}: {string.Join(", ", Children)}";
                }
                else
                {
                    return $"{Type}: {Value}";  // If no children, just return the value.
                }
            }
        }



    }

}
