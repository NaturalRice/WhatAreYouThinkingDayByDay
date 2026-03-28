using UnityEngine;
using UnityEngine.UI;

public class NationSettingPanel : MonoBehaviour
{
    [Header("UI 绑定")]
    public InputField inputNationName;
    public Dropdown dropdownGovernment;
    public Slider sliderPopulation;
    public Slider sliderResource;
    public Image imgNationColor;
    public Button btnConfirm;
    public Button btnClose;

    [Header("数据")]
    private Color currentColor = Color.blue;
    private string[] governmentTypes = {
        "君主制", "共和制", "联邦制", "部落联盟", "帝国制", "城邦制"
    };

    void Start()
    {
        // 初始化政体选项
        dropdownGovernment.ClearOptions();
        dropdownGovernment.AddOptions(new System.Collections.Generic.List<string>(governmentTypes));

        // 按钮事件
        btnConfirm.onClick.AddListener(OnConfirmCreateNation);
        btnClose.onClick.AddListener(() => gameObject.SetActive(false));
        imgNationColor.GetComponent<Button>().onClick.AddListener(OnRandomColor);

        // 初始化隐藏
        gameObject.SetActive(false);
    }

    // 随机国家颜色
    void OnRandomColor()
    {
        currentColor = new Color(Random.value, Random.value, Random.value);
        imgNationColor.color = currentColor;
    }

    // 确认创建国家
    void OnConfirmCreateNation()
    {
        string nationName = inputNationName.text;
        string government = governmentTypes[dropdownGovernment.value];
        int population = Mathf.RoundToInt(sliderPopulation.value * 100);
        int resource = Mathf.RoundToInt(sliderResource.value * 100);

        if (string.IsNullOrEmpty(nationName))
        {
            Debug.Log("国家名称不能为空");
            return;
        }

        // 输出创建结果（正式版可保存数据）
        Debug.Log("=== 创建国家成功 ===");
        Debug.Log("国家名：" + nationName);
        Debug.Log("政体：" + government);
        Debug.Log("人口：" + population);
        Debug.Log("资源：" + resource);

        gameObject.SetActive(false);
    }

    // 打开面板
    public void OpenPanel()
    {
        gameObject.SetActive(true);
    }
    
    // 保存国家数据（给GameSceneUIManager调用）
    public void SaveNationData()
    {
        string nationName = inputNationName.text;
        if (string.IsNullOrEmpty(nationName))
        {
            Debug.LogWarning("⚠️ 未设置国家名称，使用默认名称");
            nationName = "无名国度";
        }

        Debug.Log($"✅ 保存国家：{nationName}");
    }
}