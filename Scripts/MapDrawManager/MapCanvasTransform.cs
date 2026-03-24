using UnityEngine;
using UnityEngine.UI;

public class MapCanvasTransform : MonoBehaviour
{
    [Header("画布引用")]
    public RawImage drawBoardUI;
    public RenderTexture mapCanvasRT;

    [Header("缩放设置")]
    public Slider zoomSlider;
    public Text zoomValueText;
    public float minZoom = 0.5f;
    public float maxZoom = 3.0f;
    public float defaultZoom = 1.0f;
    public float wheelZoomSpeed = 0.1f;

    [Header("平移设置")]
    private float currentZoom;
    private Vector2 panOffset;
    private bool isPanning;
    private Vector2 lastPanMousePos;

    private void Awake()
    {
        if (drawBoardUI == null || mapCanvasRT == null)
        {
            Debug.LogError("[MapCanvasTransform] 画布引用未绑定");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        ResetZoomAndPan();
        BindZoomSlider();
    }

    private void Update()
    {
        HandleMouseWheelZoom();
        HandleMousePan();
    }

    /// <summary>
    /// 绑定缩放滑块
    /// </summary>
    public void BindZoomSlider()
    {
        if (zoomSlider == null)
        {
            Debug.LogWarning("[MapCanvasTransform] 缩放滑块未绑定");
            return;
        }
        
        float sliderValue = Mathf.InverseLerp(minZoom, maxZoom, defaultZoom);
        zoomSlider.value = sliderValue;
        zoomSlider.onValueChanged.AddListener(OnZoomSliderChanged);
        UpdateZoomUI();
    }

    /// <summary>
    /// 处理滚轮缩放
    /// </summary>
    private void HandleMouseWheelZoom()
    {
        bool isMouseInBoard = RectTransformUtility.RectangleContainsScreenPoint(drawBoardUI.rectTransform, Input.mousePosition);
        if (isMouseInBoard && Input.mouseScrollDelta.y != 0)
        {
            currentZoom += Input.mouseScrollDelta.y * wheelZoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
            SyncZoomToSlider();
            ApplyTransform();
        }
    }

    /// <summary>
    /// 处理鼠标平移
    /// </summary>
    private void HandleMousePan()
    {
        if (Input.GetMouseButtonDown(2))
        {
            bool isMouseInBoard = RectTransformUtility.RectangleContainsScreenPoint(drawBoardUI.rectTransform, Input.mousePosition);
            if (isMouseInBoard)
            {
                isPanning = true;
                lastPanMousePos = Input.mousePosition;
            }
        }

        if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }

        if (isPanning && Input.GetMouseButton(2))
        {
            Vector2 currentMousePos = Input.mousePosition;
            Vector2 delta = currentMousePos - lastPanMousePos;
            panOffset += delta / currentZoom;
            lastPanMousePos = currentMousePos;
            ApplyTransform();
        }
    }

    /// <summary>
    /// 应用缩放和平移变换
    /// </summary>
    public void ApplyTransform()
    {
        // 限制平移范围
        Rect canvasRect = drawBoardUI.rectTransform.rect;
        float maxX = (canvasRect.width * currentZoom - canvasRect.width) / 2;
        float maxY = (canvasRect.height * currentZoom - canvasRect.height) / 2;
        panOffset.x = Mathf.Clamp(panOffset.x, -maxX, maxX);
        panOffset.y = Mathf.Clamp(panOffset.y, -maxY, maxY);

        // 应用变换
        drawBoardUI.rectTransform.localScale = new Vector3(currentZoom, currentZoom, 1f);
        drawBoardUI.rectTransform.anchoredPosition = panOffset;
    }

    /// <summary>
    /// 重置缩放和平移
    /// </summary>
    public void ResetZoomAndPan()
    {
        currentZoom = defaultZoom;
        panOffset = Vector2.zero;
        SyncZoomToSlider();
        ApplyTransform();
        UpdateZoomUI();
    }

    /// <summary>
    /// 缩放滑块值变更回调
    /// </summary>
    private void OnZoomSliderChanged(float value)
    {
        currentZoom = Mathf.Lerp(minZoom, maxZoom, value);
        UpdateZoomUI();
        ApplyTransform();
    }

    /// <summary>
    /// 同步缩放值到滑块
    /// </summary>
    private void SyncZoomToSlider()
    {
        if (zoomSlider == null) return;
        float sliderValue = Mathf.InverseLerp(minZoom, maxZoom, currentZoom);
        zoomSlider.value = sliderValue;
    }

    /// <summary>
    /// 更新缩放UI显示
    /// </summary>
    private void UpdateZoomUI()
    {
        if (zoomValueText != null)
        {
            zoomValueText.text = $"缩放：{Mathf.Round(currentZoom * 100)}%";
        }
    }

    /// <summary>
    /// 获取鼠标在画布内的坐标（对外提供）
    /// </summary>
    public Vector2 GetMousePosInCanvas()
    {
        Rect rect = drawBoardUI.rectTransform.rect;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(drawBoardUI.rectTransform, Input.mousePosition, null, out localPoint);
        float x = Mathf.Clamp((localPoint.x + rect.width / 2) / rect.width * mapCanvasRT.width, 0, mapCanvasRT.width - 1);
        float y = Mathf.Clamp((localPoint.y + rect.height / 2) / rect.height * mapCanvasRT.height, 0, mapCanvasRT.height - 1);
        return new Vector2(Mathf.Round(x), Mathf.Round(y));
    }
}