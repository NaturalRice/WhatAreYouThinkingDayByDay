using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Core.Base;
using Game.Map;
using static Game.Game.Nation.NationCityManager;

namespace Game.Game.Nation
{
    public class NationTerritoryManager : BaseManager
    {
        [Header("核心")] public RectTransform mapRoot;
        public NationCityManager cityManager;
        public NationSettingPanel nationPanel;
        public CityTypeConfig cityTypeConfig;
        public RawImage territoryMask;

        [Header("领土基础半径")] public float baseTerritoryRadius = 50f;
        [Range(0.1f, 0.5f)] public float territoryAlpha = 0.3f;

        private Texture2D territoryTex;
        private int lastCityCount = 0;

        void Start()
        {
            if (MapGlobalData.savedMapTexture != null)
            {
                territoryTex = new Texture2D(
                    MapGlobalData.savedMapTexture.width,
                    MapGlobalData.savedMapTexture.height,
                    TextureFormat.RGBA32, false);

                territoryTex.filterMode = FilterMode.Bilinear;
                territoryTex.wrapMode = TextureWrapMode.Clamp;
                ClearTerritory();
                territoryTex.Apply();
                territoryMask.texture = territoryTex;
            }
        }

        void Update()
        {
            if (!NationSettingPanel.isNationCreated || territoryTex == null)
                return;

            int count = cityManager.GetCityCount();

            if (count > 0 && count != lastCityCount)
            {
                RedrawAll();
                lastCityCount = count;
            }
            else if (count == 0 && lastCityCount != 0)
            {
                ClearTerritory();
                territoryTex.Apply();
                lastCityCount = 0;
            }
        }

        void RedrawAll()
        {
            ClearTerritory();
            var list = cityManager.GetCityDataList();

            foreach (var data in list)
            {
                if (data == null || data.go == null) continue;
                Draw(data);
            }

            territoryTex.Apply();
        }
        
        void Draw(CityData data)
        {
            // 新增：空值校验（防止核心变量为空）
            if (data == null || cityTypeConfig == null) return;
    
            Vector2 lp = data.localPos;
            Vector2 tp = UIToTex(lp);

            int cx = Mathf.RoundToInt(tp.x);
            int cy = Mathf.RoundToInt(tp.y);
            float mult = 1f;

            // 优化：改用 cityManager 的 GetTerritoryMultByType 方法（复用已有逻辑，减少重复）
            mult = cityManager.GetTerritoryMultByType(data.type);

            // 兜底：如果获取到的倍数异常，使用默认值
            if (mult <= 0) mult = 1f;

            int r = Mathf.RoundToInt(baseTerritoryRadius * mult);
            Color col = nationPanel.currentColor;
            col.a = territoryAlpha;

            for (int dx = -r; dx <= r; dx++)
            for (int dy = -r; dy <= r; dy++)
            {
                if (dx * dx + dy * dy > r * r) continue;
                int x = cx + dx;
                int y = cy + dy;
                if (x < 0 || x >= territoryTex.width || y < 0 || y >= territoryTex.height) continue;
                if (territoryTex.GetPixel(x, y).a < 0.01f)
                    territoryTex.SetPixel(x, y, col);
            }
        }

        // ✅ 最终修复：领土坐标完全正确，不再Y轴翻转
        Vector2 UIToTex(Vector2 local)
        {
            Rect r = mapRoot.rect;
            float x = (local.x + r.width / 2f) / r.width;
            float y = (local.y + r.height / 2f) / r.height;
            return new Vector2(
                x * territoryTex.width,
                y * territoryTex.height
            );
        }

        void ClearTerritory()
        {
            var clear = new Color[territoryTex.width * territoryTex.height];
            for (int i = 0; i < clear.Length; i++)
                clear[i] = Color.clear;
            territoryTex.SetPixels(clear);
        }
    }
}