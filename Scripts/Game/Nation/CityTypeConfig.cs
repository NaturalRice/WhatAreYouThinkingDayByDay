using Game.Game.Nation;
using UnityEngine;
// 新建 CityTypeConfig.cs（挂载到各城市预制体上）
[CreateAssetMenu(fileName = "CityTypeConfig", menuName = "游戏配置/城市类型配置")]
public class CityTypeConfig : ScriptableObject
{
    [Header("城市预制体")]
    public GameObject prefabTown;       // 小镇预制体
    public GameObject prefabNormal;     // 普通城预制体
    public GameObject prefabCapital;    // 都城预制体
    public GameObject prefabFarm;       // 农村预制体
    public GameObject prefabMarket;     // 市集预制体
    public GameObject prefabPort;       // 港口预制体

    [Header("领土倍数")] 
    public float territoryMultTown = 0.4f;
    public float territoryMultNormal = 0.6f;
    public float territoryMultCapital = 1.0f;
    public float territoryMultFarm = 0.3f;
    public float territoryMultMarket = 0.3f;
    public float territoryMultPort = 0.3f;
    
    // ====================== 🔥 新增：城市大小 ======================
    [Header("城市贴图缩放大小")]
    public float cityScaleTown = 0.4f;
    public float cityScaleNormal = 0.6f;
    public float cityScaleCapital = 1.0f;
    public float cityScaleFarm = 0.3f;
    public float cityScaleMarket = 0.3f;
    public float cityScalePort = 0.3f;

    // ====================== 🔥 新增：领土半径单独系数 ======================
    [Header("领土半径系数（不影响原有倍数）")]
    public float radiusMultTown = 1f;
    public float radiusMultNormal = 1f;
    public float radiusMultCapital = 1.2f;
    public float radiusMultFarm = 0.8f;
    public float radiusMultMarket = 0.8f;
    public float radiusMultPort = 1f;

    [Header("核心资源基础产出（每结算周期）")] 
    [Tooltip("小镇：均衡基础产出")] public Vector4 resTown = new Vector4(2, 1, 3, 0);
    [Tooltip("普通城：均衡产出")] public Vector4 resNormal = new Vector4(3, 2, 5, 1);
    [Tooltip("都城：金币+军队高")] public Vector4 resCapital = new Vector4(5, 8, 10, 5);
    [Tooltip("农村：粮食超高")] public Vector4 resFarm = new Vector4(10, 1, 4, 0);
    [Tooltip("市集：金币高")] public Vector4 resMarket = new Vector4(2, 7, 6, 0);
    [Tooltip("港口：金币+粮食均衡")] public Vector4 resPort = new Vector4(4, 5, 3, 1);

    [Header("扩展资源基础产出（每结算周期）")] 
    [Tooltip("小镇：少量基础资源")] public NationCityManager.Vector12 resExtendTown = new NationCityManager.Vector12(1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0);
    [Tooltip("普通城：均衡扩展资源")] public NationCityManager.Vector12 resExtendNormal = new NationCityManager.Vector12(2, 2, 1, 0, 1, 1, 2, 0, 0, 1, 0, 1);
    [Tooltip("都城：少量稀有资源")] public NationCityManager.Vector12 resExtendCapital = new NationCityManager.Vector12(3, 3, 2, 1, 2, 2, 3, 1, 0, 1, 1, 2);
    [Tooltip("农村：牲畜+草料+木材高")] public NationCityManager.Vector12 resExtendFarm = new NationCityManager.Vector12(4, 1, 5, 0, 0, 0, 6, 0, 0, 0, 0, 1);
    [Tooltip("市集：布匹+皮革高")] public NationCityManager.Vector12 resExtendMarket = new NationCityManager.Vector12(1, 1, 2, 0, 3, 3, 1, 0, 0, 0, 0, 0);
    [Tooltip("港口：石料+盐矿高")] public NationCityManager.Vector12 resExtendPort = new NationCityManager.Vector12(2, 4, 1, 0, 1, 1, 1, 2, 0, 0, 0, 1);       

    // ====================== 获取缩放 ======================
    public float GetCityScale(CityType type)
    {
        return type switch
        {
            CityType.Town => cityScaleTown,
            CityType.Normal => cityScaleNormal,
            CityType.Capital => cityScaleCapital,
            CityType.Farm => cityScaleFarm,
            CityType.Market => cityScaleMarket,
            CityType.Port => cityScalePort,
            _ => 1f
        };
    }

    // ====================== 获取半径系数 ======================
    public float GetRadiusMult(CityType type)
    {
        return type switch
        {
            CityType.Town => radiusMultTown,
            CityType.Normal => radiusMultNormal,
            CityType.Capital => radiusMultCapital,
            CityType.Farm => radiusMultFarm,
            CityType.Market => radiusMultMarket,
            CityType.Port => radiusMultPort,
            _ => 1f
        };
    }
}
