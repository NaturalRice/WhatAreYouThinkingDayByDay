using Game.Map;
using UnityEngine;

namespace Game.Game.Terrain
{
    public static class MapTerrainChecker
    {
        // 颜色相似度阈值（和填充系统保持一致，可微调）
        private static float colorSimilarity = 0.1f;

        /// <summary>
        /// 核心判断：指定坐标的基础地形属性（兼容旧逻辑的海洋/陆地判断）
        /// 废弃原MapTerrainType，改用bool+枚举组合
        /// </summary>
        /// <param name="mapTex">地图绘制纹理</param>
        /// <param name="mapPos">地图内的像素坐标</param>
        /// <returns>是否为海洋、是否为陆地、是否为港口可建区域</returns>
        public static (bool isOcean, bool isLand, bool canBuildPort) CheckBasicTerrain(Texture2D mapTex, Vector2 mapPos)
        {
            if (mapTex == null) return (false, false, false);

            int x = Mathf.RoundToInt(mapPos.x);
            int y = Mathf.RoundToInt(mapPos.y);
            if (x < 0 || x >= mapTex.width || y < 0 || y >= mapTex.height)
                return (false, false, false);

            TerrainConfig cfg = TerrainManager.Instance.GetTerrainAtPosition(mapTex, mapPos);
            bool isOcean = cfg.terrainType == TerrainType.Ocean;
            bool isLand = !isOcean && cfg.terrainType != TerrainType.River;
            bool canBuildPort = IsNearOceanOrRiver(x, y, mapTex);

            return (isOcean, isLand, canBuildPort);
        }

        /// <summary>
        /// 检测指定坐标是否在海洋/河流周边（港口可建造区域）
        /// 替代原Coastal枚举的逻辑，支持河流+海洋双地形
        /// </summary>
        private static bool IsNearOceanOrRiver(int x, int y, Texture2D mapTex, int range = 2)
        {
            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int cx = x + dx;
                    int cy = y + dy;
                    if (cx < 0 || cx >= mapTex.width || cy < 0 || cy >= mapTex.height) continue;

                    var cfg = TerrainManager.Instance.GetTerrainAtPosition(mapTex, new Vector2(cx, cy));
                    if (cfg.terrainType == TerrainType.Ocean || cfg.terrainType == TerrainType.River)
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取指定坐标的细化地形配置（复用原有逻辑）
        /// </summary>
        public static TerrainConfig GetDetailedTerrain(Texture2D mapTex, Vector2 mapPos)
        {
            return TerrainManager.Instance.GetTerrainAtPosition(mapTex, mapPos);
        }

        /// <summary>
        /// 激活TerrainConfig的建造规则：是否可建造城池
        /// </summary>
        public static bool CanBuildCityAtPosition(Texture2D mapTex, Vector2 mapPos)
        {
            var cfg = GetDetailedTerrain(mapTex, mapPos);
            return cfg != null && cfg.canBuildCity;
        }

        /// <summary>
        /// 激活TerrainConfig的扩张规则：是否阻挡领土扩张
        /// </summary>
        public static bool IsBlockTerritoryAtPosition(Texture2D mapTex, Vector2 mapPos)
        {
            var terrainConfig = GetDetailedTerrain(mapTex, mapPos);
            return terrainConfig != null && terrainConfig.blockTerritory;
        }

        // 兼容旧代码的过渡方法（可选保留，逐步替换）
        [System.Obsolete("请使用CheckBasicTerrain替代")]
        public static bool IsOcean(Texture2D mapTex, Vector2 mapPos)
        {
            var (isOcean, _, _) = CheckBasicTerrain(mapTex, mapPos);
            return isOcean;
        }

        [System.Obsolete("请使用CheckBasicTerrain替代")]
        public static bool IsCoastal(Texture2D mapTex, Vector2 mapPos)
        {
            var (_, _, canBuildPort) = CheckBasicTerrain(mapTex, mapPos);
            return canBuildPort;
        }
    }
}