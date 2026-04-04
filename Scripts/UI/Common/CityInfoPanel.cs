using UnityEngine;
using UnityEngine.UI;
using Game.Game.Nation;

namespace Game.UI.Common
{
    public class CityInfoPanel : MonoBehaviour
    {
        public static CityInfoPanel Instance;

        public GameObject panelRoot;
        public Text txtCityName;
        public Text txtTerrainName;
        public Text txtTerrainBonus;

        private CityData currentCity;

        void Awake()
        {
            Instance = this;
            panelRoot.SetActive(false);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (currentCity != null &&
                    !RectTransformUtility.RectangleContainsScreenPoint(
                        panelRoot.GetComponent<RectTransform>(),
                        Input.mousePosition,
                        null))
                {
                    HidePanel();
                }
            }
        }

        public void ShowPanel(CityData data)
        {
            if (data == null) return;

            currentCity = data;
            panelRoot.SetActive(true);
            transform.position = Input.mousePosition + new Vector3(0, 60, 0);

            txtCityName.text = "城市：" + data.type;

            if (data.terrainConfig != null)
            {
                txtTerrainName.text = "地形：" + data.terrainConfig.terrainName;

                string bonus = "";
                if (data.terrainConfig.foodBonus != 0) bonus += $"粮食{data.terrainConfig.foodBonus:+#;-#;0}% ";
                if (data.terrainConfig.goldOreBonus != 0) bonus += $"金矿{data.terrainConfig.goldOreBonus:+#;-#;0}% ";
                if (data.terrainConfig.woodBonus != 0) bonus += $"木材{data.terrainConfig.woodBonus:+#;-#;0}% ";
                txtTerrainBonus.text = bonus == "" ? "无加成" : bonus;
            }
            else
            {
                txtTerrainName.text = "地形：未知";
                txtTerrainBonus.text = "无加成";
            }
        }

        public void HidePanel()
        {
            panelRoot.SetActive(false);
            currentCity = null;
        }
    }
}