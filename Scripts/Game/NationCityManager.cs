using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NationCityManager : MonoBehaviour
{
    [Header("城池设置")]
    public Sprite citySprite;
    public float cityPixelSize = 3f; // 地图像素大小（永远不变）

    [Header("冷却")]
    public float createCD = 2f;
    public Text cdTipText;

    [Header("地图")]
    public RectTransform mapRoot;

    private Color nationColor;
    private bool canBuildCity;
    private float cdTimer;
    private bool readyToBuild = true;
    private float lastClickTime;
    private float doubleClickTime = 0.25f;
    private List<GameObject> cityList = new List<GameObject>();

    void Start()
    {
        if (cdTipText != null) cdTipText.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleCooldown();
        HandleDoubleClickBuild();
    }

    void HandleCooldown()
    {
        if (!readyToBuild)
        {
            cdTimer -= Time.deltaTime;
            if (cdTimer <= 0) readyToBuild = true;
        }
    }

    void HandleDoubleClickBuild()
    {
        if (canBuildCity && Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastClickTime < doubleClickTime)
                TryBuildCity();

            lastClickTime = Time.time;
        }
    }

    public void SetNationData(Color color)
    {
        nationColor = color;
        canBuildCity = true;
    }

    void TryBuildCity()
    {
        if (!readyToBuild) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapRoot, Input.mousePosition, null, out Vector2 localPoint))
            return;

        CreateCity(localPoint);
        readyToBuild = false;
        cdTimer = createCD;
    }

    void CreateCity(Vector2 localPos)
    {
        GameObject city = new GameObject("City");
        city.transform.SetParent(mapRoot);
        city.transform.localPosition = localPos;
        city.transform.localScale = Vector3.one;

        Image img = city.AddComponent<Image>();
        img.sprite = citySprite;
        img.color = nationColor;
        img.raycastTarget = false;

        RectTransform rt = city.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(cityPixelSize, cityPixelSize);

        cityList.Add(city);
    }

    public List<GameObject> GetCityList() => cityList;
    public int GetCityCount() => cityList.Count;
}