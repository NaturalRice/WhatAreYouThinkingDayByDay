using UnityEngine;
using Game.Game.Nation;
using System.Collections.Generic;

public class AINationManager : MonoBehaviour
{
    public static AINationManager Instance;

    [Header("AI 设置")]
    public int maxAICount = 5;
    public float aiBuildInterval = 5f;
    public float minDistanceFromPlayer = 150f;

    [Header("引用")]
    public NationCityManager playerCityManager;
    public RectTransform mapRoot;

    private int _currentAICount = 0;
    
    public List<NationCityManager> aiNations = new List<NationCityManager>();
    public float aiAttackInterval = 5f; // AI每5秒检测一次攻击
    private float aiAttackTimer;

    private void Awake()
    {
        Instance = this;
        AutoFindReferences();
    }
    
    private void Update()
    {
        aiAttackTimer += Time.deltaTime;
        if (aiAttackTimer >= aiAttackInterval)
        {
            aiAttackTimer = 0;
            foreach (var aiNation in aiNations)
            {
                AITryAttack(aiNation);
            }
        }
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
        for (int i = 0; i < maxAICount; i++) SpawnOneAI();
    }
    
    private void SpawnOneAI()
    {
        GameObject aiRoot = new GameObject($"AI_Nation_{_currentAICount++}");
        aiRoot.transform.SetParent(mapRoot, false);

        NationCityManager aiCityMgr = aiRoot.AddComponent<NationCityManager>();
        NationResManager aiResMgr = aiRoot.AddComponent<NationResManager>();
        AICityBuilder aiBuilder = aiRoot.AddComponent<AICityBuilder>();

        aiCityMgr.isPlayer = false;

        aiCityMgr.cityTypeConfigSO = playerCityManager.cityTypeConfigSO;
        aiCityMgr.mapRoot = mapRoot;
        aiCityMgr.resManager = aiResMgr;
        aiCityMgr.mapTexture = playerCityManager.mapTexture;

        aiResMgr.InitRes();
        aiResMgr.food = 200;
        aiResMgr.gold = 150;
        aiResMgr.wood = 150;
        aiResMgr.stone = 150;

        Color playerColor = playerCityManager.nationColor;
        Color aiColor = GetRandomColorAvoidPlayer(playerColor);
    
        // 🔥 只需要这两行！
        aiCityMgr.nationColor = aiColor;

        aiCityMgr.SetNationData(aiColor);

        aiBuilder.StartAI(aiCityMgr, aiResMgr, this);
    }

    private Color GetRandomColorAvoidPlayer(Color player)
    {
        Color c;
        do { c = Random.ColorHSV(0,1,0.7f,1,0.7f,1); } 
        while (ColorDistance(c, player) < 0.4f);
        return c;
    }

    private float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
    }
    
    // AI尝试发起攻击
    private void AITryAttack(NationCityManager aiNation)
    {
        // 1. AI无城市/无军队，跳过
        if (aiNation.cityList.Count == 0) return;
        CityData fromCity = null;
        foreach (var city in aiNation.cityList)
        {
            if (city.armyOut > 0)
            {
                fromCity = city;
                break;
            }
        }
        if (fromCity == null) return;

        // 2. 寻找敌方城市（非自己的城市）
        List<CityData> enemyCities = new List<CityData>();
        foreach (var nation in FindObjectsOfType<NationCityManager>())
        {
            if (nation != aiNation && nation.cityList.Count > 0)
            {
                enemyCities.AddRange(nation.cityList);
            }
        }
        if (enemyCities.Count == 0) return;

        // 3. 随机选一个敌方城市攻击
        CityData targetCity = enemyCities[Random.Range(0, enemyCities.Count)];
        NationCityManager targetNation = NationWarManager.Instance.GetNationByCity(targetCity);
        if (targetNation == null) return;

        // 4. 宣战+出兵（至少1支）
        if (aiNation.diplomacy == null)
            aiNation.diplomacy = new NationDiplomacy(aiNation);
        if (!aiNation.diplomacy.IsAtWar(targetNation))
            aiNation.diplomacy.DeclareWar(targetNation);

        int sendCount = Mathf.Max(1, fromCity.armyOut / 2);
        NationWarManager.Instance.SendArmy(aiNation, fromCity, targetCity, sendCount);
        Debug.Log($"[AI攻击] {aiNation.gameObject.name} 派出{sendCount}军队攻击{targetNation.gameObject.name}的{targetCity.type}");
    }

    // 注册AI国家
    public void RegisterAINation(NationCityManager nation)
    {
        if (!nation.isPlayer && !aiNations.Contains(nation))
        {
            aiNations.Add(nation);
        }
    }
}