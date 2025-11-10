using SQLite;
using System.Collections.Generic;
using UnityEngine;

public class TableName {
    public string name { get; set; }
}

public class ColumnInfo {
    public string name { get; set; }
    public string type { get; set; }
    public int pk { get; set; }
}

public class ForeignKey {
    public string from { get; set; }
    public string table { get; set; }
}

public enum KeyType {
    NonKey = 0,
    PrimaryKey,
    ForeignKey,
}

public interface Column {
    string name { get; set; }
    string tableName { get; set; }

    public string getCellText(int row);
    public string getType();
    public void removeAt(int index);
    public void setOrderTo(List<int> indices);
}

public class IntColumn : Column {
    public KeyType keyType = KeyType.NonKey;
    public string? foreignTable;
    public List<int?> data;

    public string name { get; set; }
    public string tableName { get; set; }

    public string getCellText(int row) {
        if (data[row] == null)
            return "";
        return data[row].ToString();
    }

    public string getType() {
        return "Int";
    }

    public void removeAt(int index) {
        data.RemoveAt(index);
    }

    public void setOrderTo(List<int> indices) {
        List<int?> newData = new List<int?>();
        foreach (int i in indices) {
            newData.Add(data[i]);
        }
        data = newData;
    }
}

public class TextColumn : Column {
    public List<string?> data;

    public string name { get; set; }
    public string tableName { get; set; }

    public string getCellText(int row) {
        if (data[row] == null)
            return "";
        return data[row];
    }

    public string getType() {
        return "Text";
    }

    public void removeAt(int index) {
        data.RemoveAt(index);
    }

    public void setOrderTo(List<int> indices) {
        List<string?> newData = new List<string?>();
        foreach (int i in indices) {
            newData.Add(data[i]);
        }
        data = newData;
    }
}

public class LevelContext {
    public Vector3[] positions;
    public string levelGoalText;
    public List<int> rowsAffectedPks;
    public List<string> columnsAffected;
    public bool mustBeDistinct;
}

public static class LCDict {
    // One tuple for each level that contains the context that every level has
    public static Dictionary<int, LevelContext> dict = new Dictionary<int, LevelContext> {
        [0] = new LevelContext { positions = new Vector3[] { new Vector3(-3f, 2.5f, 0)}, levelGoalText = "Show all rectangles of Shape A", rowsAffectedPks = new List<int>{1,2,3,4,5}, columnsAffected = new List<string>{"ShapeA.Red", "ShapeA.Green", "ShapeA.Blue"} },
    };
}

namespace Schemas {
    namespace Level1 {
        public class ShapeA {
            [PrimaryKey, AutoIncrement]
            public int? Red { get; set; }
            public int? Green { get; set; }
            public string? Blue { get; set; }
        }
    }
}