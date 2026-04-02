using System.Collections.Generic;
using UnityEngine;

namespace Game.Game.Terrain
{
    public class TerrainManager : MonoBehaviour
    {
        public static TerrainManager Instance;

        [Header("颜色匹配容错（0.1~0.2最佳）")]
        public float colorTolerance = 0.15f;

        private Dictionary<TerrainType, TerrainConfig> terrainConfigDict = new Dictionary<TerrainType, TerrainConfig>();
        private List<TerrainConfig> terrainList = new List<TerrainConfig>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            LoadAllTerrainConfigs();
        }

        private void LoadAllTerrainConfigs()
        {
            TerrainConfig[] configs = Resources.LoadAll<TerrainConfig>("TerrainConfigs");

            if (configs == null || configs.Length == 0)
            {
                Debug.LogError("❌ 未找到地形配置！");
                return;
            }

            terrainConfigDict.Clear();
            terrainList.Clear();

            foreach (var cfg in configs)
            {
                if (cfg == null) continue;
                terrainConfigDict[cfg.terrainType] = cfg;
                terrainList.Add(cfg);
            }

            Debug.Log($"✅ 成功加载 {terrainList.Count} 个地形");
        }

        public TerrainConfig GetTerrainConfig(TerrainType type)
        {
            if (terrainConfigDict.TryGetValue(type, out var config))
                return config;

            Debug.LogError($"❌ 未找到地形：{type}");
            return GetDefaultConfig();
        }

        private TerrainConfig GetDefaultConfig()
        {
            return terrainConfigDict[TerrainType.Plains];
        }

        // 🔥 修复：颜色匹配逻辑（正确版！）
        public TerrainConfig GetTerrainAtColor(Color pixelColor)
        {
            if (terrainList.Count == 0) return GetDefaultConfig();

            TerrainConfig bestMatch = null;
            float minDifference = float.MaxValue;

            foreach (var cfg in terrainList)
            {
                float diff = ColorDifference(pixelColor, cfg.terrainColor);
                if (diff < minDifference && diff <= colorTolerance)
                {
                    minDifference = diff;
                    bestMatch = cfg;
                }
            }

            return bestMatch ?? GetDefaultConfig();
        }

        // 🔥 修复：计算两个颜色的差异值（越小越接近）
        private float ColorDifference(Color a, Color b)
        {
            float r = Mathf.Abs(a.r - b.r);
            float g = Mathf.Abs(a.g - b.g);
            float bv = Mathf.Abs(a.b - b.b);
            return (r + g + bv) / 3f;
        }

        public TerrainConfig GetTerrainAtPosition(Texture2D mapTex, Vector2 mapPos)
        {
            if (mapTex == null) return GetDefaultConfig();

            int x = Mathf.RoundToInt(mapPos.x);
            int y = Mathf.RoundToInt(mapPos.y);

            if (x < 0 || x >= mapTex.width || y < 0 || y >= mapTex.height)
                return GetDefaultConfig();

            Color color = mapTex.GetPixel(x, y);
            return GetTerrainAtColor(color);
        }
    }
}