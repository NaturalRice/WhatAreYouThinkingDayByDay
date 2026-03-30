using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NationCityManager : MonoBehaviour
{
    [Header("城池图标")]
    public Sprite spriteTown;
    public Sprite spriteNormal;
    public Sprite spriteCapital;
    public Sprite spriteFarm;
    public Sprite spriteMarket;
    public Sprite spritePort;

    [Header("城池尺寸")]
    public float sizeTown = 14f;
    public float sizeNormal = 20f;
    public float sizeCapital = 32f;
    public float sizeFarm = 16f;
    public float sizeMarket = 18f;
    public float sizePort = 22f;

    [Header("领土倍数")]
    public float territoryMultTown = 0.6f;
    public float territoryMultNormal = 1.0f;
    public float territoryMultCapital = 1.6f;
    public float territoryMultFarm = 0.7f;
    public float territoryMultMarket = 0.9f;
    public float territoryMultPort = 1.2f;

    [Header("冷却")]
    public float createCD = 2f;
    public Text cdTipText;

    [Header("地图")]
    public RectTransform mapRoot;

    [Header("城池选择面板")]
    public GameObject panelCitySelect;
    public Button btnTown;
    public Button btnNormal;
    public Button btnCapital;
    public Button btnFarm;
    public Button btnMarket;
    public Button btnPort;
    public Text textCapital;
    public float panelOffsetY = 80f;

    public enum CityType
    {
        Town,
        Normal,
        Capital,
        Farm,
        Market,
        Port
    }

    private CityType buildType;
    // 移除：private Color nationColor; （如果仅用于城市颜色，可删除）
    private bool canBuildCity;
    private float cdTimer;
    private bool readyToBuild = true;
    private float lastClickTime;
    private float doubleClickTime = 0.25f;

    private List<CityData> cityList = new List<CityData>();
    private CityData currentCapital;
    private Vector2 buildUIPos;

    [System.Serializable]
    public class CityData
    {
        public GameObject go;
        public CityType type;
        public Vector2 localPos;
        public RectTransform rt;
    }

    void Start()
    {
        if (cdTipText != null) cdTipText.gameObject.SetActive(false);
        panelCitySelect.SetActive(false);
        cityList.Clear();
        currentCapital = null;

        btnTown.onClick.AddListener(() => SelectType(CityType.Town));
        btnNormal.onClick.AddListener(() => SelectType(CityType.Normal));
        btnCapital.onClick.AddListener(() => SelectType(CityType.Capital));
        btnFarm.onClick.AddListener(() => SelectType(CityType.Farm));
        btnMarket.onClick.AddListener(() => SelectType(CityType.Market));
        btnPort.onClick.AddListener(() => SelectType(CityType.Port));
    }

    void Update()
    {
        HandleCooldown();
        HandleDoubleClick();
        HandleHotkey();
        UpdateButtonAnim();
    }

    void HandleCooldown()
    {
        if (!readyToBuild)
        {
            cdTimer -= Time.deltaTime;
            if (cdTimer <= 0) readyToBuild = true;
        }
    }

    void HandleDoubleClick()
    {
        if (canBuildCity && Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastClickTime < doubleClickTime)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    mapRoot, Input.mousePosition, null, out buildUIPos);
                ShowSelectPanel();
            }
            lastClickTime = Time.time;
        }
    }

    void ShowSelectPanel()
    {
        panelCitySelect.SetActive(true);
        RectTransform rt = panelCitySelect.GetComponent<RectTransform>();
        rt.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y + panelOffsetY);
        textCapital.text = currentCapital == null ? "都城" : "迁都";
    }

    void HandleHotkey()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectType(CityType.Town);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectType(CityType.Normal);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectType(CityType.Capital);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectType(CityType.Farm);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectType(CityType.Market);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SelectType(CityType.Port);
    }

    void SelectType(CityType type)
    {
        if (!readyToBuild) return;

        // ✅ 统一规则：所有城市 只能在陆地建造
        if (!IsLand(buildUIPos))
        {
            Debug.Log("只能在陆地上建造");
            panelCitySelect.SetActive(false);
            return;
        }

        buildType = type;

        if (type == CityType.Capital)
        {
            if (currentCapital == null)
                CreateCity(buildUIPos, CityType.Capital);
            else
                MoveCapital();
        }
        else
        {
            CreateCity(buildUIPos, type);
        }

        panelCitySelect.SetActive(false);
        readyToBuild = false;
        cdTimer = createCD;
    }

    // ✅ 统一：判断是否为陆地（所有城市共用）
    bool IsLand(Vector2 localPos)
    {
        Color c = GetMapPixel(localPos);
        float tol = 0.1f;
        return !(
            Mathf.Abs(c.r - MapGlobalData.seaColor.r) < tol &&
            Mathf.Abs(c.g - MapGlobalData.seaColor.g) < tol &&
            Mathf.Abs(c.b - MapGlobalData.seaColor.b) < tol
        );
    }

    Color GetMapPixel(Vector2 localPos)
    {
        Texture2D tex = MapGlobalData.savedMapTexture;
        if (tex == null) return Color.blue;

        Rect r = mapRoot.rect;
        float xRatio = (localPos.x + r.width / 2f) / r.width;
        float yRatio = (localPos.y + r.height / 2f) / r.height;

        int px = Mathf.RoundToInt(xRatio * tex.width);
        int py = Mathf.RoundToInt(yRatio * tex.height);

        if (px < 0 || px >= tex.width || py < 0 || py >= tex.height)
            return Color.blue;

        return tex.GetPixel(px, py);
    }

    void CreateCity(Vector2 localPos, CityType type)
    {
        // ✅ 安全过滤：防止空坐标/中心脏数据生成
        if (localPos.magnitude < 5f)
        {
            Debug.Log("拒绝生成中心脏数据城池");
            return;
        }

        GameObject city = new GameObject($"City_{type}");
        city.transform.SetParent(mapRoot);
        city.transform.localPosition = localPos;
        city.transform.localScale = Vector3.one;

        Image img = city.AddComponent<Image>();
        // 核心修改：删除国家颜色设置，改为默认白色（显示贴图原始颜色）
        img.color = Color.white; 
        img.raycastTarget = false;

        RectTransform rt = city.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        float size = sizeNormal;
        Sprite sprite = spriteNormal;

        switch (type)
        {
            case CityType.Town: size = sizeTown; sprite = spriteTown; break;
            case CityType.Normal: size = sizeNormal; sprite = spriteNormal; break;
            case CityType.Capital: size = sizeCapital; sprite = spriteCapital; break;
            case CityType.Farm: size = sizeFarm; sprite = spriteFarm; break;
            case CityType.Market: size = sizeMarket; sprite = spriteMarket; break;
            case CityType.Port: size = sizePort; sprite = spritePort; break;
        }

        rt.sizeDelta = new Vector2(size, size);
        img.sprite = sprite;

        CityData data = new CityData
        {
            go = city,
            type = type,
            localPos = localPos,
            rt = rt
        };

        cityList.Add(data);
        if (type == CityType.Capital) currentCapital = data;
    }

    void MoveCapital()
    {
        currentCapital.type = CityType.Normal;
        currentCapital.rt.sizeDelta = new Vector2(sizeNormal, sizeNormal);
        Image oldImg = currentCapital.go.GetComponent<Image>();
        oldImg.sprite = spriteNormal;
        // 可选：确保迁都后旧都城也保持原始贴图颜色
        oldImg.color = Color.white;

        CreateCity(buildUIPos, CityType.Capital);
    }

    void UpdateButtonAnim()
    {
        btnTown.transform.localScale = Input.GetKey(KeyCode.Alpha1) ? new Vector3(1.1f, 1.1f, 1) : Vector3.one;
        btnNormal.transform.localScale = Input.GetKey(KeyCode.Alpha2) ? new Vector3(1.1f, 1.1f, 1) : Vector3.one;
        btnCapital.transform.localScale = Input.GetKey(KeyCode.Alpha3) ? new Vector3(1.1f, 1.1f, 1) : Vector3.one;
        btnFarm.transform.localScale = Input.GetKey(KeyCode.Alpha4) ? new Vector3(1.1f, 1.1f, 1) : Vector3.one;
        btnMarket.transform.localScale = Input.GetKey(KeyCode.Alpha5) ? new Vector3(1.1f, 1.1f, 1) : Vector3.one;
        btnPort.transform.localScale = Input.GetKey(KeyCode.Alpha6) ? new Vector3(1.1f, 1.1f, 1) : Vector3.one;
    }

    public void SetNationData(Color color)
    {
        // 移除：nationColor = color; （如果仅用于城市颜色，可删除）
        canBuildCity = true;
        // ✅ 重置所有数据，彻底杜绝中心幽灵城
        cityList.Clear();
        currentCapital = null;
    }

    public List<CityData> GetCityDataList() => cityList;
    public int GetCityCount() => cityList.Count;
}