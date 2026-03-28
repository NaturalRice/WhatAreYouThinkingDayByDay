using UnityEngine;

public class MapViewerController : MonoBehaviour
{
    [Header("核心设置")]
    public RectTransform mapRectTransform;
    public float minScale = 0.5f;
    public float maxScale = 2.5f;
    public float zoomSpeed = 0.3f;
    public float dragSpeed = 1f;

    private Vector2 dragOrigin;
    private bool isDragging;

    void Update()
    {
        if (mapRectTransform == null) return;

        HandleZoom();
        HandleDrag();
    }

    // 鼠标滚轮缩放
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0) return;

        float currentScale = mapRectTransform.localScale.x;
        float newScale = Mathf.Clamp(currentScale + scroll * zoomSpeed, minScale, maxScale);
        mapRectTransform.localScale = new Vector3(newScale, newScale, 1);
    }

    // 鼠标中键拖动
    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 delta = (Vector2)Input.mousePosition - dragOrigin;
            mapRectTransform.anchoredPosition += delta * dragSpeed;
            dragOrigin = (Vector2)Input.mousePosition;
        }
    }

    // 重置地图位置与大小
    public void ResetMapView()
    {
        mapRectTransform.localScale = Vector3.one;
        mapRectTransform.anchoredPosition = Vector2.zero;
    }
}