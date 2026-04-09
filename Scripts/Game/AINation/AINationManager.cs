using UnityEngine;
using Game.Game.Nation;
using Game.Map;

public class AINationManager : MonoBehaviour
{
    public static AINationManager Instance;

    [Header("AI 设定")]
    public int maxAICount = 2;
    public float aiBuildInterval = 12f;
    public float minDistanceFromPlayer = 250f;

    [Header("引用")]
    public NationCityManager playerCityManager;
    public NationSettingPanel nationPanel;
    public RectTransform mapRoot;

    private int _currentAICount = 0;

    private void Awake()
    {
        Instance = this;
        AutoFindReferences();
    }

    private void AutoFindReferences()
    {
        if (playerCityManager == null)
            playerCityManager = FindObjectOfType<NationCityManager>();

        if (mapRoot == null)
            mapRoot = playerCityManager.mapRoot;
    }

    public void StartSpawnAI()
    {
        if (_currentAICount > 0) return;

        for (int i = 0; i < maxAICount; i++)
        {
            SpawnOneAI();
        }
    }

    private void SpawnOneAI()
    {
        GameObject aiRoot = new GameObject($"AI_Nation_{_currentAICount++}");
        aiRoot.transform.SetParent(mapRoot, false);

        NationCityManager aiCityMgr = aiRoot.AddComponent<NationCityManager>();
        NationResManager aiResMgr = aiRoot.AddComponent<NationResManager>();
        NationTerritoryManager aiTerriMgr = aiRoot.AddComponent<NationTerritoryManager>();
        AICityBuilder aiBuilder = aiRoot.AddComponent<AICityBuilder>();

        // ##############################
        // 🔥 关键：AI 关闭 UI 模式
        // ##############################
        aiCityMgr.isPlayer = false;

        // 自动复制配置
        aiCityMgr.cityTypeConfigSO = playerCityManager.cityTypeConfigSO;
        aiCityMgr.mapRoot = mapRoot;
        aiCityMgr.territoryManager = aiTerriMgr;
        aiCityMgr.resManager = aiResMgr;
        aiCityMgr.mapTexture = playerCityManager.mapTexture;

        aiTerriMgr.mapRoot = mapRoot;
        aiTerriMgr.cityManager = aiCityMgr;
        aiTerriMgr.cityTypeConfig = playerCityManager.cityTypeConfigSO;
        aiTerriMgr.territoryMask = playerCityManager.territoryManager.territoryMask;

        aiResMgr.InitRes();

        // 随机颜色
        Color playerColor = playerCityManager.nationColor;
        Color aiColor = GetRandomColorAvoidPlayer(playerColor);
        aiCityMgr.SetNationData(aiColor);
        aiCityMgr.nationColor = aiColor;
        aiTerriMgr.nationColor = aiColor;

        aiBuilder.StartAI(aiCityMgr, aiResMgr, this);
    }

    private Color GetRandomColorAvoidPlayer(Color player)
    {
        Color c;
        do
        {
            c = Random.ColorHSV(0, 1, 0.7f, 1, 0.7f, 1);
        }
        while (ColorDistance(c, player) < 0.4f);
        return c;
    }

    private float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
    }
}