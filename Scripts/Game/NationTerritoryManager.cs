using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NationTerritoryManager : MonoBehaviour
{
    [Header("核心")]
    public RectTransform mapRoot;
    public NationCityManager cityManager;
    public NationSettingPanel nationPanel;
    public RawImage territoryMask;

    [Header("领土")]
    public float territoryPixelRadius = 5f; // 地图像素半径（永远不变）
    [Range(0.1f, 0.5f)] public float territoryAlpha = 0.3f;

    private Texture2D territoryTex;
    private int lastCityCount = -1;

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
            territoryMask.texture = territoryTex;
        }
    }

    void Update()
    {
        if (!nationPanel.isNationCreated) return;

        // 只在城池数量变化时重绘 → 缩放不重绘 → 零卡顿
        if (cityManager.GetCityCount() != lastCityCount)
        {
            RedrawAllTerritory();
            lastCityCount = cityManager.GetCityCount();
        }
    }

    void RedrawAllTerritory()
    {
        if (territoryTex == null) return;
        ClearTerritory();

        var cities = cityManager.GetCityList();
        foreach (var city in cities)
        {
            if (city == null) continue;
            DrawCircle(city);
        }

        territoryTex.Apply();
    }

    void DrawCircle(GameObject city)
    {
        Vector2 local = city.GetComponent<RectTransform>().localPosition;
        Vector2 texPos = UIToTex(local);

        int cx = Mathf.RoundToInt(texPos.x);
        int cy = Mathf.RoundToInt(texPos.y);
        int r = Mathf.RoundToInt(territoryPixelRadius);

        Color color = nationPanel.currentColor;
        color.a = territoryAlpha;

        for (int dx = -r; dx <= r; dx++)
        for (int dy = -r; dy <= r; dy++)
        {
            if (dx * dx + dy * dy > r * r) continue;

            int x = cx + dx;
            int y = cy + dy;

            if (x < 0 || x >= territoryTex.width || y < 0 || y >= territoryTex.height)
                continue;

            if (territoryTex.GetPixel(x, y).a < 0.01f)
                territoryTex.SetPixel(x, y, color);
        }
    }

    Vector2 UIToTex(Vector2 localPos)
    {
        Rect r = mapRoot.rect;
        float x = (localPos.x + r.width / 2) / r.width;
        float y = (localPos.y + r.height / 2) / r.height;
        return new Vector2(x * territoryTex.width, y * territoryTex.height);
    }

    void ClearTerritory()
    {
        Color[] clear = new Color[territoryTex.width * territoryTex.height];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = Color.clear;

        territoryTex.SetPixels(clear);
    }
}