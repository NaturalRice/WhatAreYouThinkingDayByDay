using System.Collections.Generic;
using UnityEngine;

namespace Game.Game.Terrain
{
    public class TerrainManager : MonoBehaviour
    {
        public static TerrainManager Instance;

        [Header("颜色匹配容错")]
        public float colorTolerance = 0.15f;

        private Dictionary<TerrainType, TerrainConfig> terrainConfigDict = new Dictionary<TerrainType, TerrainConfig>();
        private Dictionary<Color, TerrainConfig> colorToTerrainDict = new Dictionary<Color, TerrainConfig>();

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
                Debug.LogError("❌ 未在 Resources/TerrainConfigs 目录下找到任何地形配置文件！");
                return;
            }

            terrainConfigDict.Clear();
            colorToTerrainDict.Clear();

            foreach (var cfg in configs)
            {
                if (cfg == null) continue;
                terrainConfigDict[cfg.terrainType] = cfg;
                colorToTerrainDict[cfg.terrainColor] = cfg;
            }

            Debug.Log($"✅ 成功加载 {configs.Length} 个地形配置");
        }

        public TerrainConfig GetTerrainConfig(TerrainType type)
        {
            if (terrainConfigDict == null)
            {
                Debug.LogError("❌ 地形配置字典未初始化！");
                return null;
            }

            if (terrainConfigDict.TryGetValue(type, out var config))
                return config;

            Debug.LogError($"❌ 未找到地形类型 {type} 的配置");
            return null;
        }

        public TerrainConfig GetTerrainAtColor(Color pixelColor)
        {
            if (colorToTerrainDict == null || colorToTerrainDict.Count == 0)
                return GetTerrainConfig(TerrainType.Plains);

            foreach (var kvp in colorToTerrainDict)
            {
                if (ColorSimilarity(pixelColor, kvp.Key) <= colorTolerance)
                {
                    return kvp.Value;
                }
            }

            return GetTerrainConfig(TerrainType.Plains);
        }

        private float ColorSimilarity(Color a, Color b)
        {
            float r = Mathf.Abs(a.r - b.r);
            float g = Mathf.Abs(a.g - b.g);
            float bVal = Mathf.Abs(a.b - b.b);
            return (r + g + bVal) / 3f;
        }

        public TerrainConfig GetTerrainAtPosition(Texture2D mapTex, Vector2 mapPos)
        {
            if (mapTex == null)
                return GetTerrainConfig(TerrainType.Plains);

            int x = Mathf.RoundToInt(mapPos.x);
            int y = Mathf.RoundToInt(mapPos.y);

            if (x < 0 || x >= mapTex.width || y < 0 || y >= mapTex.height)
                return GetTerrainConfig(TerrainType.Plains);

            Color color = mapTex.GetPixel(x, y);
            return GetTerrainAtColor(color);
        }
    }
}