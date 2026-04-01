using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 地图填充系统：独立管理陆地/海洋泛洪填充、填充模式切换
/// </summary>
namespace Game.Map
{
    public class MapFillSystem : MonoBehaviour
    {
        [Header("填充配置")] public Color landColor = new Color(0.5f, 0.8f, 0.5f); // 陆地填充色
        public Color seaColor = new Color(0.2f, 0.5f, 0.8f); // 海洋填充色
        [Range(0f, 1f)] public float fillSensitivity = 0.1f; // 填充灵敏度
        public float fillClickLockDelay = 0.1f; // 填充点击锁延迟

        [Header("UI引用")] public Slider fillSensitivitySlider;
        public Text fillSensitivityText;

        [Header("依赖引用")] public MapDrawCore mapDrawCore;
        public MapCanvasTransform canvasTransform;
        public MapHistoryManager historyManager;

        // 填充模式枚举
        public enum FillMode
        {
            None,
            Land,
            Sea
        }

        public FillMode CurrentFillMode { get; private set; } = FillMode.None;
        public bool IsFilling => CurrentFillMode != FillMode.None; // 对外暴露是否处于填充模式
        private bool isFillClickLocked = false; // 填充点击锁（防止触发手绘）

        private void Awake()
        {
            // 校验核心依赖
            if (mapDrawCore == null || canvasTransform == null || historyManager == null)
            {
                Debug.LogError("[MapFillSystem] 核心依赖未绑定，功能禁用！");
                enabled = false;
            }
        }

        private void Start()
        {
            // 初始化填充灵敏度滑块
            BindFillSensitivitySlider();
        }

        private void Update()
        {
            // 仅在填充模式下处理点击
            if (CurrentFillMode != FillMode.None && !isFillClickLocked)
            {
                HandleFillInput();
            }
        }

        #region 填充核心逻辑

        /// <summary>
        /// 处理填充模式的鼠标点击
        /// </summary>
        private void HandleFillInput()
        {
            bool isMouseInBoard = RectTransformUtility.RectangleContainsScreenPoint(
                canvasTransform.drawBoardUI.rectTransform, Input.mousePosition);

            if (isMouseInBoard && Input.GetMouseButtonDown(0))
            {
                // 填充前保存状态（委托给历史管理器）
                historyManager.SaveCanvasState();

                // 加锁防止手绘逻辑响应本次点击
                isFillClickLocked = true;

                // 获取鼠标在画布内的坐标（委托给变换模块）
                Vector2 fillPos = canvasTransform.GetMousePosInCanvas();
                Color targetColor = CurrentFillMode == FillMode.Land ? landColor : seaColor;

                // 执行泛洪填充
                FloodFill((int)fillPos.x, (int)fillPos.y, targetColor);

                // 延迟释放锁
                Invoke(nameof(ReleaseFillClickLock), fillClickLockDelay);
            }
        }

        /// <summary>
        /// 泛洪填充算法（染料桶核心）
        /// </summary>
        private void FloodFill(int startX, int startY, Color targetColor)
        {
            Texture2D drawTex = mapDrawCore.GetDrawTexture();
            if (drawTex == null)
            {
                Debug.LogError("[MapFillSystem] 填充失败：绘制纹理为空！");
                return;
            }

            int width = drawTex.width;
            int height = drawTex.height;

            // 边界校验
            if (startX < 0 || startX >= width || startY < 0 || startY >= height)
            {
                Debug.LogWarning("[MapFillSystem] 填充失败：点击位置超出画布范围！");
                return;
            }

            Color[] pixels = drawTex.GetPixels();
            Color originColor = pixels[startY * width + startX];

            // 目标色与原始色一致则无需填充
            if (MapTextureUtility.IsColorSimilar(originColor, targetColor, fillSensitivity))
            {
                Debug.Log("[MapFillSystem] 填充跳过：目标色与原始色一致");
                return;
            }

            // 广度优先搜索（BFS）实现泛洪填充
            Queue<Vector2Int> fillQueue = new Queue<Vector2Int>();
            HashSet<int> filledPixels = new HashSet<int>();
            fillQueue.Enqueue(new Vector2Int(startX, startY));

            // 四邻域方向（上下左右）
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            while (fillQueue.Count > 0)
            {
                Vector2Int pos = fillQueue.Dequeue();
                int x = pos.x;
                int y = pos.y;
                int pixelIndex = y * width + x;

                // 边界/已填充/颜色不匹配 则跳过
                if (x < 0 || x >= width || y < 0 || y >= height) continue;
                if (filledPixels.Contains(pixelIndex)) continue;
                if (!MapTextureUtility.IsColorSimilar(pixels[pixelIndex], originColor, fillSensitivity)) continue;

                // 填充颜色并标记
                pixels[pixelIndex] = targetColor;
                filledPixels.Add(pixelIndex);

                // 四邻域入队
                for (int i = 0; i < 4; i++)
                {
                    fillQueue.Enqueue(new Vector2Int(x + dx[i], y + dy[i]));
                }
            }

            // 应用填充结果到纹理
            drawTex.SetPixels(pixels);
            drawTex.Apply();
            mapDrawCore.RefreshCanvasRT();

            Debug.Log($"[MapFillSystem] 填充完成：{filledPixels.Count}个像素，颜色={targetColor}");
        }

        /// <summary>
        /// 释放填充点击锁
        /// </summary>
        private void ReleaseFillClickLock()
        {
            isFillClickLocked = false;
        }

        #endregion

        #region 外部调用接口（由UIHandler触发）

        /// <summary>
        /// 切换到陆地填充模式
        /// </summary>
        public void SwitchToLandFill()
        {
            CurrentFillMode = FillMode.Land;
            Debug.Log("[MapFillSystem] 切换为陆地填充模式");
        }

        /// <summary>
        /// 切换到海洋填充模式
        /// </summary>
        public void SwitchToSeaFill()
        {
            CurrentFillMode = FillMode.Sea;
            Debug.Log("[MapFillSystem] 切换为海洋填充模式");
        }

        /// <summary>
        /// 退出填充模式
        /// </summary>
        public void ExitFillMode()
        {
            CurrentFillMode = FillMode.None;
            Debug.Log("[MapFillSystem] 退出填充模式");
        }

        #endregion

        #region 灵敏度滑块绑定

        /// <summary>
        /// 绑定填充灵敏度滑块
        /// </summary>
        private void BindFillSensitivitySlider()
        {
            if (fillSensitivitySlider == null || fillSensitivityText == null)
            {
                Debug.LogWarning("[MapFillSystem] 填充灵敏度滑块/文本未绑定！");
                return;
            }

            // 初始化滑块值
            fillSensitivitySlider.value = fillSensitivity;
            UpdateFillSensitivityText(fillSensitivity);

            // 绑定值变更事件
            fillSensitivitySlider.onValueChanged.AddListener((float val) =>
            {
                fillSensitivity = val;
                UpdateFillSensitivityText(val);
            });
        }

        /// <summary>
        /// 更新填充灵敏度显示文本
        /// </summary>
        private void UpdateFillSensitivityText(float value)
        {
            fillSensitivityText.text = $"填充灵敏度：{Mathf.Round(value * 100)}%";
        }

        #endregion
    }
}