using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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

        [Header("地形信息")]
        public Text txtTerrainName;
        public Text txtTerrainBonusSummary;

        [Header("下拉面板")]
        public GameObject panelExtended;

        [Header("引用")]
        public NationResManager resManager;
        public NationCityManager cityManager;
        public TerrainManager terrainManager;

        private const float ResSettleCycle = 3f;
        private NationCityManager.CityData currentSelectedCity;

        void Update()
        {
            /*if (!NationSettingPanel.isNationCreated || resManager == null || cityManager == null || terrainManager == null)
            {
                gameObject.SetActive(false);
                return;
            }*/

            gameObject.SetActive(true);
            UpdateSelectedCity();

            if (currentSelectedCity == null) return;
            if (MapGlobalData.savedMapTexture == null) return;

            RefreshAll();
        }

        void UpdateSelectedCity()
        {
            var cityList = cityManager.GetCityDataList();
            if (cityList != null && cityList.Count > 0)
            {
                if (currentSelectedCity == null || currentSelectedCity.go == null)
                {
                    currentSelectedCity = cityList[0];
                }
            }
        }

        void RefreshAll()
        {
            TerrainConfig cfg = GetCurrentCityTerrainConfig();
            if (cfg == null)
            {
                cfg = ScriptableObject.CreateInstance<TerrainConfig>();
                cfg.terrainName = "未知地形";
            }

            RefreshTerrainInfo(cfg);

            Game.Nation.CoreResOutput core = cityManager.GetCoreResOut();

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
                Game.Nation.ExtendResOutput ext = cityManager.GetExtendResOut();

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

        void RefreshTerrainInfo(TerrainConfig cfg)
        {
            if (txtTerrainName == null || txtTerrainBonusSummary == null) return;

            txtTerrainName.text = $"当前地形：{cfg.terrainName}";
            List<string> list = new List<string>();

            if (cfg.foodBonus != 0) list.Add($"粮食{GetBonus(cfg.foodBonus)}");
            if (cfg.goldBonus != 0) list.Add($"金币{GetBonus(cfg.goldBonus)}");
            if (cfg.woodBonus != 0) list.Add($"木材{GetBonus(cfg.woodBonus)}");
            if (cfg.stoneBonus != 0) list.Add($"石料{GetBonus(cfg.stoneBonus)}");
            if (cfg.saltBonus != 0) list.Add($"盐矿{GetBonus(cfg.saltBonus)}");
            if (cfg.ironBonus != 0) list.Add($"铁矿{GetBonus(cfg.ironBonus)}");
            if (cfg.goldOreBonus != 0) list.Add($"金矿{GetBonus(cfg.goldOreBonus)}");
            if (cfg.clayBonus != 0) list.Add($"黏土{GetBonus(cfg.clayBonus)}");

            txtTerrainBonusSummary.text = list.Count > 0 ? $"加成：{string.Join("，", list)}" : "加成：无";
        }

        TerrainConfig GetCurrentCityTerrainConfig()
        {
            if (currentSelectedCity == null) return null;
            if (MapGlobalData.savedMapTexture == null) return null;
            if (terrainManager == null) return null;

            Vector2 pixelPos = cityManager.ConvertLocalPosToPixelPos(currentSelectedCity.localPos);
            return terrainManager.GetTerrainAtPosition(MapGlobalData.savedMapTexture, pixelPos);
        }

        string GetBonus(int p) => p > 0 ? $"+{p}%" : $"{p}%";
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