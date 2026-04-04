using Game.Core.Base;
using UnityEngine;
using UnityEngine.UI;
using Game.Game.Terrain;

/// <summary>
/// 地图UI处理器：独立管理所有UI按钮/滑块事件绑定、提示文本更新、预览更新
/// </summary>
namespace Game.Map
{
    public class MapUIHandler : BasePanel
    {
        [Header("核心UI引用")] public Button penBtn; // 画笔按钮
        public Button eraserBtn; // 橡皮按钮
        public Button fillLandBtn; // 陆地填充按钮
        public Button fillSeaBtn; // 海洋填充按钮
        public Button undoBtn; // 撤回按钮
        public Button clearBtn; // 清空按钮
        public Text tipText; // 提示文本
        public Image brushPreviewImage; // 画笔预览图

        [Header("画笔设置UI")] public Slider brushSizeSlider; // 画笔大小滑块
        public Text brushSizeText; // 画笔大小文本

        [Header("依赖模块")] public MapDrawCore mapDrawCore;
        public MapFillSystem fillSystem;
        public MapHistoryManager historyManager;
        public MapIOManager mapIOManager;
        
        [Header("地形画笔按钮")]
        public Button btnMountain;
        public Button btnForest;
        public Button btnDesert;
        public Button btnRiver;
        public Button btnSaltMine;
        public Button btnIronMine;
        public Button btnGoldMine;
        public Button btnClayHill;

        private void Awake()
        {
            // 校验核心依赖
            if (mapDrawCore == null || fillSystem == null || historyManager == null)
            {
                Debug.LogError("[MapUIHandler] 核心依赖未绑定，功能禁用！");
                enabled = false;
            }
        }

        private void Start()
        {
            // 绑定所有UI事件
            BindAllUIEvents();

            // 初始化UI状态
            UpdateBrushPreview(mapDrawCore.GetCurrentBrushColor(), mapDrawCore.brushSize);
            UpdateTipText("当前模式：黑色画笔 | 绘制陆地轮廓");
            
            // 绑定地形画笔按钮（适配新的SetBrushColor方法）
            btnMountain.onClick.AddListener(() => SetTerrainBrush(TerrainType.Mountain, "山地"));
            btnForest.onClick.AddListener(() => SetTerrainBrush(TerrainType.Forest, "森林"));
            btnDesert.onClick.AddListener(() => SetTerrainBrush(TerrainType.Desert, "沙漠"));
            btnRiver.onClick.AddListener(() => SetTerrainBrush(TerrainType.River, "河流"));
            btnSaltMine.onClick.AddListener(() => SetTerrainBrush(TerrainType.SaltMine, "盐矿"));
            btnIronMine.onClick.AddListener(() => SetTerrainBrush(TerrainType.IronMine, "铁矿"));
            btnGoldMine.onClick.AddListener(() => SetTerrainBrush(TerrainType.GoldMine, "金矿"));
            btnClayHill.onClick.AddListener(() => SetTerrainBrush(TerrainType.ClayHill, "黏土丘"));
        }

        /// <summary>
        /// 地形画笔设置（统一调用SetBrushColor）
        /// </summary>
        private void SetTerrainBrush(TerrainType terrainType, string terrainName)
        {
            fillSystem.ExitFillMode();
            mapDrawCore.SetPenMode(true); // 确保是画笔模式（非橡皮）
            mapDrawCore.SetBrushColor(terrainType);
            UpdateBrushPreview(mapDrawCore.GetCurrentBrushColor(), mapDrawCore.brushSize);
            UpdateTipText($"当前模式：{terrainName}画笔 | 绘制{terrainName}地形");
        }

        #region UI事件绑定

        /// <summary>
        /// 绑定所有UI组件的事件
        /// </summary>
        private void BindAllUIEvents()
        {
            // 1. 画笔/橡皮按钮
            BindPenEraserButtons();

            // 2. 填充按钮
            BindFillButtons();

            // 3. 撤回/清空按钮
            BindUndoClearButtons();

            // 4. 画笔大小滑块
            BindBrushSizeSlider();

            // 5. 导入/导出按钮（委托给IO管理器）
            BindIOButtons();
        }

        /// <summary>
        /// 绑定画笔/橡皮按钮事件（适配新的SetBrushColor）
        /// </summary>
        private void BindPenEraserButtons()
        {
            if (penBtn != null)
            {
                penBtn.onClick.AddListener(() =>
                {
                    fillSystem.ExitFillMode(); // 退出填充模式
                    mapDrawCore.SetPenMode(true); // 切换为画笔模式
                    mapDrawCore.SetBrushColor(null, Color.black); // 恢复黑色普通画笔
                    UpdateBrushPreview(Color.black, mapDrawCore.brushSize);
                    UpdateTipText("当前模式：黑色画笔 | 绘制陆地轮廓");
                });
            }
            else
            {
                Debug.LogWarning("[MapUIHandler] 画笔按钮未绑定！");
            }

            if (eraserBtn != null)
            {
                eraserBtn.onClick.AddListener(() =>
                {
                    fillSystem.ExitFillMode(); // 退出填充模式
                    mapDrawCore.SetPenMode(false); // 切换为橡皮模式
                    UpdateBrushPreview(Color.white, mapDrawCore.brushSize);
                    UpdateTipText("当前模式：白色橡皮 | 擦除错误轮廓");
                });
            }
            else
            {
                Debug.LogWarning("[MapUIHandler] 橡皮按钮未绑定！");
            }
        }

        /// <summary>
        /// 绑定填充按钮事件
        /// </summary>
        private void BindFillButtons()
        {
            if (fillLandBtn != null)
            {
                fillLandBtn.onClick.AddListener(() =>
                {
                    fillSystem.SwitchToLandFill(); // 切换为陆地填充
                    UpdateBrushPreview(fillSystem.landColor, mapDrawCore.brushSize);
                    UpdateTipText("当前模式：陆地填充 | 点击画板闭合区域填充为棕色");
                });
            }
            else
            {
                Debug.LogWarning("[MapUIHandler] 陆地填充按钮未绑定！");
            }

            if (fillSeaBtn != null)
            {
                fillSeaBtn.onClick.AddListener(() =>
                {
                    fillSystem.SwitchToSeaFill(); // 切换为海洋填充
                    UpdateBrushPreview(fillSystem.seaColor, mapDrawCore.brushSize);
                    UpdateTipText("当前模式：海洋填充 | 点击画板闭合区域填充为蓝色");
                });
            }
            else
            {
                Debug.LogWarning("[MapUIHandler] 海洋填充按钮未绑定！");
            }
        }

        /// <summary>
        /// 绑定撤回/清空按钮事件
        /// </summary>
        private void BindUndoClearButtons()
        {
            if (undoBtn != null)
            {
                undoBtn.onClick.AddListener(() =>
                {
                    fillSystem.ExitFillMode(); // 退出填充模式
                    historyManager.Undo(); // 执行撤回（委托给历史管理器）
                    UpdateTipText("已撤回上一步操作");
                });
            }
            else
            {
                Debug.LogWarning("[MapUIHandler] 撤回按钮未绑定！");
            }

            if (clearBtn != null)
            {
                clearBtn.onClick.AddListener(() =>
                {
                    fillSystem.ExitFillMode(); // 退出填充模式
                    mapDrawCore.ClearCanvas(); // 清空画布（委托给绘制核心）
                    UpdateTipText("画布已清空 | 当前模式：黑色画笔");
                    UpdateBrushPreview(Color.black, mapDrawCore.brushSize);
                });
            }
            else
            {
                Debug.LogWarning("[MapUIHandler] 清空按钮未绑定！");
            }
        }

        /// <summary>
        /// 绑定画笔大小滑块
        /// </summary>
        private void BindBrushSizeSlider()
        {
            if (brushSizeSlider == null || brushSizeText == null)
            {
                Debug.LogWarning("[MapUIHandler] 画笔大小滑块/文本未绑定！");
                return;
            }

            // 初始化滑块值（读取本地保存）
            int savedSize = PlayerPrefs.GetInt("SavedBrushSize", mapDrawCore.brushSize);
            mapDrawCore.brushSize = savedSize;
            brushSizeSlider.value = savedSize;
            UpdateBrushSizeText(savedSize);
            UpdateBrushPreview(mapDrawCore.GetCurrentBrushColor(), savedSize);

            // 绑定值变更事件
            brushSizeSlider.onValueChanged.AddListener((float val) =>
            {
                int newSize = Mathf.RoundToInt(val);
                mapDrawCore.brushSize = newSize;
                UpdateBrushSizeText(newSize);
                UpdateBrushPreview(
                    mapDrawCore.isPenMode ? mapDrawCore.GetCurrentBrushColor() : Color.white,
                    newSize
                );
                PlayerPrefs.SetInt("SavedBrushSize", newSize);
            });
        }

        /// <summary>
        /// 绑定导入/导出按钮事件
        /// </summary>
        private void BindIOButtons()
        {
            if (mapIOManager == null)
            {
                Debug.LogWarning("[MapUIHandler] IO管理器未绑定，导入/导出功能禁用！");
                return;
            }

            // 导入图片按钮
            if (mapIOManager.loadImageBtn != null)
            {
                mapIOManager.loadImageBtn.onClick.AddListener(() =>
                {
                    fillSystem.ExitFillMode(); // 退出填充模式
                    historyManager.SaveCanvasState(); // 导入前保存状态
                    mapIOManager.OnLoadImageClick(); // 委托给IO管理器
                    UpdateTipText("正在导入本地图片...");
                });
            }

            // 保存图片按钮
            if (mapIOManager.saveMapBtn != null)
            {
                mapIOManager.saveMapBtn.onClick.AddListener(() =>
                {
                    Texture2D drawTex = mapDrawCore.GetDrawTexture();
                    if (drawTex != null)
                    {
                        mapIOManager.SaveMapToFile(drawTex); // 委托给IO管理器
                        UpdateTipText("正在保存地图图片...");
                    }
                    else
                    {
                        UpdateTipText("保存失败：画布纹理为空！");
                    }
                });
            }
        }

        #endregion

        #region UI更新辅助方法

        /// <summary>
        /// 更新画笔大小显示文本
        /// </summary>
        private void UpdateBrushSizeText(int size)
        {
            brushSizeText.text = $"画笔大小：{size}";
        }

        /// <summary>
        /// 更新画笔预览（颜色+大小）
        /// </summary>
        private void UpdateBrushPreview(Color color, int size)
        {
            if (brushPreviewImage == null) return;

            // 调整预览大小（填充色/画笔色区分）
            int previewSize = color == Color.black || color == Color.white
                ? size + 2
                : size;

            brushPreviewImage.color = color;
            brushPreviewImage.rectTransform.sizeDelta = new Vector2(previewSize, previewSize);
        }

        /// <summary>
        /// 更新提示文本（对外暴露，供其他模块调用）
        /// </summary>
        public void UpdateTipText(string content)
        {
            if (tipText != null)
            {
                tipText.text = content;
            }
            else
            {
                Debug.LogWarning($"[MapUIHandler] 提示文本：{content}");
            }
        }

        #endregion
    }
}