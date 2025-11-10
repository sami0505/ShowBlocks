using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class AnimationHandler {
    public bool hasFinished;
    public string table;
    public List<string> colNames;
    public int? recordsInserted;
    
    private DBManager dbManager;
    private QueryType queryType;
    private List<int> rowsAffected;
    private List<string> columnsAffected;
    private GameObject animationShape;
    private ShapeScript shapeScript;
    private bool hasDistinct;
    private int? limit;
  

    private Sequence sequence;
    private float DUR = 1f;
    private Ease ease = Ease.OutCubic;
    private bool isStart = true;
    private List<int> sortedARows;
    

    public AnimationHandler(DBManager dbManager, QueryType queryType, List<int> rowsAffected, List<string> columnsAffected, GameObject animationShape, bool hasDistinct, int? limit) {
        hasFinished = false;
        this.dbManager = dbManager;
        this.queryType = queryType;
        this.rowsAffected = rowsAffected;
        this.columnsAffected = columnsAffected;
        this.animationShape = animationShape;
        shapeScript = animationShape.GetComponent<ShapeScript>();
        bool isSorted = queryType != QueryType.INSERT && isSortedAsc(rowsAffected);
        if (isSorted) {
            sortedARows = new List<int>(rowsAffected);
            sortedARows.Sort();
        } else {
            sortedARows = rowsAffected;
        }
        table = shapeScript.columns[0].tableName;
        colNames = dbManager.getColumnNames(table);
        this.hasDistinct = hasDistinct;
        if (queryType == QueryType.INSERT) {
            this.limit = null;
            this.recordsInserted = (int)limit;
        } else {
            this.limit = limit;
            this.recordsInserted = null;
        }
    }

    bool isSortedAsc(List<int> list) {
        return list.SequenceEqual(list.OrderBy(x => x)) && list.Count > 1 ? list[0] > list[1] : true;
    }

    void fadeCell(GameObject cell, bool startsSequence) {
        if (startsSequence)
            sequence.Append(cell.transform.GetChild(0).GetComponent<SpriteRenderer>().DOFade(0, DUR).SetEase(ease));
        else
            sequence.Join(cell.transform.GetChild(0).GetComponent<SpriteRenderer>().DOFade(0, DUR).SetEase(ease));
        sequence.Join(cell.transform.GetChild(1).GetComponent<SpriteRenderer>().DOFade(0, DUR).SetEase(ease));
        sequence.Join(cell.transform.GetChild(2).GetComponent<TextMeshPro>().DOFade(0, DUR).SetEase(ease).OnComplete(() => GameObject.Destroy(cell)));
    }
    
    void fadeRow(int index, bool awaitFade) {
        List<GameObject> row = shapeScript.getRow(index);
        foreach (var cell in row) {
            fadeCell(cell, isStart || awaitFade);
            isStart = false;
            awaitFade = false;
        }
    }

    void unfadeCell(GameObject cell, bool startsSequence) {
        if (startsSequence)
            sequence.Append(cell.transform.GetChild(0).GetComponent<SpriteRenderer>().DOFade(1f, DUR).SetEase(ease));
        else
            sequence.Join(cell.transform.GetChild(0).GetComponent<SpriteRenderer>().DOFade(1f, DUR).SetEase(ease));
        sequence.Join(cell.transform.GetChild(1).GetComponent<SpriteRenderer>().DOFade(1f, DUR).SetEase(ease));
        sequence.Join(cell.transform.GetChild(2).GetComponent<TextMeshPro>().DOFade(1f, DUR).SetEase(ease));
    }

    void unfadeRow(int index) {
        List<GameObject> row = shapeScript.getRow(index);
        bool isFirstCell = true;
        foreach (var cell in row) {
            unfadeCell(cell, isFirstCell);
            isFirstCell = false;
        }
    }
    
    void moveRowTo(int initialIndex, int targetIndex) {
        List<GameObject> row = shapeScript.getRow(initialIndex);
        int change = targetIndex - initialIndex;
        foreach (GameObject obj in row) {
            float newY = obj.transform.position.y - change;
            if (isStart) {
                sequence.Append(obj.transform.DOMoveY(newY, DUR).SetEase(ease));
                isStart = false;
            }
            else {
                sequence.Join(obj.transform.DOMoveY(newY, DUR).SetEase(ease));
            }
        }
    }

    void fadeCol(string colName, bool isStart) {
        List<GameObject> col = shapeScript.getColumn(colName);
        foreach (var obj in col) {
            fadeCell(obj, isStart);
            isStart = false;
        }
    }

    void moveColTo(int initialIndex, int targetIndex, bool isStart) {
        List<GameObject> col = shapeScript.getColumn(initialIndex);
        int change = targetIndex - initialIndex;
        foreach (GameObject obj in col) {
            float newX = obj.transform.position.x + change * 2;
            if (isStart) {
                sequence.Append(obj.transform.DOMoveX(newX, DUR).SetEase(ease));
                isStart = false;
            } else {
                sequence.Join(obj.transform.DOMoveX(newX, DUR).SetEase(ease));
            }
        }
    }

    void fadeRowsToward(int initialIndex, List<int> indicesToFade) {
        foreach (int i  in indicesToFade) {
            fadeRow(i, false);
            moveRowTo(i, initialIndex);
        }

    }

    void beginCallbackChain() {
        if (queryType == QueryType.SELECT || queryType == QueryType.DELETE)
            rowFadeStep();
        else
            insertStep();
    }

    void insertStep () {
        sequence = DOTween.Sequence();
        sequence.AppendInterval(DUR);
        for (int i = shapeScript.shapeCells.Count-1; i >= 0; i--)
            unfadeRow(i);
        sequence.AppendCallback(() => { hasFinished  = true; });
        sequence.Play();

    }

    void rowFadeStep() {
        sequence = DOTween.Sequence();
        sequence.AppendInterval(DUR);
        List<int> rowsToFade = Enumerable.Range(1, shapeScript.shapeCells.Count).Except(rowsAffected).ToList();
        // Animate only if necessary
        if (rowsToFade.Count > 0) {
            foreach (int pk in rowsToFade) {
                fadeRow(pk - 1, true);
                isStart = false;
            }
            sequence.AppendInterval(DUR);
            isStart = true;
            foreach (int pk in sortedARows) {
                int indexInSorted = sortedARows.IndexOf(pk);
                if (pk - 1 != indexInSorted) {
                    moveRowTo(pk - 1, indexInSorted);
                    isStart = false;
                }
            }
            sequence.AppendInterval(DUR);
        }
        sequence.AppendCallback(() => {
            rowsToFade.Reverse();  // Done to ensure the indices are not disturbed at removal
            rowsToFade.ForEach(r => shapeScript.removeRow(r - 1));
            orderStep();
        });
        sequence.Play();
    }

    void orderStep() {
        sequence = DOTween.Sequence();
        if (sortedARows != rowsAffected) {
            isStart = true;
            foreach (int pk in rowsAffected) {
                int currentIndex = sortedARows.IndexOf(pk);
                if (currentIndex != rowsAffected.IndexOf(pk)) {
                    moveRowTo(currentIndex, rowsAffected.IndexOf(pk));
                    isStart = false;
                }
            }
            sequence.AppendInterval(DUR);
            sequence.AppendCallback(() => {
                var newShapeCells = new List<List<GameObject>>();
                List<int> indicesInData = new List<int>();
                foreach (int pk in rowsAffected) {
                    int indexInData = sortedARows.IndexOf(pk);
                    indicesInData.Add(indexInData);
                    newShapeCells.Add(shapeScript.getRow(indexInData));
                }
                shapeScript.shapeCells = newShapeCells;
                foreach (Column col in shapeScript.columns) {
                    col.setOrderTo(indicesInData);
                }
            });
        }
        sequence.AppendCallback(() => {
            columnProjectionStep();
        });
        sequence.Play();
    }

    void columnProjectionStep() {
        sequence = DOTween.Sequence();
        List<string> colsToFade = colNames.Except(columnsAffected).ToList();  // TODO: Ensure this works for joined tables
        if (colsToFade.Count > 0) {
            isStart = true;
            foreach (string col in colsToFade) {
                fadeCol(col, isStart);
                isStart = false;
            }
            isStart = true;
            for (int i = 0; i < columnsAffected.Count; i++) {
                moveColTo(colNames.IndexOf(columnsAffected[i]), i, isStart);
                isStart = false;
            }
            sequence.AppendInterval(DUR);
        }
        sequence.AppendCallback(() => {
            colsToFade.ForEach(c => shapeScript.removeColumn(c));
            mergeDistinctStep();
        });
        sequence.Play();
    }

    void mergeDistinctStep() {
        List<Tuple<int, List<int>>> duplicates;
        if (hasDistinct && (duplicates = shapeScript.getDuplicates()).Count > 0) {
            List<int> removedIndex = new List<int>();
            merge(duplicates, removedIndex);
        } else {
            limitFadeStep();
        }
    }

    void merge(List<Tuple<int, List<int>>> duplicates, List<int> removedIndex) {
        if (duplicates.Count > 0) {
            sequence = DOTween.Sequence();
            // Normalise duplicate list indices to current coordinates
            var entry = duplicates[0];
            int initialIndex = entry.Item1 - removedIndex.Count(r => r < entry.Item1);
            List<int> indicesToFade = new List<int>(entry.Item2);
            for (int i = 0; i < indicesToFade.Count; i++)
                indicesToFade[i] -= removedIndex.Count(indexFaded => indexFaded < indicesToFade[i]);

            isStart = true;
            fadeRowsToward(initialIndex, indicesToFade);
            for (int i = initialIndex + 1; i < shapeScript.shapeCells.Count; i++) {
                int change = indicesToFade.Count(indexFaded => indexFaded < i);
                moveRowTo(i, i - change);
            }
            indicesToFade.Reverse();
            foreach (int i in indicesToFade) {
                shapeScript.removeRow(i);
                rowsAffected.RemoveAt(i);
                removedIndex.Add(i);
            }
            sequence.AppendInterval(DUR);
            duplicates.RemoveAt(0);

            sequence.AppendCallback(() => merge(duplicates, removedIndex));
            sequence.Play();
        } else {
            limitFadeStep();
        }
    }

    void limitFadeStep() {
        sequence = DOTween.Sequence();
        if (limit != null) {
            isStart = true;
            for (int i = (int)limit; i < rowsAffected.Count; i++) {
                fadeRow(i, isStart);
                isStart = false;
            }
            sequence.AppendInterval(DUR);
        }
        // NOTE: Add this callback to potential animation conclusions
        sequence.AppendCallback(() => { hasFinished = true; });
        sequence.Play();
    }

    public void animateQuery() {
        // TODO: Handle Joins
        beginCallbackChain();
    }
}
