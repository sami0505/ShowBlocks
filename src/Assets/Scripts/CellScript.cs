using TMPro;
using UnityEngine;

public class CellScript : MonoBehaviour
{
    
    public string CellLabel { get { return cellLabel; } set { cellLabel = value; refreshLabel(); } }

    public Color cellColor = Color.white;
    public GameObject labelPrefab;

    private string cellLabel;
    private GameObject labelInstance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        setColour();
        labelInstance = Instantiate(labelPrefab, transform.TransformPoint(Vector3.zero), Quaternion.identity, transform);
        refreshLabel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void setColour() {
        if (cellColor != null) {
            transform.Find("Space").GetComponent<SpriteRenderer>().material.color = cellColor;
        }
    }

    void refreshLabel() {
        if (labelInstance == null) {
            return;
        }

        TextMeshPro textMeshPro = labelInstance.GetComponent<TextMeshPro>();
        textMeshPro.text = cellLabel;
    }
}
