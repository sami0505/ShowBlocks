using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class InGameController : MonoBehaviour
{
    public UIDataHandler handler;

    private UIDocument document;
    private ScrollView structurePanel;
    private Button runQueryButton;
    private MessageController messageController;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        document = gameObject.GetComponent<UIDocument>();
        structurePanel = document.rootVisualElement.Q<ScrollView>("StructurePanel");
        runQueryButton = document.rootVisualElement.Q<Button>("RunButton");
        runQueryButton.RegisterCallback<ClickEvent>(OnQueryClick);
        messageController = gameObject.GetComponent<MessageController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnQueryClick(ClickEvent e) {
        GameObject.FindWithTag("GameController").GetComponent<GameManager>().tryQuery();
    }

    public void addFoldouts() {
        DBManager dbManager = DBManager.getInstance();
        foreach (var table in dbManager.getTableNames()) {
            Foldout foldout = new Foldout();
            foldout.text = table;
            foldout.value = false;
            
            List<string> columns = dbManager.getTableStructure(table);
            foreach (var columnName in columns) {
                foldout.Add(new Label(columnName));
            }
            structurePanel.Add(foldout);
        }
    }
    
    public void setUIVisibility(bool visible) {
        document.rootVisualElement.Q<VisualElement>("Panel").visible = visible;
    }

    public void popupError(string title, string message) {
        messageController.popupError(title, message);
    }
}
