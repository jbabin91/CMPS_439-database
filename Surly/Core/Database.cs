using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Surly.Core.Structure;

namespace Surly.Core {
    public class Database {
        public LinkedList<Tuple<string, Relation>> Tables { get; set; }

        public Database() {
            Tables = new LinkedList<Tuple<string, Relation>>();
            LoadDatabase(@"Database.txt");
        }

        public void CreateTable(string tableName, string[] attributeName, string[] attributeType,
            int[] attributeLengths, bool[] nullable) {
            var columns = new LinkedList<ColumnAttributes>();

            for (var i = 0; i < attributeType.Length; i++)
                columns.AddLast(new ColumnAttributes(
                    attributeName[i],
                    attributeType[i],
                    attributeLengths[i],
                    nullable[i]
                ));

            var table = new Relation(columns.ToList());
            Tables.AddLast(Tuple.Create(tableName, table));
        }

        public void Insert(string tableName, object[] item) {
            // Find Table and set it as table variable
            var table = Tables.FirstOrDefault(x => x.Item1.ToLower().Equals(tableName.ToLower()));

            if (table != null) {
                var tempTable = table.Item2.Columns.ToArray();
                var temp = new Row(tempTable.Length);

                if (tempTable.Length != item.Length) {
                    Console.WriteLine("Number of columns is incorrect");
                    return;
                }

                for (var i = 0; i < item.Length; i++) {
                    // Make sure that the table is not null if it should not be.
                    if (!tempTable[i].Nullable && (item[i] == null || item[i].ToString() == "")) {
                        Console.WriteLine("Make sure that the table is not null if it was not suppose to be.");
                        return;
                    }
                    if (!(item[i] == null || item[i].ToString() == "")) {
                        // Check to make sure that the item is the correct length.
                        if (item[i].ToString().Length > tempTable[i].Length) {
                            Console.WriteLine("Make sure the data is the correct length.");
                            return;
                        }
                        // Check to make sure that the item is the correct type
                        if (CheckType(item[i].GetType().ToString(), tempTable[i].Type)) {
                            Console.WriteLine("Typing for the data is incorrect.");
                            return;
                        }
                        temp.Cells[i] = item[i];
                    }
                }
                table.Item2.Rows.Add(temp);
            }
        }

        public Tuple<string, Relation> Print(string tableName) {
            var table = Tables.FirstOrDefault(x => x.Item1.ToLower().Equals(tableName.ToLower()));
            return table;
        }

        public void Destroy(string tableName) {
            // Find Table and set it as destroyed
            var destroyed = Tables.FirstOrDefault(x => x.Item1.ToLower().Equals(tableName.ToLower().Trim()));
            if (destroyed != null) Tables.Remove(destroyed);
        }

        public void Delete(string tableName) {
            // Find the table
            var deleted = Tables.FirstOrDefault(x => x.Item1.Equals(tableName));
            // Remove everything in the table.
            deleted?.Item2.Rows.RemoveAll(x => true);
        }

        public void Delete(string tableName, string whereColumn, string whereValue) {
            // Find the table
            var deleted = Tables.FirstOrDefault(x => x.Item1.ToLower().Equals(tableName.ToLower()));

            if (deleted != null) {
                var tempColumnIndexes = 0;

                foreach (var column in deleted.Item2.Columns)
                    if (column.Name.ToLower().Equals(whereColumn.ToLower()))
                        tempColumnIndexes = deleted.Item2.Columns.ToList().IndexOf(column);
                // Remove the specific item from the table
                deleted.Item2.Rows.RemoveAll(
                    x => x.Cells[tempColumnIndexes].ToString().ToLower().Equals(whereValue.ToLower())
                );
            }
        }

        public Tuple<string, Relation> Project(string tableName, List<string> column) {
            // T1 = PROJECT CREDITS, CNUM FROM COURSE;
            var tempColumns = new List<ColumnAttributes>();
            var tempColumnIndex = 0;
            var table = Tables.FirstOrDefault(
                x => x.Item1.ToLower().Equals(tableName.ToLower())
            );
            var columnName = column.Select(s => s.Trim().ToUpper()).ToArray();
            var columnsz = new List<string>();
            foreach (var item in table.Item2.Columns) {
                columnsz.Add(item.Name.ToUpper());
            }

            for (var i = 0; i < column.Count; i++) {
                var columns = table.Item2.Columns[i];
                //var columns = columnsz[i];
                var columnz = columnName[i];
                //var columnName = column[i];
                Debug.WriteLine("Columns: " + columns);
                Debug.WriteLine("Type: " + columns.GetType());
                Debug.WriteLine("Columnz: " + columnz);
                Debug.WriteLine("Type: " + columnz.GetType());
                Debug.WriteLine(string.Equals(columns.Name.ToUpper(), columnz, StringComparison.Ordinal));

                //if (columns.Name.ToUpper().Equals(column[tempColumnIndex].ToUpper()))
                if (string.Equals(columns.Name.ToUpper(), columnz, StringComparison.Ordinal))
                    tempColumns.Add(columns);
            }

            //foreach (var columns in table.Item2.Columns)
            //{
            //    if (columns.Name.ToUpper().Equals(column[tempColumnIndex]))
            //        tempColumns.Add(columns);
            //    tempColumnIndex++;
            //}

            var tempRelation = new Relation(tempColumns);

            //var selectedRelation = table.Item2.Rows.Where(
            //    x => x.Cells[tempColumnIndex].ToString().ToLower().Equals(value.ToLower())
            //);

            foreach (var row in table.Item2.Rows) tempRelation.Rows.Add(row);
            return new Tuple<string, Relation>(table.Item1, tempRelation);
        }

        public Tuple<string, Relation> Select(string tableName, List<string> conditions) {
            var tempColumns = new List<ColumnAttributes>();
            var conditionValue = new List<string>();
            var tempColumnIndex = new List<int>();
            var table = Tables.FirstOrDefault(
                x => x.Item1.ToLower().Equals(tableName.ToLower())
            );
           
            foreach (var columns in table.Item2.Columns) {
                tempColumns.Add(columns);
                foreach (var condition in conditions) {
                    if (condition.Contains("||")) {
                        var orCondition = condition.Split(new [] { "||", "=" }, StringSplitOptions.None);
                        if (columns.Name.ToUpper().Equals(orCondition[0].Trim().ToUpper())) {
                            tempColumnIndex.Add(table.Item2.Columns.IndexOf(columns));
                        }
                    }
                    else { 
                        var columnName = Regex.Split(condition, "=", RegexOptions.None);
                        Debug.WriteLine("Columns Name Length: " + columns.Name.Length);
                        Debug.WriteLine("Condition Name Length: " + columnName[0].Trim().Length);
                        Debug.WriteLine("Testing select: " + columns.Name.ToUpper().Equals(columnName[0].Trim().ToUpper()));
                        if (columns.Name.ToUpper().Equals(columnName[0].Trim().ToUpper()))
                        tempColumnIndex.Add(table.Item2.Columns.IndexOf(columns));
                    }
                }
            }
            var tempRelation = new Relation(tempColumns);

            foreach (var value in conditionValue) {
                foreach (var columnIndex in tempColumnIndex) {
                    var selectedRelation = table.Item2.Rows.Where(
                        x => x.Cells[columnIndex].ToString().ToLower().Equals(value.ToLower())
                    );
                    foreach (var row in selectedRelation) tempRelation.Rows.Add(row);
                }
            }
            
            return new Tuple<string, Relation>(table.Item1, tempRelation);
        }

        public Relation Join(string table1, string table2, string column1, string column2) {
            var tempColumns = new LinkedList<ColumnAttributes>();
            var tempColumnIndex1 = 0;
            var tempColumnIndex2 = 0;

            var tempTable1 = Tables.FirstOrDefault(x => x.Item1.Equals(table1));
            foreach (var column in tempTable1.Item2.Columns.ToList()) {
                tempColumns.AddLast(column);

                if (column.Name.Equals(column1)) tempColumnIndex1 = tempTable1.Item2.Columns.ToList().IndexOf(column);
            }

            var tempTable2 = Tables.FirstOrDefault(x => x.Item1.Equals(table2));
            foreach (var column in tempTable2.Item2.Columns) {
                tempColumns.AddLast(column);
                if (column.Name.Equals(column2)) tempColumnIndex2 = tempTable2.Item2.Columns.ToList().IndexOf(column);
            }

            var tempRelation = new Relation(tempColumns.ToList());
            foreach (var row in tempTable1.Item2.Rows) {
                var tempRowsFromTable = new LinkedList<Row>();

                for (var i = 0; i < tempTable2.Item2.Rows.Count; i++)
                    if (row.Cells[tempColumnIndex1].ToString().Equals(tempTable2.Item2.Rows[i].Cells[tempColumnIndex2].ToString()))
                        tempRowsFromTable.AddLast(tempTable2.Item2.Rows[i]);

                foreach (var table in tempRowsFromTable) {
                    var tempRow = new Row(tempRelation.Columns.Count);

                    for (var i = 0; i < row.Cells.Length; i++) tempRow.Cells[i] = row.Cells[i];

                    var j = row.Cells.Length;

                    for (var i = 0; i < table.Cells.Length; i++, j++) tempRow.Cells[j] = table.Cells[i];
                    tempRelation.Rows.Add(tempRow);
                }
            }
            return tempRelation;
        }

        public void Update(string tableName, string[] columnUpdate, string[] valueUpdate) {
            var table = Tables.FirstOrDefault(x => x.Item1.ToLower().Equals(tableName.ToLower()));
            var tempUpdateIndexes = new List<int>();

            foreach (var column in table.Item2.Columns) // Loop through to get the index of each of the columns to update
            foreach (var update in columnUpdate)
                if (column.Name.ToLower().Equals(update.ToLower()))
                    tempUpdateIndexes.Add(table.Item2.Columns.IndexOf(column));

            foreach (var row in table.Item2.Rows) // Put the values into the specified columns
            foreach (var item in tempUpdateIndexes) row.Cells[item] = valueUpdate[tempUpdateIndexes.IndexOf(item)];
        }

        public void Update(string tableName, string[] columnUpdate, string[] valueUpdate, string whereColumn, string whereValue) {
            var table = Tables.FirstOrDefault(
                x => x.Item1.ToLower().Equals(tableName.ToLower())
            );
            var tempColumnIndexes = 0;
            var tempUpdateIndexes = new List<int>();

            foreach (var column in table.Item2.Columns) {
                // Get the index of the where column
                if (column.Name.ToLower().Equals(whereColumn.ToLower()))
                    tempColumnIndexes = table.Item2.Columns.IndexOf(column);
                // This get the index of the update column
                foreach (var update in columnUpdate)
                    if (column.Name.ToLower().Equals(update.ToLower()))
                        tempUpdateIndexes.Add(table.Item2.Columns.IndexOf(column));
            }

            var whereFields = table.Item2.Rows.Where(
                x => x.Cells[tempColumnIndexes].ToString().ToLower().Equals(whereValue.ToString().ToLower())
            );

            foreach (var row in whereFields)
            foreach (var item in tempUpdateIndexes)
                row.Cells[item] = valueUpdate[tempUpdateIndexes.IndexOf(item)];
        }

        private static bool CheckType(string typeOfInput, string typeOfNeeded) {
            if (typeOfNeeded.ToLower().Equals("string")) return false;
            switch (typeOfInput) {
                case "System.Int32":
                    return !typeOfNeeded.ToLower().Equals("int") |
                           typeOfNeeded.ToLower().Equals("integer");
                case "System.String":
                    return !typeOfNeeded.ToLower().Equals("string");
                case "System.Boolean":
                    return !(typeOfNeeded.ToLower().Equals("bool") |
                             typeOfNeeded.ToLower().Equals("boolean"));
                default:
                    return false;
            }
        }

        private static object TryCast(string input) {
            try {
                return Convert.ToInt32(input);
            }
            catch {
                try {
                    return Convert.ToBoolean(input);
                }
                catch {
                    return input;
                }
            }
        }

        private void LoadDatabase(string fileName) {
            var uri = AppContext.BaseDirectory + "\\" + fileName;
            using (var fileStream = new FileStream(uri, FileMode.OpenOrCreate)) {
                using (var sw = new StreamReader(fileStream)) {
                    string line;
                    var tableName = "";
                    var columnNames = new List<string>();
                    var columnAttributes = new List<string>();
                    var columnLength = new List<int>();
                    var nullable = new List<bool>();
                    var newTable = false;

                    while ((line = sw.ReadLine()) != null)
                        switch (line[0]) {
                            case 'τ': {
                                var split = line.Split(new[] { "τ" }, StringSplitOptions.RemoveEmptyEntries);
                                tableName = split[0];
                                columnNames = new List<string>();
                                columnAttributes = new List<string>();
                                columnLength = new List<int>();
                                nullable = new List<bool>();
                                newTable = true;
                            }
                                break;
                            case 'α': {
                                var split = line.Split(new[] { "α", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                                columnNames.AddRange(split);
                            }
                                break;
                            case 'π': {
                                var split = line.Split(new[] { "π", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                                columnAttributes.AddRange(split);
                            }
                                break;
                            case 'Δ': {
                                var split = line.Split(new[] { "Δ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                                columnLength.AddRange(split.Select(length => Convert.ToInt32(length)));
                            }
                                break;
                            case 'λ': {
                                var split = line.Split(new[] { "λ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                                nullable.AddRange(split.Select(Convert.ToBoolean));
                            }
                                break;
                            case 'Θ': {
                                if (newTable) {
                                    CreateTable(tableName, columnNames.ToArray(), columnAttributes.ToArray(), columnLength.ToArray(), nullable.ToArray());
                                    newTable = false;
                                }
                                var split = line.Split(new[] { "Θ", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                                var send = new object[split.Length];
                                for (var i = 0; i < split.Length; i++) send[i] = TryCast(split[i]);
                                Insert(tableName, send);
                            }
                                break;
                        }
                }
            }
        }

        private async Task SaveDatabase(string filePath) {
            var uri = AppContext.BaseDirectory + "\\" + filePath;
            using (var fileStream = new FileStream(uri, FileMode.OpenOrCreate)) {
                using (var file = new StreamWriter(fileStream)) {
                    foreach (var table in Tables) {
                        file.WriteLine("τ" + table.Item1);
                        file.Write("α");

                        foreach (var item in table.Item2.Columns) file.Write(item.Name + "\t");

                        file.WriteLine("");
                        file.Write("π");

                        foreach (var data in table.Item2.Columns) file.Write(data.Type + "\t");

                        file.WriteLine("");
                        file.Write("Δ");

                        foreach (var data in table.Item2.Columns) file.Write(data.Length + "\t");

                        file.WriteLine("");
                        file.Write("λ");

                        foreach (var data in table.Item2.Columns) file.Write(data.Nullable + "\t");

                        file.WriteLine("");

                        if (table.Item2.Rows.Count > 0)
                            foreach (var row in table.Item2.Rows) {
                                for (var i = 0; i < table.Item2.Columns.Count; i++)
                                    file.Write("Θ" + row.Cells[i] + "\t");
                                file.WriteLine("");
                            }
                        else file.WriteLine("Θ");
                    }
                }
            }
        }

        #region parse

        private void parseCreate(string statement) {
            //var split = Regex.Split(statement, "table", RegexOptions.IgnoreCase);
            statement = statement.Remove(0, 6).Trim();
            var tableData = statement.Split(new[] { "(", ")" }, StringSplitOptions.RemoveEmptyEntries);
            var tableName = tableData[0].Trim();

            if (tableData.Length > 1) {
                var createAttributes = Regex.Split(tableData[1], ",", RegexOptions.IgnoreCase);

                // All the components for ColumnAttributes
                var attributeName = new List<string>();
                var attributeType = new List<string>();
                var attributeLength = new List<int>();
                var nullable = new List<bool>();

                foreach (var column in createAttributes)
                    try {
                        var columnSplit = column.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        int length;
                        bool isNull;
                        attributeName.Add(columnSplit[0]);
                        attributeType.Add(columnSplit[1]);

                        try {
                            length = Convert.ToInt32(columnSplit[2]);
                        }
                        catch {
                            throw new Exception("Could not convert the length field to number");
                        }

                        try {
                            isNull = Convert.ToBoolean(columnSplit[3]);
                        }
                        catch {
                            throw new Exception("Could not convert the null field to boolean");
                        }

                        attributeLength.Add(length);
                        nullable.Add(isNull);
                    }
                    catch {
                        throw new Exception("Could not parse create input");
                    }
                CreateTable(tableName, attributeName.ToArray(), attributeType.ToArray(), attributeLength.ToArray(), nullable.ToArray());
            }
        }

        private void parseInsert(string statement) {
            statement = statement.Trim().Remove(0, 6);
            var split = Regex.Split(statement, "values", RegexOptions.IgnoreCase);
            var tableName = split[0].Trim();

            if (split.Length > 1) {
                var values = split[1].Replace('(', ' ').Replace(')', ' ').Split(',');
                var tempObjects = new List<object>();

                foreach (var item in values)
                    if (string.IsNullOrWhiteSpace(item)) {
                        tempObjects.Add(null);
                    }
                    else {
                        var addedItem = item.Trim();
                        tempObjects.Add(TryCast(addedItem));
                    }
                Insert(tableName, tempObjects.ToArray());
            }
        }

        private Tuple<string, Relation> parsePrint(string statement) {
            var split = statement.ToLower().Split(new[] { " " }, StringSplitOptions.None);

            if (split.Length > 1) return Print(split[1].Trim());
            return null;
        }

        private void parseUpdate(string statement) {
            var split = statement.Trim().Remove(0, 6);
            var setWhereSplit = Regex.Split(split, "set", RegexOptions.IgnoreCase);

            if (setWhereSplit.Length > 1) {
                var tableName = setWhereSplit[0].Trim();
                var setSplit = Regex.Split(setWhereSplit[1], "where", RegexOptions.IgnoreCase);
                var valuesColumns = setSplit[0].Split(',');
                var updateColumns = new List<string>();
                var updateValues = new List<string>();

                foreach (var valueSplit in valuesColumns.Select(update => update.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries))) {
                    updateColumns.Add(valueSplit[0].Trim());
                    updateValues.Add(valueSplit[1].Trim());
                }

                if (setSplit.Length > 1) {
                    var whereSplit = setSplit[1].Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    Update(tableName, updateColumns.ToArray(), updateValues.ToArray(), whereSplit[0].Trim(), whereSplit[1].Trim());
                }
                else {
                    Update(tableName, updateColumns.ToArray(), updateValues.ToArray());
                }
            }
        }

        private void parseDestroy(string statement) {
            var split = statement.ToLower().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length > 1) Destroy(split[1].Trim());
        }

        private void parseDelete(string statement) {
            var split = statement.ToLower().Split(new[] { "from" }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length > 1) {
                var whereSplit = split[1].ToLower().Split(new[] { "where" }, StringSplitOptions.RemoveEmptyEntries);

                if (whereSplit.Length > 1) {
                    var whereValue = whereSplit[1].Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    Delete(whereSplit[0].Trim(), whereValue[0].Trim(), whereValue[1].Trim());
                }
                else {
                    Delete(split[1].Trim());
                }
            }
        }

        private Tuple<string, Relation> parseProject(string statement) {
            // T1 = PROJECT CREDITS, CNUM FROM COURSE;
            statement = statement.Trim().Remove(0, 7);
            var split = Regex.Split(statement, "from", RegexOptions.IgnoreCase);
            var tableName = split[1].Trim();

            if (split.Length < 1) return null;
            var tableColumns = split[0].ToLower().Split(new[] { "," }, StringSplitOptions.None);
            var columnList = new List<string>();
            foreach (var column in tableColumns) {
                columnList.Add(column);
            }
            if (tableColumns.Length > 1) return Project(tableName, columnList);
            return null;
        }

        private Tuple<string, Relation> parseSelect(string statement) {
            // Select tableName where column = value
            statement = statement.Trim().Remove(0, 6);
            var split = Regex.Split(statement, "where", RegexOptions.IgnoreCase);
            var tableName = split[0].Trim();

            if (split.Length < 1) return null;
     
            var conditions = Regex.Split(split[1].ToLower(), "and", RegexOptions.IgnoreCase);
            var whereClauses = new List<string>();

            foreach (var condition in conditions) {
                whereClauses.Add(condition);
            }

            if (whereClauses.Count > 1) return Select(tableName, whereClauses);
            return null;
        }

        private Tuple<string, Relation> parseJoin(string statement) {
            statement = statement.Trim().Remove(0, 4);
            var tableNames = Regex.Split(statement, ",", RegexOptions.IgnoreCase);

            if (tableNames.Length <= 1) return null;
            var whereSplit = Regex.Split(tableNames[1], "on", RegexOptions.IgnoreCase);

            if (tableNames.Length <= 1) return null;
            var valueSplit = Regex.Split(whereSplit[1], "=");

            if (valueSplit.Length > 1)
                return new Tuple<string, Relation>(tableNames[0].Trim() + whereSplit[0].Trim(),
                    Join(tableNames[0].Trim(), whereSplit[0].Trim(), valueSplit[0].Trim(),
                        valueSplit[1].Trim()));
            return null;
        }

        public List<Tuple<string, Relation>> Parse(string allStatments) {
            // Split at ';' for each command 
            var statements = allStatments.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            var relationList = new List<Tuple<string, Relation>>();

            // Parse through the commands and call the corresponding command
            foreach (var statement in statements) {
                var command = statement.Substring(0, 10).ToLower();
                if (command.Contains("create")) {
                    parseCreate(statement.Trim());
                }
                else if (command.Contains("insert")) {
                    parseInsert(statement.Trim());
                }
                else if (command.Contains("print")) {
                    parsePrint(statement.Trim());
                }
                else if (command.Contains("update")) {
                    parseUpdate(statement.Trim());
                }
                else if (command.Contains("delete")) {
                    parseDelete(statement.Trim());
                }
                else if (command.Contains("destroy")) {
                    parseDestroy(statement.Trim());
                }
                else if (command.Contains("project")) {
                    var temp = parseProject(statement.Trim());

                    if (temp != null) relationList.Add(temp);
                }
                else if (command.Contains("select")) {
                    var temp = parseSelect(statement.Trim());

                    if (temp != null) relationList.Add(temp);
                }
                else if (command.Contains("join")) {
                    var temp = parseJoin(statement.Trim());

                    if (temp != null) relationList.Add(temp);
                }
            }
            var task = SaveDatabase(@"Database.txt");
            task.Wait();
            return relationList;
        }

        #endregion
    }
}
