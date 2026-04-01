using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace Game.Map
{
    public class MapHistoryManager : MonoBehaviour
    {
        [Header("历史配置")] public int maxHistoryCount = 20;
        public float saveThrottle = 0.2f;

        [Header("依赖引用")] public MapDrawCore mapDrawCore;
        public Button undoBtn;

        private Stack<Color[]> canvasHistory;
        private float lastSaveTime;
        private bool isUndoing;

        private void Awake()
        {
            canvasHistory = new Stack<Color[]>();
            if (mapDrawCore == null)
            {
                Debug.LogError("[MapHistoryManager] 绘制核心未绑定");
                enabled = false;
            }
        }

        private void Start()
        {
            // 初始化空白状态
            SaveCanvasState();
            UpdateUndoBtnState();
        }

        /// <summary>
        /// 保存当前画布状态
        /// </summary>
        public void SaveCanvasState()
        {
            if (mapDrawCore == null || isUndoing || Time.time - lastSaveTime < saveThrottle)
                return;

            lastSaveTime = Time.time;
            try
            {
                Texture2D drawTex = mapDrawCore.GetDrawTexture();
                if (drawTex == null) return;

                Color[] currentPixels = drawTex.GetPixels();
                Color[] pixelsCopy = new Color[currentPixels.Length];
                Array.Copy(currentPixels, pixelsCopy, currentPixels.Length);

                // 限制历史栈长度
                if (canvasHistory.Count >= maxHistoryCount)
                {
                    List<Color[]> tempList = canvasHistory.ToList();
                    tempList.RemoveAt(0);
                    canvasHistory = new Stack<Color[]>(tempList.AsEnumerable().Reverse());
                }

                canvasHistory.Push(pixelsCopy);
                UpdateUndoBtnState();
            }
            catch (Exception e)
            {
                Debug.LogError($"[MapHistoryManager] 保存状态失败：{e.Message}");
            }
        }

        /// <summary>
        /// 执行撤回操作
        /// </summary>
        public void Undo()
        {
            if (canvasHistory.Count <= 1 || isUndoing || mapDrawCore == null)
            {
                Debug.LogWarning("[MapHistoryManager] 无更多撤回记录");
                return;
            }

            try
            {
                isUndoing = true;
                canvasHistory.Pop();
                Color[] lastState = canvasHistory.Peek();
                Texture2D drawTex = mapDrawCore.GetDrawTexture();
                drawTex.SetPixels(lastState);
                drawTex.Apply();
                mapDrawCore.RefreshCanvasRT(); // 刷新RenderTexture
                UpdateUndoBtnState();
            }
            catch (Exception e)
            {
                Debug.LogError($"[MapHistoryManager] 撤回失败：{e.Message}");
            }
            finally
            {
                isUndoing = false;
            }
        }

        /// <summary>
        /// 更新撤回按钮状态
        /// </summary>
        private void UpdateUndoBtnState()
        {
            if (undoBtn != null)
            {
                undoBtn.interactable = canvasHistory.Count > 1;
            }
        }

        /// <summary>
        /// 清空历史记录（配合画布清空）
        /// </summary>
        public void ClearHistory()
        {
            canvasHistory.Clear();
            SaveCanvasState(); // 重新保存空白状态
            UpdateUndoBtnState();
        }
    }
}