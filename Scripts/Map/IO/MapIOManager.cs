using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Random = UnityEngine.Random;

/// <summary>
/// 地图IO管理器：独立管理本地图片导入/导出，与绘制核心解耦
/// </summary>
namespace Game.Map
{
    public class MapIOManager : MonoBehaviour
    {
        [Header("IO按钮引用")] public Button loadImageBtn; // 导入图片按钮
        public Button saveMapBtn; // 保存地图按钮

        [Header("画布配置")] public RawImage drawBoardUI; // 画布UI（用于刷新显示）
        public RenderTexture mapCanvasRT; // 画布RenderTexture

        [Header("新增依赖：绘制核心")] public MapDrawCore mapDrawCore; // 绑定绘制核心，用于同步drawTexture

        [Header("优化配置")] private string savePath; // 保存路径
        private bool isRefreshing = false; // 资源刷新节流标记

        private void Awake()
        {
            // 【路径修复】区分编辑器/打包版本的保存路径
#if UNITY_EDITOR
        // 编辑器下保存到Assets/Maps
        savePath = Path.Combine(Application.dataPath, "Maps");
#else
            // 打包后保存到持久化路径
            savePath = Path.Combine(Application.persistentDataPath, "Maps");
#endif

            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
                Debug.Log($"[MapIOManager] 创建保存目录：{savePath}");
            }

            // 校验核心依赖
            if (drawBoardUI == null || mapCanvasRT == null || mapDrawCore == null)
            {
                Debug.LogError("[MapIOManager] 画布/绘制核心引用未绑定，功能禁用！");
                enabled = false;
            }
        }

        #region 图片导入逻辑

        /// <summary>
        /// 导入本地图片（对外暴露，由UIHandler触发）
        /// </summary>
        public void OnLoadImageClick()
        {
            int targetWidth = mapCanvasRT.width;
            int targetHeight = mapCanvasRT.height;

#if UNITY_EDITOR
        // 编辑器下打开文件选择窗口
        string filePath = UnityEditor.EditorUtility.OpenFilePanel(
            "选择地图图片", 
            "", 
            "png,jpg,jpeg,bmp"
        );

        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                LoadImageFromPath(filePath, targetWidth, targetHeight);
            };
        }
        else
        {
            Debug.Log("[MapIOManager] 取消选择图片或文件不存在");
            ShowTip("取消选择图片");
        }
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            // 打包后PC端：读取指定目录下的图片
            ShowStandaloneLoadTip(targetWidth, targetHeight);
#else
        // 移动端/其他平台提示
        ShowTip("当前平台不支持读取本地图片！");
        Debug.LogWarning("[MapIOManager] 当前平台不支持读取本地图片");
#endif
        }

        /// <summary>
        /// 从指定路径加载图片并适配画布
        /// </summary>
        private void LoadImageFromPath(string filePath, int targetWidth, int targetHeight)
        {
            try
            {
                // 读取图片字节
                byte[] imageBytes = File.ReadAllBytes(filePath);
                Texture2D loadTex = new Texture2D(2, 2);
                if (!loadTex.LoadImage(imageBytes))
                {
                    Destroy(loadTex);
                    throw new Exception("图片格式不支持，无法加载");
                }

                // 缩放+翻转纹理（委托给工具类）
                Texture2D resizedTex = MapTextureUtility.ResizeAndFlipTexture(
                    loadTex, targetWidth, targetHeight
                );
                Destroy(loadTex);

                // 【关键修复】将导入的图片同步到绘制核心的drawTexture
                mapDrawCore.GetDrawTexture().SetPixels(resizedTex.GetPixels());
                mapDrawCore.GetDrawTexture().Apply();

                // 刷新RenderTexture（复用绘制核心的刷新逻辑）
                mapDrawCore.RefreshCanvasRT();

                // 提示加载成功
                ShowTip($"导入成功：{Path.GetFileName(filePath)}");
                Debug.Log($"[MapIOManager] 成功导入图片：{filePath}");

                Destroy(resizedTex);
            }
            catch (Exception e)
            {
                ShowTip($"导入失败：{e.Message}");
                Debug.LogError($"[MapIOManager] 导入图片失败：{e.Message}");
            }
        }

        /// <summary>
        /// 打包后PC端的导入提示+自动读取
        /// </summary>
        private void ShowStandaloneLoadTip(int targetWidth, int targetHeight)
        {
            string importPath = Path.Combine(savePath, "Import");
            string tip = $"请将图片放入以下路径后重启游戏：\n{importPath}\n支持格式：png/jpg/jpeg/bmp";

            // 创建导入目录
            if (!Directory.Exists(importPath))
            {
                Directory.CreateDirectory(importPath);
            }

            // 自动读取目录下的第一张图片
            string[] files = Directory.GetFiles(importPath, "*.png");
            if (files.Length == 0) files = Directory.GetFiles(importPath, "*.jpg");

            if (files.Length > 0)
            {
                LoadImageFromPath(files[0], targetWidth, targetHeight);
            }
            else
            {
                ShowTip(tip);
            }
        }

        #endregion

        #region 图片导出逻辑

        /// <summary>
        /// 保存地图到本地文件（对外暴露，由UIHandler触发）
        /// </summary>
        public void SaveMapToFile(Texture2D sourceTex)
        {
            if (sourceTex == null)
            {
                ShowTip("保存失败：画布纹理为空！");
                Debug.LogError("[MapIOManager] 保存失败：源纹理为空");
                return;
            }

            // 生成唯一文件名（时间戳+随机数）
            string fileName = $"Map_{System.DateTime.Now:yyyyMMdd_HHmmss}_{Random.Range(100, 999)}.png";
            string fullPath = Path.Combine(savePath, fileName);

            try
            {
                // 编码为PNG并写入文件
                byte[] pngBytes = sourceTex.EncodeToPNG();
                File.WriteAllBytes(fullPath, pngBytes);

                // 编辑器下异步刷新资源库（避免事务冲突）
#if UNITY_EDITOR
            if (!isRefreshing)
            {
                isRefreshing = true;
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    UnityEditor.AssetDatabase.Refresh();
                    isRefreshing = false;
                };
            }
#endif

                // 提示保存成功
                ShowTip($"保存成功！\n路径：{fullPath}");
                Debug.Log($"[MapIOManager] 成功保存地图：{fullPath}");
            }
            catch (Exception e)
            {
                ShowTip($"保存失败：{e.Message}");
                Debug.LogError($"[MapIOManager] 保存地图失败：{e.Message}");
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 显示操作提示（3秒后重置）
        /// </summary>
        private void ShowTip(string content)
        {
            Debug.Log($"[MapIOManager] {content}");

            // 尝试通过UIHandler更新提示文本（解耦：优先找场景中的UIHandler）
            MapUIHandler uiHandler = FindAnyObjectByType<MapUIHandler>();
            if (uiHandler != null)
            {
                uiHandler.UpdateTipText(content);
                Invoke(nameof(ResetTipText), 3f);
            }
        }

        /// <summary>
        /// 重置提示文本
        /// </summary>
        private void ResetTipText()
        {
            MapUIHandler uiHandler = FindAnyObjectByType<MapUIHandler>();
            if (uiHandler != null)
            {
                uiHandler.UpdateTipText("地图操作完成 | 可继续编辑/导入/保存");
            }
        }

        #endregion
    }
}