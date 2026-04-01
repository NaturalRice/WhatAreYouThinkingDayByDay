using UnityEngine;

namespace Game.Map
{
    public static class MapTextureUtility
    {
        /// <summary>
        /// 缩放并翻转纹理（适配画布坐标系）
        /// </summary>
        public static Texture2D ResizeAndFlipTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            if (source == null)
            {
                Debug.LogError("[MapTextureUtility] 源纹理为空");
                return null;
            }

            RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
            rt.filterMode = FilterMode.Bilinear;
            RenderTexture.active = rt;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, targetWidth, targetHeight, 0);
            Graphics.Blit(source, rt);
            GL.PopMatrix();

            Texture2D finalTex = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            finalTex.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            finalTex.Apply();

            RenderTexture.active = null;
            Object.Destroy(rt);

            return finalTex;
        }

        /// <summary>
        /// 颜色相似度判断
        /// </summary>
        public static bool IsColorSimilar(Color a, Color b, float threshold)
        {
            float rDiff = Mathf.Abs(a.r - b.r);
            float gDiff = Mathf.Abs(a.g - b.g);
            float bDiff = Mathf.Abs(a.b - b.b);
            float avgDiff = (rDiff + gDiff + bDiff) / 3f;
            return avgDiff <= threshold;
        }

        /// <summary>
        /// 创建空白纹理
        /// </summary>
        public static Texture2D CreateBlankTexture(int width, int height, Color fillColor = default)
        {
            if (width <= 0 || height <= 0)
            {
                Debug.LogError("[MapTextureUtility] 纹理尺寸无效");
                return null;
            }

            fillColor = fillColor == default ? Color.white : fillColor;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = fillColor;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}