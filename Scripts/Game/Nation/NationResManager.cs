using UnityEngine;
using System.Collections.Generic;
using Game.Core.Base;
using Game.Core.GameEntry;

namespace Game.Game.Nation
{
    // 城市资源类（基类落地）
    [System.Serializable]
    public class CityResource
    {
        public string cityName;
        // 各资源的基础产量（每轮增长值）
        public Dictionary<ResourceType, int> resourceGrowth = new Dictionary<ResourceType, int>();
        // 各资源当前数量
        public Dictionary<ResourceType, int> currentResource = new Dictionary<ResourceType, int>();

        // 增长当前城市资源
        public void GrowResource()
        {
            foreach (var type in resourceGrowth.Keys)
            {
                currentResource[type] += resourceGrowth[type];
            }
        }
    }
    
    public class NationResManager : BaseManager
    {
        // 所有城市的资源数据
        public List<CityResource> allCityResources = new List<CityResource>();
        // 全局资源总和（供UI显示）
        public Dictionary<ResourceType, int> totalResources = new Dictionary<ResourceType, int>();
        
        void Awake()
        {
            if (Instance == null) Instance = this;
            
            // 新增：自动关联CityManager（避免手动挂载遗漏）
            if (cityManager == null)
            {
                cityManager = GameManager.NationCityManager;
            }
            
        }
        // 初始化资源字典
        // 重写Init方法，确保和GameManager时序兼容
        public override void Init()
        {
            base.Init();
            // 初始化资源字典（原有逻辑保留）
            /*foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                if (!totalResources.ContainsKey(type))
                {
                    totalResources[type] = 0;
                }
            }
            // 初始化城市资源（原有逻辑保留）
            foreach (var cityRes in allCityResources)
            {
                foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
                {
                    if (!cityRes.resourceGrowth.ContainsKey(type))
                    {
                        cityRes.resourceGrowth[type] = 10;
                    }
                    if (!cityRes.currentResource.ContainsKey(type))
                    {
                        cityRes.currentResource[type] = 0;
                    }
                }
            }*/
            CalculateTotalResources();
        }
        
        // 增长所有城市的资源
        public void GrowAllCityResources()
        {
            base.GrowAllCityResources(); // 调用基类逻辑
            
            foreach (var cityRes in allCityResources)
            {
                cityRes.GrowResource();
            }
        }

        // 计算所有城市资源总和（供UI刷新）
        public void CalculateTotalResources()
        {
            // 重置总和
            foreach (var type in totalResources.Keys)
            {
                totalResources[type] = 0;
            }
            // 累加所有城市资源
            foreach (var cityRes in allCityResources)
            {
                foreach (var type in totalResources.Keys)
                {
                    totalResources[type] += cityRes.currentResource[type];
                }
            }

            // 示例：打印总和（实际项目中替换为UI刷新逻辑）
            foreach (var kvp in totalResources)
            {
                Debug.Log($"总资源 {kvp.Key} : {kvp.Value}");
            }
        }


        public static NationResManager Instance;

        [Header("核心资源（默认显示）")] public int food;
        public int gold;
        public int people;
        public int army;

        [Header("扩展资源（下拉显示）")] public int wood;
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

        [Header("结算间隔秒")] public float resUpdateTime = 3f;

        [Header("资源配置")] public List<ResourceConfig> resConfigs; // 拖入16个资源配置文件

        private float resTimer;
        public NationCityManager cityManager;

        void Update()
        {
            if (!NationSettingPanel.isNationCreated) return;

            resTimer += Time.deltaTime;
            if (resTimer >= resUpdateTime)
            {
                resTimer = 0;
                UpdateAllRes();
            }
        }

        public void UpdateAllRes()
        {
            if (cityManager == null) return;

            // 核心资源产出
            var coreOutput = cityManager.GetCoreResOut();
            food += coreOutput.food;
            gold += coreOutput.gold;
            people += coreOutput.people;
            army += coreOutput.army;

            // 扩展资源产出
            var extendOutput = cityManager.GetExtendResOut();
            wood += extendOutput.wood;
            stone += extendOutput.stone;
            livestock += extendOutput.livestock;
            horse += extendOutput.horse;
            cloth += extendOutput.cloth;
            leather += extendOutput.leather;
            forage += extendOutput.forage;
            salt += extendOutput.salt;
            iron += extendOutput.iron;
            copper += extendOutput.copper;
            goldOre += extendOutput.goldOre;
            clay += extendOutput.clay;

            // 资源保底（不小于0）
            food = Mathf.Max(0, food);
            gold = Mathf.Max(0, gold);
            people = Mathf.Max(0, people);
            army = Mathf.Max(0, army);
            wood = Mathf.Max(0, wood);
            stone = Mathf.Max(0, stone);
            livestock = Mathf.Max(0, livestock);
            horse = Mathf.Max(0, horse);
            cloth = Mathf.Max(0, cloth);
            leather = Mathf.Max(0, leather);
            forage = Mathf.Max(0, forage);
            salt = Mathf.Max(0, salt);
            iron = Mathf.Max(0, iron);
            copper = Mathf.Max(0, copper);
            goldOre = Mathf.Max(0, goldOre);
            clay = Mathf.Max(0, clay);
        }

        // 初始化所有资源
        public void InitRes()
        {
            // 核心资源初始值
            food = 100;
            gold = 200;
            people = 50;
            army = 10;

            // 扩展资源初始值
            wood = 50;
            stone = 30;
            livestock = 20;
            horse = 5;
            cloth = 15;
            leather = 10;
            forage = 40;
            salt = 8;
            iron = 12;
            copper = 18;
            goldOre = 3;
            clay = 25;
        }

        // 根据类型获取资源配置（贴图）
        public ResourceConfig GetResConfig(ResourceType type)
        {
            return resConfigs.Find(cfg => cfg.resType == type);
        }
    }

// 核心资源产出结构体
    public struct CoreResOutput
    {
        public int food;
        public int gold;
        public int people;
        public int army;
    }

// 扩展资源产出结构体
    public struct ExtendResOutput
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
    }
}