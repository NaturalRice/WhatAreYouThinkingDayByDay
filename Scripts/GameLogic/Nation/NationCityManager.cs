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

    [Header("核心资源基础产出（每结算周期）")]
    [Tooltip("小镇：均衡基础产出")] public Vector4 resTown = new Vector4(2, 1, 3, 0);
    [Tooltip("普通城：均衡产出")] public Vector4 resNormal = new Vector4(3, 2, 5, 1);
    [Tooltip("都城：金币+军队高")] public Vector4 resCapital = new Vector4(5, 8, 10, 5);
    [Tooltip("农村：粮食超高")] public Vector4 resFarm = new Vector4(10, 1, 4, 0);
    [Tooltip("市集：金币高")] public Vector4 resMarket = new Vector4(2, 7, 6, 0);
    [Tooltip("港口：金币+粮食均衡")] public Vector4 resPort = new Vector4(4, 5, 3, 1);

    [Header("扩展资源基础产出（每结算周期）")]
    [Tooltip("小镇：少量基础资源")] public Vector12 resExtendTown = new Vector12(1,1,1,0,0,0,1,0,0,0,0,0);
    [Tooltip("普通城：均衡扩展资源")] public Vector12 resExtendNormal = new Vector12(2,2,1,0,1,1,2,0,0,1,0,1);
    [Tooltip("都城：少量稀有资源")] public Vector12 resExtendCapital = new Vector12(3,3,2,1,2,2,3,1,0,1,1,2);
    [Tooltip("农村：牲畜+草料+木材高")] public Vector12 resExtendFarm = new Vector12(4,1,5,0,0,0,6,0,0,0,0,1);
    [Tooltip("市集：布匹+皮革高")] public Vector12 resExtendMarket = new Vector12(1,1,2,0,3,3,1,0,0,0,0,0);
    [Tooltip("港口：石料+盐矿高")] public Vector12 resExtendPort = new Vector12(2,4,1,0,1,1,1,2,0,0,0,1);

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
        // 核心资源产出
        public int foodOut;
        public int goldOut;
        public int peopleOut;
        public int armyOut;
        // 扩展资源产出
        public int woodOut;
        public int stoneOut;
        public int livestockOut;
        public int horseOut;
        public int clothOut;
        public int leatherOut;
        public int forageOut;
        public int saltOut;
        public int ironOut;
        public int copperOut;
        public int goldOreOut;
        public int clayOut;
    }
    
    // 自定义Vector12（存储12种扩展资源）
    [System.Serializable]
    public struct Vector12
    {
        public int wood;
        public int stone;
        public int livestock;
        public int horse;
        public int cloth;
        public int leather;
        public int forage;
        public int salt;
        public int iron;
        public int copper;
        public int goldOre;
        public int clay;

        public Vector12(int w, int s, int l, int h, int c, int le, int f, int sa, int i, int co, int go, int cl)
        {
            wood = w;
            stone = s;
            livestock = l;
            horse = h;
            cloth = c;
            leather = le;
            forage = f;
            salt = sa;
            iron = i;
            copper = co;
            goldOre = go;
            clay = cl;
        }
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
        img.color = Color.white;
        img.raycastTarget = false;

        RectTransform rt = city.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        float size = sizeNormal;
        Sprite sprite = spriteNormal;
        
        // 核心资源产出赋值
        int f = 0, g = 0, p = 0, a = 0;
        // 扩展资源产出赋值
        int w = 0, s = 0, l = 0, h = 0, c = 0, le = 0, fo = 0, sa = 0, i = 0, co = 0, go = 0, cl = 0;

        switch (type)
        {
            case CityType.Town: 
                size = sizeTown; sprite = spriteTown;
                f = (int)resTown.x; g = (int)resTown.y; p = (int)resTown.z; a = (int)resTown.w;
                w = resExtendTown.wood; s = resExtendTown.stone; l = resExtendTown.livestock; h = resExtendTown.horse;
                c = resExtendTown.cloth; le = resExtendTown.leather; fo = resExtendTown.forage; sa = resExtendTown.salt;
                i = resExtendTown.iron; co = resExtendTown.copper; go = resExtendTown.goldOre; cl = resExtendTown.clay;
                break;
            case CityType.Normal: 
                size = sizeNormal; sprite = spriteNormal;
                f = (int)resNormal.x; g = (int)resNormal.y; p = (int)resNormal.z; a = (int)resNormal.w;
                w = resExtendNormal.wood; s = resExtendNormal.stone; l = resExtendNormal.livestock; h = resExtendNormal.horse;
                c = resExtendNormal.cloth; le = resExtendNormal.leather; fo = resExtendNormal.forage; sa = resExtendNormal.salt;
                i = resExtendNormal.iron; co = resExtendNormal.copper; go = resExtendNormal.goldOre; cl = resExtendNormal.clay;
                break;
            case CityType.Capital: 
                size = sizeCapital; sprite = spriteCapital;
                f = (int)resCapital.x; g = (int)resCapital.y; p = (int)resCapital.z; a = (int)resCapital.w;
                w = resExtendCapital.wood; s = resExtendCapital.stone; l = resExtendCapital.livestock; h = resExtendCapital.horse;
                c = resExtendCapital.cloth; le = resExtendCapital.leather; fo = resExtendCapital.forage; sa = resExtendCapital.salt;
                i = resExtendCapital.iron; co = resExtendCapital.copper; go = resExtendCapital.goldOre; cl = resExtendCapital.clay;
                break;
            case CityType.Farm: 
                size = sizeFarm; sprite = spriteFarm;
                f = (int)resFarm.x; g = (int)resFarm.y; p = (int)resFarm.z; a = (int)resFarm.w;
                w = resExtendFarm.wood; s = resExtendFarm.stone; l = resExtendFarm.livestock; h = resExtendFarm.horse;
                c = resExtendFarm.cloth; le = resExtendFarm.leather; fo = resExtendFarm.forage; sa = resExtendFarm.salt;
                i = resExtendFarm.iron; co = resExtendFarm.copper; go = resExtendFarm.goldOre; cl = resExtendFarm.clay;
                break;
            case CityType.Market: 
                size = sizeMarket; sprite = spriteMarket;
                f = (int)resMarket.x; g = (int)resMarket.y; p = (int)resMarket.z; a = (int)resMarket.w;
                w = resExtendMarket.wood; s = resExtendMarket.stone; l = resExtendMarket.livestock; h = resExtendMarket.horse;
                c = resExtendMarket.cloth; le = resExtendMarket.leather; fo = resExtendMarket.forage; sa = resExtendMarket.salt;
                i = resExtendMarket.iron; co = resExtendMarket.copper; go = resExtendMarket.goldOre; cl = resExtendMarket.clay;
                break;
            case CityType.Port: 
                size = sizePort; sprite = spritePort;
                f = (int)resPort.x; g = (int)resPort.y; p = (int)resPort.z; a = (int)resPort.w;
                w = resExtendPort.wood; s = resExtendPort.stone; l = resExtendPort.livestock; h = resExtendPort.horse;
                c = resExtendPort.cloth; le = resExtendPort.leather; fo = resExtendPort.forage; sa = resExtendPort.salt;
                i = resExtendPort.iron; co = resExtendPort.copper; go = resExtendPort.goldOre; cl = resExtendPort.clay;
                break;
        }

        rt.sizeDelta = new Vector2(size, size);
        img.sprite = sprite;

        // 新增：给CityData赋值资源产出，CityData 赋值新增扩展资源 ...
        CityData data = new CityData();// 显式new + 类型
        data.go = city;
        data.type = type;
        data.localPos = localPos;
        data.rt = rt;
        // 核心资源
        data.foodOut = f;
        data.goldOut = g;
        data.peopleOut = p;
        data.armyOut = a;
        // 扩展资源
        data.woodOut = w;
        data.stoneOut = s;
        data.livestockOut = l;
        data.horseOut = h;
        data.clothOut = c;
        data.leatherOut = le;
        data.forageOut = fo;
        data.saltOut = sa;
        data.ironOut = i;
        data.copperOut = co;
        data.goldOreOut = go;
        data.clayOut = cl;

        cityList.Add(data);
        if (type == CityType.Capital) currentCapital = data;
    }

    void MoveCapital()
    {
        // 旧都城改为普通城，同步普通城资源产出
        currentCapital.type = CityType.Normal;
        currentCapital.rt.sizeDelta = new Vector2(sizeNormal, sizeNormal);
        Image oldImg = currentCapital.go.GetComponent<Image>();
        oldImg.sprite = spriteNormal;
        oldImg.color = Color.white;
        // 同步核心资源
        currentCapital.foodOut = (int)resNormal.x;
        currentCapital.goldOut = (int)resNormal.y;
        currentCapital.peopleOut = (int)resNormal.z;
        currentCapital.armyOut = (int)resNormal.w;
        // 同步扩展资源
        currentCapital.woodOut = resExtendNormal.wood;
        currentCapital.stoneOut = resExtendNormal.stone;
        currentCapital.livestockOut = resExtendNormal.livestock;
        currentCapital.horseOut = resExtendNormal.horse;
        currentCapital.clothOut = resExtendNormal.cloth;
        currentCapital.leatherOut = resExtendNormal.leather;
        currentCapital.forageOut = resExtendNormal.forage;
        currentCapital.saltOut = resExtendNormal.salt;
        currentCapital.ironOut = resExtendNormal.iron;
        currentCapital.copperOut = resExtendNormal.copper;
        currentCapital.goldOreOut = resExtendNormal.goldOre;
        currentCapital.clayOut = resExtendNormal.clay;

        CreateCity(buildUIPos, CityType.Capital);
    }

    // 修复：老式结构体，兼容所有Unity版本
    public struct CityResourceOutput
    {
        public int totalFood;
        public int totalGold;
        public int totalPeople;
        public int totalArmy;
    }
    
    // 新增：计算所有城池的总资源产出（给ResManager调用）
    public CityResourceOutput GetAllCityResOut()
    {
        int f = 0;
        int g = 0;
        int p = 0;
        int a = 0;
        foreach (var city in cityList)
        {
            if (city == null || city.go == null) continue;
            f += city.foodOut;
            g += city.goldOut;
            p += city.peopleOut;
            a += city.armyOut;
        }
        
        // 完整兼容写法（确保无简化）
        CityResourceOutput res = new CityResourceOutput();
        res.totalFood = f;
        res.totalGold = g;
        res.totalPeople = p;
        res.totalArmy = a;
        return res;
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

    // 新增：创建国家时初始化资源
    public void InitNationRes()
    {
        if (NationResManager.Instance != null)
        {
            NationResManager.Instance.InitRes();
        }
    }

    public void SetNationData(Color color)
    {
        canBuildCity = true;
        cityList.Clear();
        currentCapital = null;
        InitNationRes(); // 调用资源初始化
    }

    public List<CityData> GetCityDataList() => cityList;
    public int GetCityCount() => cityList.Count;

    // 核心资源总产出（给ResManager调用）
    public CoreResOutput GetCoreResOut()
    {
        CoreResOutput output = new CoreResOutput();
        foreach (var city in cityList)
        {
            if (city == null || city.go == null) continue;
            output.food += city.foodOut;
            output.gold += city.goldOut;
            output.people += city.peopleOut;
            output.army += city.armyOut;
        }
        return output;
    }

    // 扩展资源总产出（给ResManager调用）
    public ExtendResOutput GetExtendResOut()
    {
        ExtendResOutput output = new ExtendResOutput();
        foreach (var city in cityList)
        {
            if (city == null || city.go == null) continue;
            output.wood += city.woodOut;
            output.stone += city.stoneOut;
            output.livestock += city.livestockOut;
            output.horse += city.horseOut;
            output.cloth += city.clothOut;
            output.leather += city.leatherOut;
            output.forage += city.forageOut;
            output.salt += city.saltOut;
            output.iron += city.ironOut;
            output.copper += city.copperOut;
            output.goldOre += city.goldOreOut;
            output.clay += city.clayOut;
        }
        return output;
    }
}