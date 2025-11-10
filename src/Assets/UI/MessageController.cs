using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class MessageController : MonoBehaviour {
    public VisualTreeAsset popupSource;
    private VisualElement popupRoot;
    private Button OKButton;
    private Label titleLabel;
    private Label messageLabel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        popupRoot = popupSource.Instantiate();

        popupRoot.style.position = Position.Absolute;
        popupRoot.style.width = new Length(100, LengthUnit.Percent);
        popupRoot.style.height = new Length(100, LengthUnit.Percent);
        OKButton = popupRoot.Q<Button>("OKButton");
        OKButton.RegisterCallback<ClickEvent>(onOK);
        titleLabel = popupRoot.Q<Label>("Title");
        messageLabel = popupRoot.Q<Label>("MessageContent");
        popupRoot.visible = false;
        
        UIDocument document = gameObject.GetComponent<UIDocument>();
        document.rootVisualElement.Add(popupRoot);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void onOK(ClickEvent e) {
        popupRoot.visible = false;
    }


    public void popupError(string title, string message) {
        titleLabel.text = title;
        messageLabel.text = message;
        popupRoot.visible = true;
    }
}