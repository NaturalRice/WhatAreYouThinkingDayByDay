using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Core.Base;
using Game.Core.GameEntry;
using Game.Game.Terrain; 
using Game.Map;
using Game.UI.Common;

namespace Game.Game.Nation
{
    public class NationCityManager : BaseManager
    {
        [Header("城池图标")] public Sprite spriteTown;
        public Sprite spriteNormal;
        public Sprite spriteCapital;
        public Sprite spriteFarm;
        public Sprite spriteMarket;
        public Sprite spritePort;

        [Header("城池尺寸")] public float sizeTown = 14f;
        public float sizeNormal = 20f;
        public float sizeCapital = 32f;
        public float sizeFarm = 16f;
        public float sizeMarket = 18f;
        public float sizePort = 22f;

        [Header("领土倍数")] public float territoryMultTown = 0.6f;
        public float territoryMultNormal = 1.0f;
        public float territoryMultCapital = 1.6f;
        public float territoryMultFarm = 0.7f;
        public float territoryMultMarket = 0.9f;
        public float territoryMultPort = 1.2f;

        [Header("核心资源基础产出（每结算周期）")] [Tooltip("小镇：均衡基础产出")]
        public Vector4 resTown = new Vector4(2, 1, 3, 0);

        [Tooltip("普通城：均衡产出")] public Vector4 resNormal = new Vector4(3, 2, 5, 1);
        [Tooltip("都城：金币+军队高")] public Vector4 resCapital = new Vector4(5, 8, 10, 5);
        [Tooltip("农村：粮食超高")] public Vector4 resFarm = new Vector4(10, 1, 4, 0);
        [Tooltip("市集：金币高")] public Vector4 resMarket = new Vector4(2, 7, 6, 0);
        [Tooltip("港口：金币+粮食均衡")] public Vector4 resPort = new Vector4(4, 5, 3, 1);

        [Header("扩展资源基础产出（每结算周期）")] [Tooltip("小镇：少量基础资源")]
        public Vector12 resExtendTown = new Vector12(1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0);

        [Tooltip("普通城：均衡扩展资源")] public Vector12 resExtendNormal = new Vector12(2, 2, 1, 0, 1, 1, 2, 0, 0, 1, 0, 1);
        [Tooltip("都城：少量稀有资源")] public Vector12 resExtendCapital = new Vector12(3, 3, 2, 1, 2, 2, 3, 1, 0, 1, 1, 2);
        [Tooltip("农村：牲畜+草料+木材高")] public Vector12 resExtendFarm = new Vector12(4, 1, 5, 0, 0, 0, 6, 0, 0, 0, 0, 1);
        [Tooltip("市集：布匹+皮革高")] public Vector12 resExtendMarket = new Vector12(1, 1, 2, 0, 3, 3, 1, 0, 0, 0, 0, 0);
        [Tooltip("港口：石料+盐矿高")] public Vector12 resExtendPort = new Vector12(2, 4, 1, 0, 1, 1, 1, 2, 0, 0, 0, 1);

        [Header("冷却")] public float createCD = 2f;
        public Text cdTipText;

        [Header("地图")] public RectTransform mapRoot;

        [Header("城池选择面板")] public GameObject panelCitySelect;
        public Button btnTown;
        public Button btnNormal;
        public Button btnCapital;
        public Button btnFarm;
        public Button btnMarket;
        public Button btnPort;
        public Text textCapital;
        public float panelOffsetY = 80f;
        
        // 地图纹理（需和绘制地形的纹理一致）
        public Texture2D mapTexture;

        private CityType buildType;
        private bool canBuildCity;
        private float cdTimer;
        private bool readyToBuild = true;
        private float lastClickTime;
        private float doubleClickTime = 0.25f;

        public List<CityData> cityList = new List<CityData>();
        private CityData currentCapital;
        
        private Vector2 buildUIPos;

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
            
            // 港口特殊判断：必须在沿海/沿河区域
            if (type == CityType.Port)
            {
                Vector2 pixelPos = ConvertLocalPosToPixelPos(buildUIPos);
                // 修复1：正确获取港口可建标记（从元组中解构）
                var (_, _, canBuildPort) = MapTerrainChecker.CheckBasicTerrain(
                    MapGlobalData.savedMapTexture, 
                    pixelPos
                );

                // 修复2：判断是否为港口可建区域（取反判断）
                if (!canBuildPort)
                {
                    Debug.Log("港口只能建在海边/河边！");
                    panelCitySelect.SetActive(false);
                    return;
                }
            }

            // 修复3：先判断是否为陆地，再执行创建逻辑（原逻辑顺序错误）
            if (!IsLand(buildUIPos))
            {
                Debug.Log("只能在陆地上建造！");
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
            
            panelCitySelect.SetActive(false); // 原逻辑遗漏：创建后关闭面板
            readyToBuild = false;
            cdTimer = createCD;
        }

        bool IsLand(Vector2 localPos)
        {
            // 转换UI坐标到地图纹理像素坐标（复用原有坐标转换逻辑）
            Vector2 pixelPos = ConvertLocalPosToPixelPos(localPos);
            // 复用MapTerrainChecker的可建造判断
            return MapTerrainChecker.CanBuildCityAtPosition(MapGlobalData.savedMapTexture, pixelPos);
        }
        
        // 可选：补充领土扩张阻挡的判断逻辑（使用IsBlockTerritoryAtPosition）
        public bool CanExpandTerritoryAtPosition(Vector2 localPos)
        {
            Vector2 pixelPos = ConvertLocalPosToPixelPos(localPos);
            // 使用未被调用的IsBlockTerritoryAtPosition方法
            return !MapTerrainChecker.IsBlockTerritoryAtPosition(MapGlobalData.savedMapTexture, pixelPos);
        }
        
        // 2. 新增：UI本地坐标转地图纹理像素坐标（抽离原有GetMapPixel中的坐标转换逻辑）
        public Vector2 ConvertLocalPosToPixelPos(Vector2 localPos)
        {
            Texture2D tex = MapGlobalData.savedMapTexture;
            if (tex == null) return Vector2.zero;
    
            Rect r = mapRoot.rect;
            float xRatio = (localPos.x + r.width / 2f) / r.width;
            float yRatio = (localPos.y + r.height / 2f) / r.height;
    
            int px = Mathf.RoundToInt(xRatio * tex.width);
            int py = Mathf.RoundToInt(yRatio * tex.height);
            return new Vector2(px, py);
        }

        // ==================== 城市点击 ====================
        // 移除静态 currentSelectedCity，改为实例属性
        public CityData currentSelectedCity { get; private set; }
        
        void AddCityClickEvent(GameObject city, CityData data)
        {
            Button btn = city.GetComponent<Button>();
            if (btn == null) btn = city.AddComponent<Button>();

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => 
            {
                currentSelectedCity = data;
                CityInfoPanel.Instance.ShowPanel(data);
                Debug.Log("✅ 城市已点击：" + data.type + " 地形：" + (data.terrainConfig != null ? data.terrainConfig.terrainName : "未设置"));
            });
        }
        
        // ==================== 创建城市时绑定点击 ====================
        
        // 城市选中事件
        void UpdateSelectedCity(CityData data)
        {
            currentSelectedCity = data;
            // 触发全局事件，通知所有订阅者（UI面板）刷新
            EventManager.OnCitySelected?.Invoke();
            Debug.Log($"[CityManager] 选中城市：{data.type}，地形：{data.terrainConfig?.terrainName}");
        }
        
        // 3. 重构CreateCity方法：新增地形加成计算
        void CreateCity(Vector2 localPos, CityType type)
        {
            if (localPos.magnitude < 5f)
            {
                Debug.Log("拒绝生成中心脏数据城池");
                return;
            }
            
            // 🔥 修复：先获取地形
            TerrainConfig terrain = MapTerrainChecker.GetDetailedTerrain(
                MapGlobalData.savedMapTexture, 
                ConvertLocalPosToPixelPos(localPos)
            );

            // 🔥 修复：地形不允许建城 → 直接阻止
            if (terrain != null && !terrain.canBuildCity)
            {
                Debug.Log($"❌ 无法在【{terrain.terrainName}】上建造城市");
                return;
            }

            GameObject city = new GameObject($"City_{type}");
            city.transform.SetParent(mapRoot);
            city.transform.localPosition = localPos;
            city.transform.localScale = Vector3.one;

            Image img = city.AddComponent<Image>();
            img.color = Color.white;
            img.raycastTarget = true;

            RectTransform rt = city.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(30, 30);

            float size = sizeNormal;
            Sprite sprite = spriteNormal;

            // 核心资源产出赋值（基础值）
            int f = 0, g = 0, p = 0, a = 0;
            // 扩展资源产出赋值（基础值）
            int w = 0, s = 0, l = 0, h = 0, c = 0, le = 0, fo = 0, sa = 0, i = 0, co = 0, go = 0, cl = 0;

            switch (type)
            {
                case CityType.Town: size = sizeTown; sprite = spriteTown; f = (int)resTown.x; g = (int)resTown.y; p = (int)resTown.z; a = (int)resTown.w; w = resExtendTown.wood; s = resExtendTown.stone; l = resExtendTown.livestock; h = resExtendTown.horse; c = resExtendTown.cloth; le = resExtendTown.leather; fo = resExtendTown.forage; sa = resExtendTown.salt; i = resExtendTown.iron; co = resExtendTown.copper; go = resExtendTown.goldOre; cl = resExtendTown.clay; break;
                case CityType.Normal: size = sizeNormal; sprite = spriteNormal; f = (int)resNormal.x; g = (int)resNormal.y; p = (int)resNormal.z; a = (int)resNormal.w; w = resExtendNormal.wood; s = resExtendNormal.stone; l = resExtendNormal.livestock; h = resExtendNormal.horse; c = resExtendNormal.cloth; le = resExtendNormal.leather; fo = resExtendNormal.forage; sa = resExtendNormal.salt; i = resExtendNormal.iron; co = resExtendNormal.copper; go = resExtendNormal.goldOre; cl = resExtendNormal.clay; break;
                case CityType.Capital: size = sizeCapital; sprite = spriteCapital; f = (int)resCapital.x; g = (int)resCapital.y; p = (int)resCapital.z; a = (int)resCapital.w; w = resExtendCapital.wood; s = resExtendCapital.stone; l = resExtendCapital.livestock; h = resExtendCapital.horse; c = resExtendCapital.cloth; le = resExtendCapital.leather; fo = resExtendCapital.forage; sa = resExtendCapital.salt; i = resExtendCapital.iron; co = resExtendCapital.copper; go = resExtendCapital.goldOre; cl = resExtendCapital.clay; break;
                case CityType.Farm: size = sizeFarm; sprite = spriteFarm; f = (int)resFarm.x; g = (int)resFarm.y; p = (int)resFarm.z; a = (int)resFarm.w; w = resExtendFarm.wood; s = resExtendFarm.stone; l = resExtendFarm.livestock; h = resExtendFarm.horse; c = resExtendFarm.cloth; le = resExtendFarm.leather; fo = resExtendFarm.forage; sa = resExtendFarm.salt; i = resExtendFarm.iron; co = resExtendFarm.copper; go = resExtendFarm.goldOre; cl = resExtendFarm.clay; break;
                case CityType.Market: size = sizeMarket; sprite = spriteMarket; f = (int)resMarket.x; g = (int)resMarket.y; p = (int)resMarket.z; a = (int)resMarket.w; w = resExtendMarket.wood; s = resExtendMarket.stone; l = resExtendMarket.livestock; h = resExtendMarket.horse; c = resExtendMarket.cloth; le = resExtendMarket.leather; fo = resExtendMarket.forage; sa = resExtendMarket.salt; i = resExtendMarket.iron; co = resExtendMarket.copper; go = resExtendMarket.goldOre; cl = resExtendMarket.clay; break;
                case CityType.Port: size = sizePort; sprite = spritePort; f = (int)resPort.x; g = (int)resPort.y; p = (int)resPort.z; a = (int)resPort.w; w = resExtendPort.wood; s = resExtendPort.stone; l = resExtendPort.livestock; h = resExtendPort.horse; c = resExtendPort.cloth; le = resExtendPort.leather; fo = resExtendPort.forage; sa = resExtendPort.salt; i = resExtendPort.iron; co = resExtendPort.copper; go = resExtendPort.goldOre; cl = resExtendPort.clay; break;
            }

            // ========== 新增：应用地形资源加成 ==========
            // 核心资源加成
            f = CalculateBonus(f, terrain.foodBonus);
            g = CalculateBonus(g, terrain.goldBonus);
            p = CalculateBonus(p, terrain.goldBonus); // 人口暂用金币加成（可自定义）
            a = CalculateBonus(a, terrain.goldBonus); // 军队暂用金币加成（可自定义）
            
            // 扩展资源加成
            w = CalculateBonus(w, terrain.woodBonus);
            s = CalculateBonus(s, terrain.stoneBonus);
            l = CalculateBonus(l, terrain.livestockBonus);
            h = CalculateBonus(h, terrain.horseBonus);
            c = CalculateBonus(c, terrain.clothBonus);
            le = CalculateBonus(le, terrain.leatherBonus);
            fo = CalculateBonus(fo, terrain.forageBonus);
            sa = CalculateBonus(sa, terrain.saltBonus);
            i = CalculateBonus(i, terrain.ironBonus);
            co = CalculateBonus(co, terrain.copperBonus);
            go = CalculateBonus(go, terrain.goldOreBonus);
            cl = CalculateBonus(cl, terrain.clayBonus);

            rt.sizeDelta = new Vector2(size, size);
            img.sprite = sprite;

            // CityData赋值（原有逻辑不变，只是值已包含地形加成）
            CityData data = new CityData();
            data.go = city;
            data.type = type;
            data.localPos = localPos;
            data.rt = rt;
            // 🔥 关键修复：赋值地形配置
            data.terrainConfig = terrain; 
            
            // 核心资源（已加成）
            data.foodOut = f;
            data.goldOut = g;
            data.peopleOut = p;
            data.armyOut = a;
            // 扩展资源（已加成）
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
            
            // 🔥 新增：给城市添加点击事件
            AddCityClickEvent(city, data);
        }
        
        // 4. 新增：计算加成的工具方法
        private int CalculateBonus(int baseValue, int bonusPercent)
        {
            if (bonusPercent == 0) return baseValue;
            // 计算百分比加成：基础值 + 基础值 * 加成百分比 / 100
            float bonusValue = baseValue * (1 + bonusPercent / 100f);
            return Mathf.Max(0, Mathf.RoundToInt(bonusValue)); // 确保结果非负
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
}