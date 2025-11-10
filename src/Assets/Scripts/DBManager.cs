using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// This is a singleton that manages the DB interactions for a level.
/// </summary>
public class DBManager {
    private static List<string> COMMANDS = new List<string> { "SELECT", "INSERT", "DELETE", "DROP" };
    private static DBManager managerInstance = null;
    private SQLiteConnection db = null;

    private DBManager() { }

    public static DBManager getInstance() {
        if (managerInstance == null) {
            managerInstance = new DBManager();
        }
        return managerInstance;
    }

    public void initDBLevel(int level) {
        db = new SQLiteConnection(":memory:");
        applyLevelSchema(level);
    }

    public void closeDB() {
        db.Close();
        db = null;
    }
    
    private void applyLevelSchema(int level) {
        switch (level) {
            case 0:
                db.CreateTable<Schemas.Level1.ShapeA>();
                db.Insert(new Schemas.Level1.ShapeA() {Green = 1, Blue = "David"});
                db.Insert(new Schemas.Level1.ShapeA() { Green = 1, Blue = "David" });
                db.Insert(new Schemas.Level1.ShapeA() { Green = 2, Blue = "David" });
                db.Insert(new Schemas.Level1.ShapeA() { Green = 1, Blue = "David" });
                db.Insert(new Schemas.Level1.ShapeA() { Green = 2, Blue = "David" });
                db.Insert(new Schemas.Level1.ShapeA() { Green = 5, Blue = "David" });
                break;
        }
    }

    public List<string> getTableNames() {
        return db.Query<TableName>("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';").Select(x => x.name).ToList();
    }

    public List<string> getColumnNames(string table) {
        return db.GetTableInfo(table).Select(c => c.Name).ToList();
    }

    // Given a table name, return the columns of that table
    public List<Column> getColumns(string table) {
        List<Column> columns = new List<Column>();
        var columnsInfo = db.Query<ColumnInfo>($"PRAGMA table_info({table})");
        var foreignKeys = db.Query<ForeignKey>($"PRAGMA foreign_key_list({table})");


        // Instantiate a fitting Column object depending on its data type in the table
        foreach (var colDefinition in columnsInfo) {
            Column column;
            if (colDefinition.type == "INTEGER") {
                KeyType keyType;
                string foreignTable = null;
                if (colDefinition.pk > 0)
                    keyType = KeyType.PrimaryKey;
                else if (foreignKeys.Any(fk => fk.from == colDefinition.name)) {
                    keyType = KeyType.ForeignKey;
                    foreignTable = foreignKeys.First(fk => fk.from == colDefinition.name).table;
                }
                else {
                    keyType = KeyType.NonKey;
                }

                List<int?> data = db.QueryScalars<int?>($"SELECT {colDefinition.name} FROM {table}").ToList();
                column = new IntColumn { data = data, name = colDefinition.name, tableName = table, keyType = keyType, foreignTable = foreignTable};
            }
            else if (colDefinition.type == "TEXT" || colDefinition.type == "varchar") {
                List<string?> data = db.QueryScalars<string?>($"SELECT {colDefinition.name} FROM {table}").ToList();
                column = new TextColumn { data = data, name = colDefinition.name, tableName = table};
            }
            else {
                throw new Exception($"The column type \"{colDefinition.type}\" does not exist!");
            }

            columns.Add(column);
        }
        return columns;
    }

    public int getRowCount(string table) {
        return db.ExecuteScalar<int>("SELECT COUNT(*) FROM " + table);
    }

    public List<string> getTableStructure(string table) {
        List<string> columnStrings = new List<string>();
        var columns = getColumns(table);
        foreach ( var column in columns ) {
            bool nullable = db.ExecuteScalar<int>($"SELECT [notnull] FROM pragma_table_info(\'{table}\') WHERE name = \'{column.name}\'") == 1;
            string keyText = "";
            if (column.GetType() == typeof(IntColumn)) {
                switch (((IntColumn)column).keyType) {
                    case KeyType.PrimaryKey:
                        keyText = "Primary Key, Auto Increment, ";
                        break;
                    case KeyType.ForeignKey:
                        keyText = $"Foreign Key reference to {((IntColumn)column).foreignTable}, ";
                        break;
                }
            }
            string extras = keyText + (nullable ? "Not Null" : "");
            string extraSeparator = extras != "" ? " - " : "";
            string columnDef = $"{column.name}: " + column.getType() + extraSeparator + extras + "\n";
            columnStrings.Add(columnDef);
        }
        return columnStrings;
    }

    public static string addTablePrefixToColumn(string columnName, string tableName) {
        if (columnName.StartsWith(tableName)) {
            return columnName;
        } else if (columnName.Contains(".")) {
            throw new ArgumentException("Column already is shown to belong to a table, a different table.");
        }
        return $"{tableName}.{columnName}";
    }

    public class ValidationResult {
        public bool isValid;
        public string? errorMessage;

        private ValidationResult(bool isValid, string errorMessage) {
            this.isValid = isValid;
            this.errorMessage = errorMessage;
        }

        public static ValidationResult fail(string errorMessage) {
            return new ValidationResult(false, errorMessage);
        }

        public static ValidationResult pass() {
            return new ValidationResult(true, null);
        }
    }
    
    public static Tuple<List<string>, string> getQuerySelections(List<string> qWords) {
        int fromIndex = qWords.FindIndex(w => string.Equals(w, "FROM", StringComparison.OrdinalIgnoreCase));
        
        List<string> selected = new List<string>();
        foreach (var word in qWords.GetRange(1, fromIndex - 1))
            selected.Add(word.Replace("," , ""));
        string tableSelected = qWords[fromIndex + 1];

        selected.RemoveAll(c => c == ",");

        return new Tuple<List<string>, string>(selected, tableSelected);
    }

    public static List<string> getQueryWords(string query) {
        var queryWords = query.Trim().TrimEnd(';').Replace("\n", " ").Split(" ").ToList();
        queryWords.RemoveAll(w => w == "");
        return queryWords;
    }

    public static Tuple<List<string>, string> getQuerySelections(string query) {
        List<string> queryWords = getQueryWords(query);
        queryWords.RemoveAll(w => w.Equals("DISTINCT", StringComparison.OrdinalIgnoreCase));
        return getQuerySelections(queryWords);
    }

    public static int? getLimit(string query) {
        List<string> queryWords = getQueryWords(query);
        int limitIndex = queryWords.FindIndex(w => w.Equals("LIMIT", StringComparison.OrdinalIgnoreCase));
        return limitIndex == -1 ? null : int.Parse(queryWords[limitIndex+1]);
    }

    // Case insensitive string comparison (much shorter to write)
    public static bool like(string str1, string str2) {
        return string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);
    }
    // Case insensitive StartsWith()
    public static bool startsLike(string str, string prefix) {
        return str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    // This vets valid queries that are not valid in the game to ensure valid animation rendering
    public ValidationResult isValidQuery(string query) {
        // Case insensitive contains for list of strings
        bool containsLike(List<string> list, string str) {
            return list.Any(w => w.Equals(str, StringComparison.OrdinalIgnoreCase)); ;
        }

        // Find index of string regardless of case
        int findIndexLike(List<string> list, string str) {
            return list.FindIndex(elem => like(elem,str));
        }

        // Only one SQL statement
        query = query.Trim().TrimEnd(';').Replace("\n", " ");
        if (query.Contains(';'))
            return ValidationResult.fail("Only one SQL statement is allowed to solve this puzzle!");

        List<string> queryWords = query.Split(' ').ToList();
        queryWords.RemoveAll(w => w == "");

        // At least two words (Before limit and distinct are removed)
        if (queryWords.Count < 2)
            return ValidationResult.fail("Query must have at least two words to be valid!");
        
        // Get rid of DISTINCT and LIMIT clauses
        queryWords.RemoveAll(w => like(w,"DISTINCT"));
        if (like(queryWords[^2],"LIMIT") && int.TryParse(queryWords[^1], out _))
            queryWords.RemoveRange(queryWords.Count-2, 2);

        // At least two words (After limit and distinct are removed)
        if (queryWords.Count < 2)
            return ValidationResult.fail("Query must have at least two words other than DISTINCT and LIMIT to be valid!");

        // First word is command
        if (!containsLike(COMMANDS,queryWords[0]))
            return ValidationResult.fail("Query must start with the command keyword (such as SELECT)!");

        // Only one command word
        if (queryWords.Count(word => containsLike(COMMANDS,word)) >= 2)
            return ValidationResult.fail("No subqueries are allowed to solve this puzzle!");

        // Contains any AS casts
        if (containsLike(queryWords, "AS"))
            return ValidationResult.fail("You are not allowed to rename the result columns for this puzzle!");

        // Only columns are selected in SELECT, and one table's cols are selected without JOIN (no cartesian product)
        if (like(queryWords[0],"SELECT") && containsLike(queryWords,"FROM")) {
            List<string> selected = new List<string>();
            List<string> tables = getTableNames();
            int fromIndex = findIndexLike(queryWords,"FROM");
            int whereIndex = findIndexLike(queryWords,"WHERE");
            int joinCount = queryWords.Count(word => like(word, "JOIN"));

            // Get columns and tables selected
            for (int i = 1; i < fromIndex; i++) {
                var word = queryWords[i];
                if (i != fromIndex-1 && !word.Contains(",") && queryWords[i+1] != ",")
                    return ValidationResult.fail("You are not allowed to rename the result columns for this puzzle! If you tried to select multiple columns, use commas.");
                selected.Add(word.Replace("," , ""));
            }
            List<string> tablesSelected = queryWords.GetRange(fromIndex + 1, queryWords.Count - fromIndex - 1).TakeWhile(word => tables.Contains(word)).ToList();

            selected.RemoveAll(c => c == ",");

            // No tables
            if (tablesSelected.Count <= 0) {
                return ValidationResult.fail("No shapes selected!");
            }

            // Too many tables
            if (tablesSelected.Count > 1) {
                return ValidationResult.fail("You are not allowed to select more than one shapes in this puzzle without a merge!");
            }

            // Has joins
            if (containsLike(queryWords, "JOIN")) {
                List<List<string>> colLists = new List<List<string>>();
                foreach (string table in tables)
                    colLists.Add(getColumnNames(table));

                bool hasNonColSelects = false;
                for (int i = 0; i < tables.Count; i++) {
                    List<string> colList = colLists[i];
                    hasNonColSelects |= selected.Any(word => !colList.Contains(word) && !colList.Contains(word.Replace($"{tables[i]}.", "")) && word != "*");
                }
                if (hasNonColSelects)
                    return ValidationResult.fail("You may only select columns to solve this puzzle!");
            } else {  // Does not have joins
                // Non column selections
                string tableSelected = tablesSelected[0];
                List<string> colNames = getColumnNames(tableSelected);
                if (selected.Any(word => !colNames.Contains(word) && !colNames.Contains(word.Replace($"{tableSelected}.", "")) && word != "*")) {
                    return ValidationResult.fail("Non-column / foreign column selections are not allowed in this puzzle! If selecting from other shapes, ensure joins are used.");
                }
            }
            
        }

        
        if (like(queryWords[0], "INSERT")) {
            // Ensure INSERT only inserts INTO, not OR
            if (!like(queryWords[1], "INTO"))
                return ValidationResult.fail("You are only allowed to INSERT with INTO for this puzzle!");
            // Ensure that all INSERTed values are deliberate, no DEFAULTs
            if (queryWords.Any(word => like(word, "DEFAULT")))
                return ValidationResult.fail("You are not allowed to INSERT with DEFAULT for this puzzle!");
            // Ensure no conflict handling is being done, as that is outside of scope
            if (queryWords.Any(word => like(word, "DO")))
                return ValidationResult.fail("You are not allowed to INSERT with ON for this puzzle!");
            // Prevent RETURNING usage
            if (queryWords.Any(word => like(word, "RETURNING")))
                return ValidationResult.fail("You are not allowed to INSERT with RETURNING for this puzzle!");
        }
        
        return ValidationResult.pass();
    }

    // Returns primary keys to show as a result of the query, or the primary keys of the rows affected
    public List<int> runQuery(string query) {
        List<string> queryWords = getQueryWords(query);

        // Replace SELECT columns with primary key
        if (startsLike(query, "SELECT")) {
            int fromIndex = queryWords.FindIndex(word => like(word, "FROM"));
            string table = queryWords[fromIndex + 1];
            Column pkCol = getColumns(table)[0];  // NOTE: This relies on the primary key being the first column
            if (pkCol.getType() != "Int" || ((IntColumn)pkCol).keyType != KeyType.PrimaryKey)
                throw new ArgumentException("The table does not have a primary key column!");

            queryWords.RemoveRange(1, fromIndex - 1);
            queryWords.Insert(1, pkCol.name);
            queryWords.RemoveAll(w => w.Equals("DISTINCT", StringComparison.OrdinalIgnoreCase));
            if (like(queryWords[^2], "LIMIT") && int.TryParse(queryWords[^1], out _))
                queryWords.RemoveRange(queryWords.Count - 2, 2);
            query = string.Join(" ", queryWords);
            return db.QueryScalars<int>(query);
        } else if (startsLike(query, "DELETE")) {
            string table = queryWords[2];
            Column pkCol = getColumns(table)[0];
            string rowsDeletedSelect = query.Replace("DELETE", $"SELECT {pkCol.name}");
            return db.QueryScalars<int>($"SELECT {pkCol.name} FROM {table} EXCEPT SELECT * FROM ({rowsDeletedSelect})");
        } else if (startsLike(query, "INSERT")) {
            string table = queryWords[2];
            Column pkCol = getColumns(table)[0];
            db.Execute(query);
            return db.QueryScalars<int>($"SELECT {pkCol.name} FROM {table}");
        } else if (startsLike(query, "DROP")) {
            // TODO: implement
            return null;
        }

        return null;
    }

    public int getCountOfRowsAffected() {
        return db.ExecuteScalar<int>("SELECT changes()");
    }

    public void undoInsertSideEffects(string tableName, string pkCol, int countOfInserts) {
        db.Execute($"DELETE FROM {tableName} WHERE {pkCol} IN (SELECT {pkCol} FROM {tableName} ORDER BY {pkCol} DESC LIMIT {countOfInserts})");
        db.Execute($"UPDATE sqlite_sequence SET seq = seq - {countOfInserts} WHERE name='{tableName}'");
    }
}

public enum QueryType {
    SELECT,
    DELETE,
    INSERT,
    DROP
};