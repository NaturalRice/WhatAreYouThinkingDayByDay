using UnityEngine;

namespace Game.Map
{
    public static class MapGlobalData
    {
        // 地图纹理（绘制好的图）
        public static Texture2D savedMapTexture;

        // 陆地颜色 / 海洋颜色（从画板同步过来）
        public static Color landColor;
        public static Color seaColor;
    }
}