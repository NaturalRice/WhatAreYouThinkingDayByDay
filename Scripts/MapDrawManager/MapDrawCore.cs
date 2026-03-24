using UnityEngine;
using UnityEngine.UI;

public class MapDrawCore : MonoBehaviour
{
    [Header("核心画布")]
    public RenderTexture mapCanvasRT;
    public RawImage drawBoardUI;
    private Texture2D drawTexture;

    [Header("绘制配置")]
    public int brushSize = 15;
    public bool isPenMode = true;

    [Header("依赖模块")]
    public MapCanvasTransform canvasTransform;
    public MapFillSystem fillSystem;
    public MapHistoryManager historyManager;
    public MapUIHandler uiHandler;

    private bool isDrawing;
    private Vector2 lastMousePos;

    private void Awake()
    {
        InitDrawTexture();
    }

    private void Start()
    {
        if (drawTexture == null)
        {
            Debug.LogError("[MapDrawCore] 纹理初始化失败");
            enabled = false;
        }
    }

    private void Update()
    {
        HandleDrawInput();
    }

    /// <summary>
    /// 初始化绘制纹理
    /// </summary>
    private void InitDrawTexture()
    {
        drawTexture = MapTextureUtility.CreateBlankTexture(mapCanvasRT.width, mapCanvasRT.height);
        RefreshCanvasRT();
        drawBoardUI.texture = mapCanvasRT;
    }

    /// <summary>
    /// 处理绘制输入
    /// </summary>
    private void HandleDrawInput()
    {
        if (fillSystem.IsFilling || drawTexture == null) return;

        bool isMouseInBoard = RectTransformUtility.RectangleContainsScreenPoint(drawBoardUI.rectTransform, Input.mousePosition);
        if (Input.GetMouseButtonDown(0) && isMouseInBoard)
        {
            historyManager.SaveCanvasState(); // 委托给历史管理器
            isDrawing = true;
            lastMousePos = canvasTransform.GetMousePosInCanvas();
            DrawPoint(lastMousePos);
        }

        if (Input.GetMouseButton(0) && isDrawing && isMouseInBoard)
        {
            Vector2 currentPos = canvasTransform.GetMousePosInCanvas();
            DrawLine(lastMousePos, currentPos);
            lastMousePos = currentPos;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDrawing = false;
        }
    }

    /// <summary>
    /// 绘制点
    /// </summary>
    private void DrawPoint(Vector2 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int brushHalf = brushSize / 2;
        Color drawColor = isPenMode ? Color.black : Color.white;

        for (int dy = -brushHalf; dy <= brushHalf; dy++)
        {
            for (int dx = -brushHalf; dx <= brushHalf; dx++)
            {
                int targetX = x + dx;
                int targetY = y + dy;
                if (targetX < 0 || targetX >= drawTexture.width || targetY < 0 || targetY >= drawTexture.height)
                    continue;

                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                if (distance <= brushHalf)
                {
                    drawTexture.SetPixel(targetX, targetY, drawColor);
                }
            }
        }
        drawTexture.Apply();
        RefreshCanvasRT();
    }

    /// <summary>
    /// 绘制线
    /// </summary>
    private void DrawLine(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance);
        for (int i = 0; i < steps; i++)
        {
            Vector2 pos = Vector2.Lerp(start, end, (float)i / steps);
            DrawPoint(pos);
        }
    }

    /// <summary>
    /// 刷新RenderTexture（对外提供）
    /// </summary>
    public void RefreshCanvasRT()
    {
        Graphics.Blit(drawTexture, mapCanvasRT);
        drawBoardUI.texture = mapCanvasRT;
    }

    /// <summary>
    /// 清空画布
    /// </summary>
    public void ClearCanvas()
    {
        drawTexture = MapTextureUtility.CreateBlankTexture(mapCanvasRT.width, mapCanvasRT.height);
        RefreshCanvasRT();
        canvasTransform.ResetZoomAndPan(); // 委托给变换模块
        historyManager.ClearHistory(); // 委托给历史模块
    }

    /// <summary>
    /// 获取绘制纹理（对外提供）
    /// </summary>
    public Texture2D GetDrawTexture() => drawTexture;

    /// <summary>
    /// 设置画笔模式（由UIHandler调用）
    /// </summary>
    public void SetPenMode(bool isPen)
    {
        isPenMode = isPen;
    }
}