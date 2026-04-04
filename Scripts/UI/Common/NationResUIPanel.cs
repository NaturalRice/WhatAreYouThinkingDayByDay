using UnityEngine;
using UnityEngine.UI;
using Game.Game.Nation;
using Game.Game.Terrain;
using Game.Map;

namespace Game.UI.Common
{
    public class NationResUIPanel : MonoBehaviour
    {
        [Header("核心资源")]
        public ResourceItemExt itemFood;
        public ResourceItemExt itemGold;
        public ResourceItemExt itemPeople;
        public ResourceItemExt itemArmy;

        [Header("扩展资源")]
        public ResourceItemExt itemWood;
        public ResourceItemExt itemStone;
        public ResourceItemExt itemLivestock;
        public ResourceItemExt itemHorse;
        public ResourceItemExt itemCloth;
        public ResourceItemExt itemLeather;
        public ResourceItemExt itemForage;
        public ResourceItemExt itemSalt;
        public ResourceItemExt itemIron;
        public ResourceItemExt itemCopper;
        public ResourceItemExt itemGoldOre;
        public ResourceItemExt itemClay;

        [Header("下拉面板")]
        public GameObject panelExtended;

        [Header("引用")]
        public NationResManager resManager;
        public NationCityManager cityManager = new NationCityManager();
        public TerrainManager terrainManager;

        private const float ResSettleCycle = 3f;
        private CityData currentSelectedCity;
        
        private bool isSubscribed = false;


        void Update()
        {
            gameObject.SetActive(true);
            UpdateSelectedCity();

            if (currentSelectedCity == null) return;
            if (MapGlobalData.savedMapTexture == null) return;

            RefreshAll();
        }

        void UpdateSelectedCity()
        {
            // 🔥 改为取点击的城市，不是默认第一个
            currentSelectedCity = cityManager.currentSelectedCity;
        }

        void RefreshAll()
        {
            // 🔥 从选中城市获取地形配置（不再依赖独立的地形文本）
            TerrainConfig cfg = currentSelectedCity.terrainConfig;
            if (cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<TerrainConfig>();
                cfg.terrainName = "未知地形";
                // 兜底：无地形时加成设为0
                cfg.foodBonus = 0;
                cfg.goldBonus = 0;
                cfg.woodBonus = 0;
                cfg.stoneBonus = 0;
                cfg.livestockBonus = 0;
                cfg.horseBonus = 0;
                cfg.clothBonus = 0;
                cfg.leatherBonus = 0;
                cfg.forageBonus = 0;
                cfg.saltBonus = 0;
                cfg.ironBonus = 0;
                cfg.copperBonus = 0;
                cfg.goldOreBonus = 0;
                cfg.clayBonus = 0;
            }

            // 核心资源刷新
            int fPerHour = Mathf.RoundToInt(currentSelectedCity.foodOut * 1200);
            int gPerHour = Mathf.RoundToInt(currentSelectedCity.goldOut * 1200);
            int pPerHour = Mathf.RoundToInt(currentSelectedCity.peopleOut * 1200);
            int aPerHour = Mathf.RoundToInt(currentSelectedCity.armyOut * 1200);

            itemFood.SetValue(resManager.food, currentSelectedCity.foodOut, fPerHour, cfg.foodBonus);
            itemGold.SetValue(resManager.gold, currentSelectedCity.goldOut, gPerHour, cfg.goldBonus);
            itemPeople.SetValue(resManager.people, currentSelectedCity.peopleOut, pPerHour, cfg.goldBonus);
            itemArmy.SetValue(resManager.army, currentSelectedCity.armyOut, aPerHour, cfg.goldBonus);

            if (panelExtended != null && panelExtended.activeSelf)
            {
                // 扩展资源刷新
                int wPerHour = Mathf.RoundToInt(currentSelectedCity.woodOut * 1200);
                int sPerHour = Mathf.RoundToInt(currentSelectedCity.stoneOut * 1200);
                int lPerHour = Mathf.RoundToInt(currentSelectedCity.livestockOut * 1200);
                int hPerHour = Mathf.RoundToInt(currentSelectedCity.horseOut * 1200);
                int cPerHour = Mathf.RoundToInt(currentSelectedCity.clothOut * 1200);
                int lePerHour = Mathf.RoundToInt(currentSelectedCity.leatherOut * 1200);
                int foPerHour = Mathf.RoundToInt(currentSelectedCity.forageOut * 1200);
                int saPerHour = Mathf.RoundToInt(currentSelectedCity.saltOut * 1200);
                int iPerHour = Mathf.RoundToInt(currentSelectedCity.ironOut * 1200);
                int coPerHour = Mathf.RoundToInt(currentSelectedCity.copperOut * 1200);
                int goPerHour = Mathf.RoundToInt(currentSelectedCity.goldOreOut * 1200);
                int clPerHour = Mathf.RoundToInt(currentSelectedCity.clayOut * 1200);

                itemWood.SetValue(resManager.wood, currentSelectedCity.woodOut, wPerHour, cfg.woodBonus);
                itemStone.SetValue(resManager.stone, currentSelectedCity.stoneOut, sPerHour, cfg.stoneBonus);
                itemLivestock.SetValue(resManager.livestock, currentSelectedCity.livestockOut, lPerHour, cfg.livestockBonus);
                itemHorse.SetValue(resManager.horse, currentSelectedCity.horseOut, hPerHour, cfg.horseBonus);
                itemCloth.SetValue(resManager.cloth, currentSelectedCity.clothOut, cPerHour, cfg.clothBonus);
                itemLeather.SetValue(resManager.leather, currentSelectedCity.leatherOut, lePerHour, cfg.leatherBonus);
                itemForage.SetValue(resManager.forage, currentSelectedCity.forageOut, foPerHour, cfg.forageBonus);
                itemSalt.SetValue(resManager.salt, currentSelectedCity.saltOut, saPerHour, cfg.saltBonus);
                itemIron.SetValue(resManager.iron, currentSelectedCity.ironOut, iPerHour, cfg.ironBonus);
                itemCopper.SetValue(resManager.copper, currentSelectedCity.copperOut, coPerHour, cfg.copperBonus);
                itemGoldOre.SetValue(resManager.goldOre, currentSelectedCity.goldOreOut, goPerHour, cfg.goldOreBonus);
                itemClay.SetValue(resManager.clay, currentSelectedCity.clayOut, clPerHour, cfg.clayBonus);
            }
        }
    }

    [System.Serializable]
    public class ResourceItemExt
    {
        public Text txtCurrentValue;
        public Text txtFinalOutput;
        public Text txtPerHour;
        public Text txtBonusPercent;

        public void SetValue(int current, int output, int perHour, int bonus)
        {
            if (txtCurrentValue != null) txtCurrentValue.text = $"当前：{current}";
            if (txtFinalOutput != null) txtFinalOutput.text = $"单次：{output}";
            if (txtPerHour != null) txtPerHour.text = $"每小时：{perHour}";
            if (txtBonusPercent != null) txtBonusPercent.text = bonus != 0 ? $"{(bonus > 0 ? "+" : "")}{bonus}%" : "加成：0%";
        }
    }
}