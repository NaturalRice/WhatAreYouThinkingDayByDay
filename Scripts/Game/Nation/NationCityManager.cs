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
        [Header("城市类型配置")]
        public CityTypeConfig cityTypeConfigSO; // 引用ScriptableObject配置文件
        
        [Header("城市预制体")]
        public GameObject prefabTown;       // 小镇预制体
        public GameObject prefabNormal;     // 普通城预制体
        public GameObject prefabCapital;    // 都城预制体
        public GameObject prefabFarm;       // 农村预制体
        public GameObject prefabMarket;     // 市集预制体
        public GameObject prefabPort;       // 港口预制体
        
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
        
        [Header("建城限制")]//人口和军队不消耗，人越多建造越快（后续更新）
        public float minCityDistance = 30f;       // 新城市必须离已有城市这个距离内
        public int costFood = 15;                // 建城消耗粮食
        public int costGold = 10;                // 消耗金币
        public int costWood = 12;                // 消耗木材
        public int costStone = 12;                // 消耗
        public int costLivestock = 0;                // 消耗
        public int costHorse = 0;                // 消耗
        public int costCloth = 0;                // 消耗
        public int costLeather = 0;                // 消耗
        public int costForage = 0;                // 消耗
        public int costSalt = 0;                // 消耗
        public int costIron = 0;                // 消耗
        public int costCopper = 0;                // 消耗
        public int costGoldOre = 0;                // 消耗
        public int costClay = 0;                // 消耗
        
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
        
        // 读取配置的工具方法
        private CityTypeConfig GetConfigByType(CityType type)
        {
            return cityTypeConfigSO; 
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
        public NationResManager resManager { get; set; }

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
        public void CreateCity(Vector2 localPos, CityType type)
        {
            // 1. 校验配置
            if (cityTypeConfigSO == null)
            {
                Debug.LogError("❌ 未配置CityTypeConfig ScriptableObject！");
                return;
            }
            var config = GetConfigByType(type);
            if (config == null)
            {
                Debug.LogError($"❌ 未找到{type}类型的配置！");
                return;
            }
            
            // 2. 建城限制检查（保留原有逻辑）
            // 🔥 限制 1：除了第一个城，必须靠近已有城市
            if (cityList.Count > 0)
            {
                bool nearAnyCity = false;
                foreach (var City in cityList)
                {
                    float dist = Vector2.Distance(localPos, City.localPos);
                    if (dist <= minCityDistance)
                    {
                        nearAnyCity = true;
                        break;
                    }
                }

                if (!nearAnyCity)
                {
                    Debug.LogWarning("❌ 不能建城：必须靠近已有城市！");
                    return;
                }
            }
            // 🔥 限制 2：检查资源足够吗？
            if (resManager == null)
            {
                Debug.LogError("❌ 未绑定NationResManager！");
                return;
            }
            // 检查资源是否足够
            if (!resManager.CheckResEnough(
                    costFood, costGold, costWood, costStone, 
                    costLivestock, costHorse, costCloth, costLeather,
                    costForage, costSalt, costIron, costCopper,
                    costGoldOre, costClay))
            {
                Debug.LogWarning("❌ 资源不足，无法建造城市！");
                return;
            }
            // 扣除资源
            resManager.ConsumeRes(
                costFood, costGold, costWood, costStone,
                costLivestock, costHorse, costCloth, costLeather,
                costForage, costSalt, costIron, costCopper,
                costGoldOre, costClay);
            EventManager.OnResourceUpdated?.Invoke();
            Debug.Log("✅ 扣除建城资源成功！");

            // 🔥 限制 3：中心区域禁止建城
            if (localPos.magnitude < 5f)
            {
                Debug.Log("拒绝生成中心脏数据城池");
                return;
            }
            
            // 3. 获取对应预制体（优先级：单独配置的预制体 > SO中的预制体）
            GameObject cityPrefab = type switch
            {
                CityType.Town => prefabTown != null ? prefabTown : cityTypeConfigSO.prefabTown,
                CityType.Normal => prefabNormal != null ? prefabNormal : cityTypeConfigSO.prefabNormal,
                CityType.Capital => prefabCapital != null ? prefabCapital : cityTypeConfigSO.prefabCapital,
                CityType.Farm => prefabFarm != null ? prefabFarm : cityTypeConfigSO.prefabFarm,
                CityType.Market => prefabMarket != null ? prefabMarket : cityTypeConfigSO.prefabMarket,
                CityType.Port => prefabPort != null ? prefabPort : cityTypeConfigSO.prefabPort,
                _ => cityTypeConfigSO.prefabNormal // 兜底
            };
            if (cityPrefab == null)
            {
                Debug.LogError($"❌ {type}类型的城市预制体未配置！");
                return;
            }
            
            // 4. 实例化预制体（核心步骤）
            GameObject cityObj = Instantiate(cityPrefab, mapRoot);
            cityObj.name = $"City_{type}_{cityList.Count + 1}"; // 规范命名

            // 5. 设置预制体位置（保留原有坐标逻辑）
            RectTransform cityRt = cityObj.GetComponent<RectTransform>();
            if (cityRt == null)
            {
                Debug.LogError($"❌ {cityObj.name}预制体缺少RectTransform组件！");
                Destroy(cityObj);
                return;
            }
            cityRt.localPosition = localPos;
            cityRt.localScale = Vector3.one;

            // 6. 地形校验 & 加成计算
            TerrainConfig terrain = MapTerrainChecker.GetDetailedTerrain(
                MapGlobalData.savedMapTexture, 
                ConvertLocalPosToPixelPos(localPos)
            );
            if (terrain != null && !terrain.canBuildCity)
            {
                Debug.Log($"❌ 无法在【{terrain.terrainName}】上建造城市");
                Destroy(cityObj);
                return;
            }

            // 7. 读取基础资源产出（从SO配置）
            Vector4 coreResBase = GetCoreResBaseByType(type);
            Vector12 extendResBase = GetExtendResBaseByType(type);

            // 8. 应用地形加成
            int foodOut = CalculateBonus((int)coreResBase.x, terrain.foodBonus);
            int goldOut = CalculateBonus((int)coreResBase.y, terrain.goldBonus);
            int peopleOut = CalculateBonus((int)coreResBase.z, terrain.peopleBonus);
            int armyOut = CalculateBonus((int)coreResBase.w, terrain.armyBonus);

            int woodOut = CalculateBonus(extendResBase.wood, terrain.woodBonus);
            int stoneOut = CalculateBonus(extendResBase.stone, terrain.stoneBonus);
            int livestockOut = CalculateBonus(extendResBase.livestock, terrain.livestockBonus);
            int horseOut = CalculateBonus(extendResBase.horse, terrain.horseBonus);
            int clothOut = CalculateBonus(extendResBase.cloth, terrain.clothBonus);
            int leatherOut = CalculateBonus(extendResBase.leather, terrain.leatherBonus);
            int forageOut = CalculateBonus(extendResBase.forage, terrain.forageBonus);
            int saltOut = CalculateBonus(extendResBase.salt, terrain.saltBonus);
            int ironOut = CalculateBonus(extendResBase.iron, terrain.ironBonus);
            int copperOut = CalculateBonus(extendResBase.copper, terrain.copperBonus);
            int goldOreOut = CalculateBonus(extendResBase.goldOre, terrain.goldOreBonus);
            int clayOut = CalculateBonus(extendResBase.clay, terrain.clayBonus);
            
            // 9. 构建CityData（仅存储预制体引用和计算后的数据）
            CityData cityData = new CityData();
            cityData.go = cityObj;
            cityData.type = type;
            cityData.localPos = localPos;
            cityData.rt = cityObj.GetComponent<RectTransform>();
            cityData.terrainConfig = terrain;
            
            // 赋值计算后的资源产出
            cityData.foodOut = foodOut;
            cityData.goldOut = goldOut;
            cityData.peopleOut = peopleOut;
            cityData.armyOut = armyOut;
            cityData.woodOut = woodOut;
            cityData.stoneOut = stoneOut;
            cityData.livestockOut = livestockOut;
            cityData.horseOut = horseOut;
            cityData.clothOut = clothOut;
            cityData.leatherOut = leatherOut;
            cityData.forageOut = forageOut;
            cityData.saltOut = saltOut;
            cityData.ironOut = ironOut;
            cityData.copperOut = copperOut;
            cityData.goldOreOut = goldOreOut;
            cityData.clayOut = clayOut;
            
            // 10. 绑定点击事件（预制体需提前添加Button组件，若无则自动添加）
            AddCityClickEvent(cityObj, cityData);

            // 11. 加入管理列表 & 触发事件
            cityList.Add(cityData);
            if (type == CityType.Capital) currentCapital = cityData;
            EventManager.OnCityCreated?.Invoke();
            Debug.Log($"✅ 预制体创建城市成功！类型：{type}，位置：{localPos}，地形：{terrain?.terrainName ?? "未知"}");
        }
        
        // 新增：从SO读取核心资源基础值
        private Vector4 GetCoreResBaseByType(CityType type)
        {
            return type switch
            {
                CityType.Town => cityTypeConfigSO.resTown,
                CityType.Normal => cityTypeConfigSO.resNormal,
                CityType.Capital => cityTypeConfigSO.resCapital,
                CityType.Farm => cityTypeConfigSO.resFarm,
                CityType.Market => cityTypeConfigSO.resMarket,
                CityType.Port => cityTypeConfigSO.resPort,
                _ => Vector4.zero
            };
        }
        
        // 新增：从SO读取扩展资源基础值
        private Vector12 GetExtendResBaseByType(CityType type)
        {
            return type switch
            {
                CityType.Town => cityTypeConfigSO.resExtendTown,
                CityType.Normal => cityTypeConfigSO.resExtendNormal,
                CityType.Capital => cityTypeConfigSO.resExtendCapital,
                CityType.Farm => cityTypeConfigSO.resExtendFarm,
                CityType.Market => cityTypeConfigSO.resExtendMarket,
                CityType.Port => cityTypeConfigSO.resExtendPort,
                _ => new Vector12()
            };
        }
        
        // 新增：读取对应城市类型的领土倍数
        public float GetTerritoryMultByType(CityType type)
        {
            if (cityTypeConfigSO == null) return 1.0f; // 默认值
            return type switch
            {
                CityType.Town => cityTypeConfigSO.territoryMultTown,
                CityType.Normal => cityTypeConfigSO.territoryMultNormal,
                CityType.Capital => cityTypeConfigSO.territoryMultCapital,
                CityType.Farm => cityTypeConfigSO.territoryMultFarm,
                CityType.Market => cityTypeConfigSO.territoryMultMarket,
                CityType.Port => cityTypeConfigSO.territoryMultPort,
                _ => 1.0f
            };
        }
        
        // 4. 新增：计算加成的工具方法
        private int CalculateBonus(int baseValue, int bonusPercent)
        {
            if (bonusPercent == 0) return baseValue;
            // 计算百分比加成：基础值 + 基础值 * 加成百分比 / 100
            float bonusValue = baseValue * (1 + bonusPercent / 100f);
            return Mathf.Max(0, Mathf.RoundToInt(bonusValue)); // 确保结果非负
        }
        
        //计算加成的工具方法
        void MoveCapital()
        {
            // 旧都城改为普通城
            if (currentCapital == null) return;
    
            // 方案1：替换预制体（推荐）
            Destroy(currentCapital.go); // 销毁旧都城对象
            GameObject newNormalCity = Instantiate(cityTypeConfigSO.prefabNormal, mapRoot);
            newNormalCity.name = $"City_Normal_{cityList.Count}";
            RectTransform newRt = newNormalCity.GetComponent<RectTransform>();
            newRt.localPosition = currentCapital.localPos;
            newRt.localScale = Vector3.one;
    
            // 更新旧都城的CityData
            currentCapital.go = newNormalCity;
            currentCapital.type = CityType.Normal;
            currentCapital.rt = newRt;
            // 重新计算普通城的资源产出
            Vector4 normalCoreRes = cityTypeConfigSO.resNormal;
            Vector12 normalExtendRes = cityTypeConfigSO.resExtendNormal;
            currentCapital.foodOut = CalculateBonus((int)normalCoreRes.x, currentCapital.terrainConfig.foodBonus);
            currentCapital.goldOut = CalculateBonus((int)normalCoreRes.y, currentCapital.terrainConfig.goldBonus);
            currentCapital.peopleOut = CalculateBonus((int)normalCoreRes.z, currentCapital.terrainConfig.peopleBonus);
            currentCapital.armyOut = CalculateBonus((int)normalCoreRes.w, currentCapital.terrainConfig.armyBonus);
            // 扩展资源同理...
            currentCapital.woodOut = CalculateBonus((int)normalCoreRes.x, currentCapital.terrainConfig.woodBonus);
            currentCapital.stoneOut = CalculateBonus((int)normalCoreRes.y, currentCapital.terrainConfig.stoneBonus);
            currentCapital.livestockOut = CalculateBonus((int)normalCoreRes.z, currentCapital.terrainConfig.livestockBonus);
            currentCapital.horseOut = CalculateBonus((int)normalCoreRes.w, currentCapital.terrainConfig.horseBonus);
            currentCapital.clothOut = CalculateBonus((int)normalCoreRes.x, currentCapital.terrainConfig.clothBonus);
            currentCapital.leatherOut = CalculateBonus((int)normalCoreRes.y, currentCapital.terrainConfig.leatherBonus);
            currentCapital.forageOut = CalculateBonus((int)normalCoreRes.z, currentCapital.terrainConfig.forageBonus);
            currentCapital.saltOut = CalculateBonus((int)normalCoreRes.w, currentCapital.terrainConfig.saltBonus);
            currentCapital.ironOut = CalculateBonus((int)normalCoreRes.x, currentCapital.terrainConfig.ironBonus);
            currentCapital.copperOut = CalculateBonus((int)normalCoreRes.y, currentCapital.terrainConfig.copperBonus);
            currentCapital.goldOreOut = CalculateBonus((int)normalCoreRes.z, currentCapital.terrainConfig.goldOreBonus);
            currentCapital.clayOut = CalculateBonus((int)normalCoreRes.w, currentCapital.terrainConfig.clayBonus);
            
            AddCityClickEvent(newNormalCity, currentCapital);

            // 创建新都城
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