using Game.Map;
using UnityEngine;

/// <summary>
/// 地图地形识别工具：判断陆地/海洋/沿海，基于绘制纹理的像素颜色
/// </summary>
namespace Game.Game.Terrain
{
    public static class MapTerrainChecker
    {
        // 地形类型枚举
        public enum MapTerrainType  
        {
            None,
            Plains,
            Ocean,
            Coastal
        }

        // 颜色相似度阈值（和填充系统保持一致，可微调）
        private static float colorSimilarity = 0.1f;

        /// <summary>
        /// 判断指定地图坐标的地形类型
        /// </summary>
        /// <param name="mapTex">地图绘制纹理</param>
        /// <param name="mapPos">地图内的像素坐标（从MapCanvasTransform.GetMousePosInCanvas获取）</param>
        /// <param name="landColor">陆地填充色</param>
        /// <param name="seaColor">海洋填充色</param>
        public static MapTerrainType CheckTerrain(Texture2D mapTex, Vector2 mapPos, Color landColor, Color seaColor)
        {
            if (mapTex == null) return MapTerrainType.None;

            // 边界校验
            int x = Mathf.RoundToInt(mapPos.x);
            int y = Mathf.RoundToInt(mapPos.y);
            if (x < 0 || x >= mapTex.width || y < 0 || y >= mapTex.height)
                return MapTerrainType.None;

            // 获取当前像素颜色
            Color targetColor = mapTex.GetPixel(x, y);

            // 判断是否为海洋
            if (MapTextureUtility.IsColorSimilar(targetColor, seaColor, colorSimilarity))
                return MapTerrainType.Ocean;

            // 判断是否为纯陆地
            if (MapTextureUtility.IsColorSimilar(targetColor, landColor, colorSimilarity))
            {
                // 检测周围8邻域是否有海洋，有则为沿海
                if (IsCoastal(x, y, mapTex, seaColor))
                    return MapTerrainType.Coastal;
                else
                    return MapTerrainType.Plains;
            }

            // 其他情况（手绘轮廓、过渡区）默认按沿海处理
            return MapTerrainType.Coastal;
        }

        /// <summary>
        /// 检测8邻域是否有海洋像素（判断沿海）
        /// </summary>
        private static bool IsCoastal(int x, int y, Texture2D mapTex, Color seaColor)
        {
            // 8邻域坐标偏移
            int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
            int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };

            for (int i = 0; i < 8; i++)
            {
                int checkX = x + dx[i];
                int checkY = y + dy[i];
                if (checkX < 0 || checkX >= mapTex.width || checkY < 0 || checkY >= mapTex.height)
                    continue;

                if (MapTextureUtility.IsColorSimilar(mapTex.GetPixel(checkX, checkY), seaColor, colorSimilarity))
                    return true;
            }

            return false;
        }
        
        // MapTerrainChecker.cs 新增方法
        /// <summary>
        /// 获取指定坐标的细化地形类型（Ocean/Mountain/Plains等）
        /// </summary>
        public static TerrainConfig GetDetailedTerrain(Texture2D mapTex, Vector2 mapPos)
        {
            if (mapTex == null) return TerrainManager.Instance.GetTerrainConfig(TerrainType.Ocean);
            return TerrainManager.Instance.GetTerrainAtPosition(mapTex, mapPos);
        }

        /// <summary>
        /// 简化判断：是否可建造城池（基于TerrainConfig的canBuildCity）
        /// </summary>
        public static bool CanBuildCityAtPosition(Texture2D mapTex, Vector2 mapPos)
        {
            var terrainConfig = GetDetailedTerrain(mapTex, mapPos);
            return terrainConfig != null && terrainConfig.canBuildCity;
        }
    }
}