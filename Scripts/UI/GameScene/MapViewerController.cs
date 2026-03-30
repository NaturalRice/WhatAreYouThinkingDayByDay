using UnityEngine;

public class MapViewerController : MonoBehaviour
{
    [Header("核心设置")]
    public RectTransform mapRectTransform;    // 拖入 RawImage_Map
    public RectTransform viewportRect;        // 拖入 Panel_GameBg

    [Header("缩放设置")]
    public float minScale = 0.5f;
    public float maxScale = 50.0f;    // 放大上限拉满
    public float zoomSpeed = 5.0f;

    [Header("移动设置")]
    public float moveSpeed = 800f;
    public float dragSpeed = 3.0f;

    private Vector2 dragOrigin;
    private bool isDragging;

    void Update()
    {
        if (mapRectTransform == null) return;

        HandleZoom();
        HandleWASDMove();
        HandleDrag();
        UpdateBounds();
    }

    // 滚轮缩放
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll == 0) return;

        float currentScale = mapRectTransform.localScale.x;
        float newScale = Mathf.Clamp(currentScale + scroll * zoomSpeed, minScale, maxScale);
        mapRectTransform.localScale = new Vector3(newScale, newScale, 1);
    }

    // WSAD 移动
    void HandleWASDMove()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(h, v).normalized;
        Vector2 move = inputDir * moveSpeed * Time.deltaTime;
        mapRectTransform.anchoredPosition += -move;
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

    // 边界限制：永远不会超出屏幕
    void UpdateBounds()
    {
        if (viewportRect == null) return;

        float scale = mapRectTransform.localScale.x;
        float width = mapRectTransform.rect.width * scale;
        float height = mapRectTransform.rect.height * scale;

        float viewW = viewportRect.rect.width;
        float viewH = viewportRect.rect.height;

        float maxX = Mathf.Max(0, width - viewW) / 2;
        float maxY = Mathf.Max(0, height - viewH) / 2;

        float x = Mathf.Clamp(mapRectTransform.anchoredPosition.x, -maxX, maxX);
        float y = Mathf.Clamp(mapRectTransform.anchoredPosition.y, -maxY, maxY);

        mapRectTransform.anchoredPosition = new Vector2(x, y);
    }

    // 重置地图
    public void ResetMapView()
    {
        mapRectTransform.localScale = Vector3.one;
        mapRectTransform.anchoredPosition = Vector2.zero;
    }
}