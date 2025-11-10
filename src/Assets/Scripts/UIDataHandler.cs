using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "UIDataHandler", menuName = "Scriptable Objects/UIDataHandler")]
public class UIDataHandler : ScriptableObject {
    public string levelGoalText = "Ipsum";
    public string query;
    public bool isInitialised;
}
