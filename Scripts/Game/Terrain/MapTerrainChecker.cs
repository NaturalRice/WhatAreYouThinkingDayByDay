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

            // 边界校验
            int x = Mathf.RoundToInt(mapPos.x);
            int y = Mathf.RoundToInt(mapPos.y);
            if (x < 0 || x >= mapTex.width || y < 0 || y >= mapTex.height)
                return (false, false, false);

            // 获取当前地形配置
            TerrainConfig terrainConfig = GetDetailedTerrain(mapTex, mapPos);
            if (terrainConfig == null) return (false, false, false);

            // 基础地形判断（海洋/陆地）
            bool isOcean = terrainConfig.terrainType == TerrainType.Ocean;
            bool isLand = !isOcean && terrainConfig.terrainType != TerrainType.River; // 河流既非海洋也非陆地

            // 港口可建区域：海洋/河流周边N邻域（可自定义邻域范围）
            bool canBuildPort = IsNearOceanOrRiver(x, y, mapTex, checkRange: 1); // 1格邻域，可调整

            return (isOcean, isLand, canBuildPort);
        }

        /// <summary>
        /// 检测指定坐标是否在海洋/河流周边（港口可建造区域）
        /// 替代原Coastal枚举的逻辑，支持河流+海洋双地形
        /// </summary>
        public static bool IsNearOceanOrRiver(int x, int y, Texture2D mapTex, int checkRange = 1)
        {
            // 遍历指定范围的邻域（默认8邻域，range=1）
            for (int dx = -checkRange; dx <= checkRange; dx++)
            {
                for (int dy = -checkRange; dy <= checkRange; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // 跳过自身

                    int checkX = x + dx;
                    int checkY = y + dy;
                    if (checkX < 0 || checkX >= mapTex.width || checkY < 0 || checkY >= mapTex.height)
                        continue;

                    // 获取邻域地形
                    TerrainConfig neighborTerrain = TerrainManager.Instance.GetTerrainAtPosition(mapTex, new Vector2(checkX, checkY));
                    if (neighborTerrain == null) continue;

                    // 海洋或河流则判定为港口可建区域
                    if (neighborTerrain.terrainType == TerrainType.Ocean || neighborTerrain.terrainType == TerrainType.River)
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
            if (mapTex == null) return TerrainManager.Instance.GetTerrainConfig(TerrainType.Ocean);
            return TerrainManager.Instance.GetTerrainAtPosition(mapTex, mapPos);
        }

        /// <summary>
        /// 激活TerrainConfig的建造规则：是否可建造城池
        /// </summary>
        public static bool CanBuildCityAtPosition(Texture2D mapTex, Vector2 mapPos)
        {
            var terrainConfig = GetDetailedTerrain(mapTex, mapPos);
            // 核心判断：配置中的canBuildCity + 非海洋（双重保障）
            return terrainConfig != null && terrainConfig.canBuildCity && terrainConfig.terrainType != TerrainType.Ocean;
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