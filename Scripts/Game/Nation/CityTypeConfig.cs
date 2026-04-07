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
}
