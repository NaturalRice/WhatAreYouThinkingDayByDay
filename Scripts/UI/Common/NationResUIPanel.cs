using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Game.Game.Nation;

namespace Game.UI.Common
{
    public class NationResUIPanel : MonoBehaviour
    {
        [Header("核心资源（默认显示）")] public ResourceItem itemFood;
        public ResourceItem itemGold;
        public ResourceItem itemPeople;
        public ResourceItem itemArmy;

        [Header("扩展资源（下拉显示）")] public ResourceItem itemWood;
        public ResourceItem itemStone;
        public ResourceItem itemLivestock;
        public ResourceItem itemHorse;
        public ResourceItem itemCloth;
        public ResourceItem itemLeather;
        public ResourceItem itemForage;
        public ResourceItem itemSalt;
        public ResourceItem itemIron;
        public ResourceItem itemCopper;
        public ResourceItem itemGoldOre;
        public ResourceItem itemClay;

        [Header("下拉面板")] public GameObject panelExtended;

        [Header("引用")] public NationResManager resManager;
        public NationCityManager cityManager;

        void Update()
        {
            /*if (!NationSettingPanel.isNationCreated || resManager == null || cityManager == null)
            {
                gameObject.SetActive(false);
                return;
            }*/

            gameObject.SetActive(true);
            RefreshAll();
        }

        void RefreshAll()
        {
            // 核心资源
            var core = cityManager.GetCoreResOut();
            itemFood.SetValue(resManager.food, core.food);
            itemGold.SetValue(resManager.gold, core.gold);
            itemPeople.SetValue(resManager.people, core.people);
            itemArmy.SetValue(resManager.army, core.army);

            // 扩展资源（只有展开才刷新）
            if (panelExtended != null && panelExtended.activeSelf)
            {
                var ext = cityManager.GetExtendResOut();
                itemWood.SetValue(resManager.wood, ext.wood);
                itemStone.SetValue(resManager.stone, ext.stone);
                itemLivestock.SetValue(resManager.livestock, ext.livestock);
                itemHorse.SetValue(resManager.horse, ext.horse);
                itemCloth.SetValue(resManager.cloth, ext.cloth);
                itemLeather.SetValue(resManager.leather, ext.leather);
                itemForage.SetValue(resManager.forage, ext.forage);
                itemSalt.SetValue(resManager.salt, ext.salt);
                itemIron.SetValue(resManager.iron, ext.iron);
                itemCopper.SetValue(resManager.copper, ext.copper);
                itemGoldOre.SetValue(resManager.goldOre, ext.goldOre);
                itemClay.SetValue(resManager.clay, ext.clay);
            }
        }

        // 刷新核心资源
        void RefreshCoreRes()
        {
            CoreResOutput coreOut = cityManager.GetCoreResOut();
            itemFood.SetValue(resManager.food, coreOut.food);
            itemGold.SetValue(resManager.gold, coreOut.gold);
            itemPeople.SetValue(resManager.people, coreOut.people);
            itemArmy.SetValue(resManager.army, coreOut.army);
        }

        // 刷新扩展资源
        void RefreshExtendRes()
        {
            ExtendResOutput extendOut = cityManager.GetExtendResOut();
            itemWood.SetValue(resManager.wood, extendOut.wood);
            itemStone.SetValue(resManager.stone, extendOut.stone);
            itemLivestock.SetValue(resManager.livestock, extendOut.livestock);
            itemHorse.SetValue(resManager.horse, extendOut.horse);
            itemCloth.SetValue(resManager.cloth, extendOut.cloth);
            itemLeather.SetValue(resManager.leather, extendOut.leather);
            itemForage.SetValue(resManager.forage, extendOut.forage);
            itemSalt.SetValue(resManager.salt, extendOut.salt);
            itemIron.SetValue(resManager.iron, extendOut.iron);
            itemCopper.SetValue(resManager.copper, extendOut.copper);
            itemGoldOre.SetValue(resManager.goldOre, extendOut.goldOre);
            itemClay.SetValue(resManager.clay, extendOut.clay);
        }
    }

// 资源项组件（挂载在每个 Item_XXX 上）
    [System.Serializable]
    public class ResourceItem
    {
        public Image imgIcon;
        public Text textValue;

        public void SetValue(int current, int add)
        {
            if (textValue != null)
                textValue.text = $"{current} (+{add})";
        }
    }
}