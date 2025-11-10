using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public static bool inputIsDisabled = false;
    public float minZoom = 4f;
    public float maxZoom = 9f;
    public float zoomSensitivity = 8.5f;
    public Texture2D grabHandCursor;

    private Camera cam;
    private Vector3 lastMousePos;
    private bool isDragging = false;

    public void Start() {
        cam = gameObject.GetComponent<Camera>();
    }

    private void Update() {
        if (!inputIsDisabled) {
            handleZoom();
            handleMovement();
        }
    }

    public void handleZoom() {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.01f) {
            float newSize = cam.orthographicSize - scrollInput * zoomSensitivity;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    private void handleMovement() {
        if (Input.GetMouseButtonDown(1)) {
            Cursor.SetCursor(grabHandCursor, Vector2.zero, CursorMode.Auto);
            isDragging = true;
            lastMousePos = cam.ScreenToWorldPoint(Input.mousePosition - Vector3.forward * gameObject.transform.position.z);
        }

        if (Input.GetMouseButtonUp(1)) {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            isDragging = false;
        }

        if (isDragging) {
            Vector3 newMousePos = cam.ScreenToWorldPoint(Input.mousePosition - Vector3.forward * gameObject.transform.position.z);
            Vector3 change = lastMousePos - newMousePos;
            Vector3 newPos = cam.transform.position + change;

            newPos.x = Mathf.Clamp(newPos.x, -10, 10);
            newPos.y = Mathf.Clamp(newPos.y, -10, 10);

            cam.transform.position = newPos;
        }

        // Recenter camera
        if (Input.GetKey(KeyCode.C)) {
            cam.transform.position = new Vector3(0,0,-10);
        }
    }
}
