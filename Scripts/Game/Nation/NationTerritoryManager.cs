using UnityEngine;
using UnityEngine.UI;
using Game.Core.Base;
using Game.Map;
using static Game.Game.Nation.NationCityManager;

namespace Game.Game.Nation
{
    public class NationTerritoryManager : BaseManager
    {
        [Header("核心")]
        public RectTransform mapRoot;
        public CityTypeConfig cityTypeConfig;
        public RawImage globalTerritoryMask;

        [Header("领土显示")]
        public float baseTerritoryRadius = 50f;
        [Range(0.1f, 0.5f)] public float territoryAlpha = 0.3f;

        private Texture2D _globalTerritoryTex;
        private int _lastTotalCities = -1;

        void Start()
        {
            if (MapGlobalData.savedMapTexture != null)
            {
                _globalTerritoryTex = new Texture2D(
                    MapGlobalData.savedMapTexture.width,
                    MapGlobalData.savedMapTexture.height,
                    TextureFormat.RGBA32, false);
                _globalTerritoryTex.filterMode = FilterMode.Bilinear;
                _globalTerritoryTex.wrapMode = TextureWrapMode.Clamp;
                ClearAllTerritory();
                _globalTerritoryTex.Apply();
                globalTerritoryMask.texture = _globalTerritoryTex;
            }
        }

        void Update()
        {
            if (_globalTerritoryTex == null) return;

            // 🔥 修复卡顿：只有城市数量变化才重绘
            int currentTotal = GetAllCityCount();
            if (currentTotal != _lastTotalCities)
            {
                RedrawAllNations();
                _lastTotalCities = currentTotal;
            }
        }

        // 🔥 获得所有城市数量
        int GetAllCityCount()
        {
            int count = 0;
            NationCityManager[] all = FindObjectsOfType<NationCityManager>();
            foreach (var n in all) count += n.GetCityCount();
            return count;
        }

        // 统一绘制所有国家
        void RedrawAllNations()
        {
            ClearAllTerritory();

            NationCityManager[] allNations = FindObjectsOfType<NationCityManager>();
            foreach (var nation in allNations)
            {
                if (nation == null) continue;

                // 🔥 修复空引用：安全获取玩家颜色
                Color col = nation.isPlayer
                    ? (NationSettingPanel.Instance != null ? NationSettingPanel.Instance.currentColor : Color.red)
                    : nation.nationColor;

                DrawOneNation(nation, col);
            }

            _globalTerritoryTex.Apply();
            globalTerritoryMask.texture = _globalTerritoryTex;
        }

        // 绘制单个国家
        void DrawOneNation(NationCityManager nation, Color color)
        {
            if (nation == null || nation.GetCityCount() == 0) return;

            var cities = nation.GetCityDataList();
            color.a = territoryAlpha;

            foreach (var city in cities)
            {
                if (city == null || city.go == null) continue;

                Vector2 lp = city.localPos;
                Vector2 tp = UIToTex(lp);
                int cx = Mathf.RoundToInt(tp.x);
                int cy = Mathf.RoundToInt(tp.y);

                float mult = nation.GetTerritoryMultByType(city.type);
                float radiusMult = cityTypeConfig.GetRadiusMult(city.type);
                int r = Mathf.RoundToInt(baseTerritoryRadius * mult * radiusMult);

                for (int dx = -r; dx <= r; dx++)
                for (int dy = -r; dy <= r; dy++)
                {
                    if (dx * dx + dy * dy > r * r) continue;
                    int x = cx + dx;
                    int y = cy + dy;
                    if (x < 0 || x >= _globalTerritoryTex.width || y < 0 || y >= _globalTerritoryTex.height) continue;
                    _globalTerritoryTex.SetPixel(x, y, color);
                }
            }
        }

        Vector2 UIToTex(Vector2 local)
        {
            Rect r = mapRoot.rect;
            float x = (local.x + r.width / 2f) / r.width;
            float y = (local.y + r.height / 2f) / r.height;
            return new Vector2(x * _globalTerritoryTex.width, y * _globalTerritoryTex.height);
        }

        void ClearAllTerritory()
        {
            if (_globalTerritoryTex == null) return;
            var clear = new Color[_globalTerritoryTex.width * _globalTerritoryTex.height];
            for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
            _globalTerritoryTex.SetPixels(clear);
        }
    }
}