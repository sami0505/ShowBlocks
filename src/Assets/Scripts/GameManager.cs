using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{
    public static int level = 0;
    public GameObject shapePrefab;
    public UIDataHandler handler;
    public GameObject playerCameraObj;
    public GameObject cutsceneCameraObj;

    private DBManager dbManager;
    private LevelContext levelContext;
    private List<GameObject> shapes = new List<GameObject>();
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelContext = LCDict.dict[level];
        cutsceneCameraObj.GetComponent<Camera>().enabled = false;
        cutsceneCameraObj.GetComponent<AudioListener>().enabled = false;
        dbManager = DBManager.getInstance();
        dbManager.initDBLevel(level);
        List<string> tables = dbManager.getTableNames();

        for (int i = 0; i < tables.Count; i++) {
            string table = tables[i];
            GameObject shape = makeShape(table, LCDict.dict[level].positions[i]);  // TODO: Fix position picker
            shapes.Add(shape);
        }

        handler.levelGoalText = levelContext.levelGoalText;
        // Makes sure that query from previous session is wiped, as the query will be changed only when CommandInput is first marked dirty
        handler.query = "";
        GameObject.FindWithTag("UI").GetComponent<InGameController>().addFoldouts();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public GameObject makeShape(string table, Vector3 position) {
        GameObject shape = Instantiate(shapePrefab, position, Quaternion.identity);
        ShapeScript CurrentSScript = shape.GetComponent<ShapeScript>();
        CurrentSScript.columns = dbManager.getColumns(table);
        CurrentSScript.initRowCount = dbManager.getRowCount(table);
        return shape;
    }
    IEnumerator WaitUntilDrawn(AnimationHandler animationHandler, ShapeScript animationShapeScript) {
        yield return new WaitUntil(() => animationShapeScript.isDrawn);
        if (animationHandler.recordsInserted != null)
            animationShapeScript.makeLastRowsTransparent((int)animationHandler.recordsInserted);
        animationHandler.animateQuery();
    }
    
    IEnumerator WaitUntilAnimated(AnimationHandler animationHandler, List<int> rowsAffectedPks, List<string> columnsAffected, GameObject animationShape) {
        yield return new WaitUntil(() => animationHandler.hasFinished);
        if (hasPassed(rowsAffectedPks, columnsAffected)) {
            // TODO: Implement passing level thingy:
            // Popup with pass screen, either option for next level or return to title
            Debug.Log("PASSED");
        }
        else {
            if (animationHandler.recordsInserted != null)
                dbManager.undoInsertSideEffects(animationHandler.table, animationHandler.colNames[0], (int)animationHandler.recordsInserted);
            flipCameras();
            Destroy(animationShape);
        }
        
        GameObject.FindWithTag("UI").GetComponent<InGameController>().setUIVisibility(true);
        // Unfreeze movement
        CameraMovement.inputIsDisabled = false;
    }

    public void tryQuery() {
        // Freeze input
        CameraMovement.inputIsDisabled = true;

        DBManager.ValidationResult validationResult = dbManager.isValidQuery(handler.query);
        if (!validationResult.isValid) {
            // Likely valid SQL, invalid for puzzle
            GameObject.FindWithTag("UI").GetComponent<InGameController>()
                .popupError("Query Sandbox Limit", validationResult.errorMessage);
            return;
        }

        List<int> rowsAffectedPks;

        try {
            rowsAffectedPks = dbManager.runQuery(handler.query);
        } catch (SQLite.SQLiteException e) {  // Error with SQL, expected
            GameObject.FindWithTag("UI").GetComponent<InGameController>().popupError("Major SQL Error", e.Message);
            return;
        } catch (ArgumentException e) {  // Error with something internal
            Debug.LogError(e);
            Application.Quit();
            return; // Purely here to satisfy the compiler
        }

        string tableAffected;
        List<string> columnsAffected;
        bool hasDistinct;
        int? limit;
        QueryType queryType;
        if (DBManager.startsLike(handler.query, "SELECT")) {
            queryType = QueryType.SELECT;
            var querySelection = DBManager.getQuerySelections(handler.query);
            columnsAffected = querySelection.Item1;
            tableAffected = querySelection.Item2;
            if (columnsAffected[0] == "*")
                columnsAffected = dbManager.getColumnNames(tableAffected);
            hasDistinct = handler.query.Contains("DISTINCT", StringComparison.OrdinalIgnoreCase);
            limit = DBManager.getLimit(handler.query);
        } else if (DBManager.startsLike(handler.query, "DELETE")) {
            queryType = QueryType.DELETE;
            List<string> queryWords = DBManager.getQueryWords(handler.query);
            tableAffected = queryWords[2];  // The table's name is always [2] , DELETE FROM ___
            columnsAffected = dbManager.getColumnNames(tableAffected);
            hasDistinct = false;
            limit = null;
        } else if (DBManager.startsLike(handler.query, "INSERT")) {
            queryType = QueryType.INSERT;
            List<string> queryWords = DBManager.getQueryWords(handler.query);
            tableAffected = queryWords[2];  // The table's name is always [2] , INSERT INTO ___
            columnsAffected = dbManager.getColumnNames(tableAffected);
            hasDistinct = false;
            limit = dbManager.getCountOfRowsAffected();  // Limit here actually contains the number of records inserted
        } else {
            // Should be a drop command, as validated earlier
            // TODO: Implement DROP Logic
            queryType = QueryType.DROP;
            return;
        }
        GameObject animationShape = makeShape(tableAffected, new Vector3(-3,52.5f,0));  // TODO: Consider XYZ placement calculation??
        animationShape.name = "Animation Shape";

        // Change cam
        flipCameras();

        GameObject.FindWithTag("UI").GetComponent<InGameController>().setUIVisibility(false);
        ShapeScript animationShapeScript = animationShape.GetComponent<ShapeScript>();
        AnimationHandler animationHandler = new AnimationHandler(dbManager, queryType, rowsAffectedPks, columnsAffected, animationShape, hasDistinct, limit);
        columnsAffected.ForEach(column => { DBManager.addTablePrefixToColumn(column, tableAffected); });
        StartCoroutine(WaitUntilDrawn(animationHandler, animationShapeScript));
        StartCoroutine(WaitUntilAnimated(animationHandler, rowsAffectedPks, columnsAffected, animationShape));
    }

    bool hasPassed(List<int> rowsAffectedPks, List<string> columnsAffected) {
        // NOTE: Column order matters in real SQL, as it should here
        return levelContext.columnsAffected.SequenceEqual(columnsAffected) && levelContext.rowsAffectedPks.SequenceEqual(rowsAffectedPks);
    }

    private void flipCameras() {
        Camera playerCamera = playerCameraObj.GetComponent<Camera>();
        AudioListener playerListener = playerCameraObj.GetComponent<AudioListener>();
        Camera cutsceneCamera = cutsceneCameraObj.GetComponent<Camera>();
        AudioListener cutsceneListener = cutsceneCameraObj.GetComponent<AudioListener>();

        playerCamera.enabled = !playerCamera.enabled;
        cutsceneCamera.enabled = !cutsceneCamera.enabled;
        playerListener.enabled = !playerListener.enabled;
        cutsceneListener.enabled = !cutsceneListener.enabled;
    }
}
