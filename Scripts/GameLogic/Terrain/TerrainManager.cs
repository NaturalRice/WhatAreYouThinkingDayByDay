using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TerrainManager : MonoBehaviour
{
    // 单例实例，全局唯一
    public static TerrainManager Instance { get; private set; }

    // 所有地形配置（自动加载，无需手动赋值）
    private Dictionary<TerrainType, TerrainConfig> terrainConfigDict = new Dictionary<TerrainType, TerrainConfig>();
    private Dictionary<Color, TerrainConfig> colorToTerrainDict = new Dictionary<Color, TerrainConfig>();

    // 颜色匹配容错值（可在Inspector调整）
    [SerializeField] private float colorTolerance = 0.08f;

    private void Awake()
    {
        // 单例初始化，确保全局唯一
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 切换场景不销毁
        DontDestroyOnLoad(gameObject);

        // 自动从Resources加载所有TerrainConfig
        LoadAllTerrainConfigs();
    }

    // 加载所有地形配置
    private void LoadAllTerrainConfigs()
    {
        // 加载Resources/TerrainConfigs下的所有TerrainConfig
        TerrainConfig[] allConfigs = Resources.LoadAll<TerrainConfig>("TerrainConfigs");
        
        if (allConfigs.Length == 0)
        {
            Debug.LogError("❌ 未在Resources/TerrainConfigs中找到任何TerrainConfig配置文件！");
            return;
        }

        // 构建字典
        terrainConfigDict.Clear();
        colorToTerrainDict.Clear();
        foreach (var config in allConfigs)
        {
            if (config == null) continue;
            
            // 按地形类型存储
            if (!terrainConfigDict.ContainsKey(config.terrainType))
            {
                terrainConfigDict.Add(config.terrainType, config);
            }
            else
            {
                Debug.LogWarning($"⚠️ 地形类型 {config.terrainType} 存在重复配置，已跳过");
            }

            // 按颜色存储（用于地图像素匹配）
            Color32 packedColor = config.terrainColor;
            if (!colorToTerrainDict.ContainsKey(packedColor))
            {
                colorToTerrainDict.Add(packedColor, config);
            }
        }

        Debug.Log($"✅ 成功加载 {allConfigs.Length} 个地形配置");
    }

    // 对外提供获取配置的方法
    public TerrainConfig GetTerrainConfig(TerrainType type)
    {
        if (terrainConfigDict.TryGetValue(type, out var config))
            return config;
        
        Debug.LogError($"❌ 未找到地形类型 {type} 的配置");
        return null;
    }

    // 按像素颜色获取地形配置（核心方法，用于地图绘制/识别）
    public TerrainConfig GetTerrainAtColor(Color pixelColor)
    {
        // 先精确匹配
        Color32 packedPixel = pixelColor;
        if (colorToTerrainDict.TryGetValue(packedPixel, out var exactConfig))
            return exactConfig;

        // 精确匹配失败，用容错值模糊匹配
        foreach (var kvp in colorToTerrainDict)
        {
            if (ColorSimilarity(pixelColor, kvp.Key) <= colorTolerance)
            {
                return kvp.Value;
            }
        }

        // 无匹配，返回默认海洋
        return GetTerrainConfig(TerrainType.Ocean);
    }

    // 颜色相似度计算（0为完全相同，越大差异越大）
    private float ColorSimilarity(Color a, Color b)
    {
        return Mathf.Sqrt(
            Mathf.Pow(a.r - b.r, 2) +
            Mathf.Pow(a.g - b.g, 2) +
            Mathf.Pow(a.b - b.b, 2)
        );
    }

    // 供MapDraw场景获取画笔颜色
    public Color GetTerrainBrushColor(TerrainType type)
    {
        var config = GetTerrainConfig(type);
        return config != null ? config.terrainColor : Color.white;
    }
}