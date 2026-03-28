using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSceneUIManager : MonoBehaviour
{
    [Header("场景按钮")]
    public Button btnBackToEditor;
    public Button btnSaveWorld;  // 保存世界按钮

    [Header("功能面板")]
    public NationSettingPanel nationPanel;
    public EventTimelinePanel timelinePanel;

    [Header("功能按钮")]
    public Button btnOpenNation;
    public Button btnOpenTimeline;

    [Header("地图显示")]
    public RawImage mapDisplayRawImage;
    public MapViewerController mapViewer;

    [Header("需要隐藏/显示的UI")]
    public GameObject topNav;        // 顶部导航
    public GameObject leftFuncBar;  // 左侧功能栏

    private bool isGameUIHidden = false; // 标记UI是否隐藏（游戏模式状态）

    void Start()
    {
        btnBackToEditor.onClick.AddListener(GoBackToMapDraw);
        btnSaveWorld.onClick.AddListener(OnSaveAndStartGame);
        btnOpenNation.onClick.AddListener(() => nationPanel.OpenPanel());
        btnOpenTimeline.onClick.AddListener(() => timelinePanel.OpenPanel());

        LoadSavedMapTexture();
    }

    void Update()
    {
        // 监听 ESC 键：切换 UI 显示/隐藏（只在游戏模式下生效）
        if (Input.GetKeyDown(KeyCode.Escape) && isGameUIHidden)
        {
            ToggleGameUI(true); // 呼出 UI
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && !isGameUIHidden && topNav.activeSelf)
        {
            ToggleGameUI(false); // 隐藏 UI（仅游戏模式下可隐藏）
        }
    }

    void LoadSavedMapTexture()
    {
        if (MapGlobalData.savedMapTexture != null)
        {
            mapDisplayRawImage.texture = MapGlobalData.savedMapTexture;
            mapDisplayRawImage.color = Color.white;
        }
        else
        {
            Debug.Log("未找到保存的地图");
        }
    }

    // 保存世界并开始游戏（隐藏UI）
    void OnSaveAndStartGame()
    {
        Debug.Log("========================");
        Debug.Log("🌍 世界保存成功！");
        Debug.Log("🎮 正式进入游戏模式！");
        Debug.Log("⌨️ 按 ESC 键呼出/隐藏菜单");
        Debug.Log("========================");

        // 1. 保存数据
        nationPanel.SaveNationData();
        timelinePanel.SaveTimeline();

        // 2. 隐藏UI，标记进入游戏模式
        ToggleGameUI(false);
        isGameUIHidden = true;
    }

    // 核心：切换 UI 显示/隐藏
    void ToggleGameUI(bool isShow)
    {
        topNav.SetActive(isShow);
        leftFuncBar.SetActive(isShow);
        
        // 隐藏时强制关闭弹窗，避免弹窗残留
        if (!isShow)
        {
            nationPanel.gameObject.SetActive(false);
            timelinePanel.gameObject.SetActive(false);
        }
    }

    // 返回编辑器
    void GoBackToMapDraw()
    {
        SceneManager.LoadScene("MapDraw");
    }
}