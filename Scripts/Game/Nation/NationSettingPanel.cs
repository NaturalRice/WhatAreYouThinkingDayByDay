using Game.Core.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Game.Nation
{
    public class NationSettingPanel : BasePanel
    {
        [Header("UI 绑定")] public InputField inputNationName;
        public Dropdown dropdownGovernment;
        public Slider sliderPopulation;
        public Slider sliderResource;
        public Image imgNationColor;
        public Button btnConfirm;
        public Button btnClose;

        [Header("数据")] public Color currentColor = Color.blue;
        
        public static NationSettingPanel Instance;

        void Awake()
        {
            Instance = this;
        }
        
        private string[] governmentTypes =
        {
            "君主制", "共和制", "联邦制", "部落联盟", "帝国制", "城邦制"
        };

        // 关键：国家是否创建成功
        public static bool isNationCreated = false;

        void Start()
        {
            dropdownGovernment.ClearOptions();
            dropdownGovernment.AddOptions(new System.Collections.Generic.List<string>(governmentTypes));

            btnConfirm.onClick.AddListener(OnConfirmCreateNation);
            btnClose.onClick.AddListener(() => gameObject.SetActive(false));
            imgNationColor.GetComponent<Button>().onClick.AddListener(OnRandomColor);

            gameObject.SetActive(false);
        }

        void OnRandomColor()
        {
            currentColor = new Color(Random.value, Random.value, Random.value);
            imgNationColor.color = currentColor;
        }

        // 【严格条件】必须全部填写才能创建国家
        void OnConfirmCreateNation()
        {
            string nationName = inputNationName.text.Trim();
            int population = Mathf.RoundToInt(sliderPopulation.value * 100);
            int resource = Mathf.RoundToInt(sliderResource.value * 100);

            // 严格条件判断
            if (string.IsNullOrEmpty(nationName))
            {
                Debug.Log("请输入国家名称！");
                return;
            }

            if (dropdownGovernment.value < 0)
            {
                Debug.Log("请选择政体！");
                return;
            }

            if (population <= 0)
            {
                Debug.Log("人口必须大于 0！");
                return;
            }

            if (resource <= 0)
            {
                Debug.Log("资源必须大于 0！");
                return;
            }

            // 同步颜色给城池系统
            FindObjectOfType<NationCityManager>().SetNationData(currentColor);

            Debug.Log("========================");
            Debug.Log("国家创建成功！");
            Debug.Log("可以开始建立城池！");
            Debug.Log("========================");

            gameObject.SetActive(false);
            // 创建成功
            isNationCreated = true;
        }

        public void OpenPanel()
        {
            gameObject.SetActive(true);
        }

        public void SaveNationData()
        {
            Debug.Log($"✅ 保存国家设定：");
        }
    }
}