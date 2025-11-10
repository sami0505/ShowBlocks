using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;

public class ShapeScript : MonoBehaviour {
    public GameObject cellPrefab;
    public int initRowCount;
    public List<Column> columns;
    public bool isDrawn;

    // 2D array of all the cells in the shape
    public List<List<GameObject>> shapeCells;

    void Start() {
        shapeCells = new List<List<GameObject>>();
        for (int i = 0; i < initRowCount; i++)
            shapeCells.Add(new List<GameObject>());

        gameObject.name = columns[0].tableName;
        spawnShape();
    }

    void spawnShape() {
        Vector2 shapePos = gameObject.transform.position + Vector3.right + Vector3.down / 2;
        for (int colIndex = 0; colIndex < columns.Count; colIndex++) {
            // NOTE: This relies on the columns being named after colours
            Color colour = Colours.getColour(columns[colIndex].name);

            // Fill column with cells
            for (int rowIndex = 0; rowIndex < shapeCells.Count; rowIndex++) {
                Vector2 position = new Vector2(colIndex*2 + shapePos.x, shapePos.y - rowIndex);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);

                CellScript currentCScript = cell.GetComponent<CellScript>();
                currentCScript.cellColor = colour;
                currentCScript.CellLabel = columns[colIndex].getCellText(rowIndex);
                
                shapeCells[rowIndex].Add(cell);
            }
        }
        isDrawn = true;
    }

    public List<GameObject> getRow(int rowIndex) {
        return new List<GameObject>(shapeCells[rowIndex]);
    }

    public List<GameObject> getColumn(int columnIndex) {
        List<GameObject> col = new List<GameObject>();
        for (int row = 0; row < shapeCells.Count; row++)
            col.Add(shapeCells[row][columnIndex]);
        return col;
    }

    public List<GameObject> getColumn(string colName) {
        bool useDotNotation = colName.Contains(".");
        int colIndex = columns.FindIndex(c => (useDotNotation ? $"{c.tableName}." : "") + c.name == colName);
        return getColumn(colIndex);
    }

    public GameObject getCell(int rowIndex, int colIndex) {
        return shapeCells[rowIndex][colIndex];
    }

    public void removeRow(int rowIndex) {
        columns.ForEach(c => c.removeAt(rowIndex));
        shapeCells.RemoveAt(rowIndex);
    }
    public void removeColumn(int columnIndex) {
        columns.RemoveAt(columnIndex);
        foreach (var row in shapeCells)
            row.RemoveAt(columnIndex);
    }

    public void removeColumn(string colName) {
        bool useDotNotation = colName.Contains(".");
        int colIndex = columns.FindIndex(c => (useDotNotation ? $"{c.tableName}." : "") + c.name == colName);
        removeColumn(colIndex);
    }

    public void appendRow(List<GameObject> row) {
        if (row.Count != columns.Count) {
            Debug.LogError("Row does not fit!");
            return;
        }
        shapeCells.Add(row);
    }

    public void appendColumn(List<GameObject> col) {
        if (col.Count != shapeCells.Count) {
            Debug.LogError("Column does not fit!");
            return;
        }

        for (int i = 0; i < col.Count; i++)
            shapeCells[i].Add(col[i]);
    }

    public List<Tuple<int, List<int>>> getDuplicates() {
        bool columnRowsAreEqual(Column col, int initIdx, int otherIdx) {
            if (col.getType() == "Int") {
                List<int?> data = ((IntColumn)col).data;
                return data[initIdx] == data[otherIdx];
            }
            else if (col.getType() == "Text") {
                List<string> data = ((TextColumn)col).data;
                return data[initIdx] == data[otherIdx];
            }
            else {
                throw new ArgumentException("Invalid column type!");
            }
        }

        var duplicateEntries = new List<Tuple<int, List<int>>>();

        for (int origRowIndex = 0; origRowIndex < shapeCells.Count; origRowIndex++) {
            // If this has been proven to be a duplicate, skip
            if (duplicateEntries.Any(e => e.Item2.Contains(origRowIndex)))
                continue;

            var duplicatesOfOriginal = new List<int>();
            for (int otherRowIndex = origRowIndex + 1; otherRowIndex < shapeCells.Count; otherRowIndex++) {
                bool isDuplicate = columns.All(c => columnRowsAreEqual(c, origRowIndex, otherRowIndex));
                if (isDuplicate)
                    duplicatesOfOriginal.Add(otherRowIndex);
            }

            if (duplicatesOfOriginal.Count > 0)
                duplicateEntries.Add(new Tuple<int, List<int>>(origRowIndex, duplicatesOfOriginal));
        }

        return duplicateEntries;
    }

    public void makeRowTransparent(int index) {
        List<GameObject> row = shapeCells[index];

        for (int i = 0; i < row.Count; i++) {
            GameObject cell = row[i];
            Color spaceColor = cell.transform.GetChild(0).GetComponent<SpriteRenderer>().color;
            Color outlineColor = cell.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
            spaceColor.a = 0;
            outlineColor.a = 0;
            
            cell.transform.GetChild(0).GetComponent<SpriteRenderer>().color = spaceColor;
            cell.transform.GetChild(1).GetComponent<SpriteRenderer>().color = outlineColor;
            cell.transform.GetChild(2).GetComponent<TextMeshPro>().alpha = 0;
        }
    }

    public void makeLastRowsTransparent(int count) {
        for (int i = 0; i < count; i++) {
            makeRowTransparent(shapeCells.Count - i - 1);
        }
    }
}