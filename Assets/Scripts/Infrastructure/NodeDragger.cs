// Add to each node prefab root as NodeDragger.cs
using UnityEngine;
public class NodeDragger : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera cam;

    void Start() { cam = Camera.main; }

    void OnMouseDown()
    {
        isDragging = true;
        Ray r = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out RaycastHit hit, 100f))
            offset = transform.position - hit.point;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;
        Ray r = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(r, out RaycastHit hit, 100f))
            transform.position = hit.point + offset;
    }

    void OnMouseUp() { isDragging = false; }
}