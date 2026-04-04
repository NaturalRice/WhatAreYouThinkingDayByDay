using UnityEngine;
using System.Collections;
using Game.Game.Nation;
using Game.Game.Terrain;

namespace Game.Core.GameEntry
{
    public class GameManager : MonoBehaviour
    {
        // 声明为可序列化字段，允许在Inspector赋值
        [Header("核心管理器")]
        [SerializeField] private TerrainManager _terrainManager;
        [SerializeField] private NationCityManager _nationCityManager;
        [SerializeField] private NationResManager _nationResManager;

        // 对外提供只读访问
        public static TerrainManager TerrainManager { get; private set; }
        public static NationCityManager NationCityManager { get; private set; }
        public static NationResManager NationResManager { get; private set; }

        private void Awake()
        {
            InitializeSystems();
            // 启动资源定时增长协程
            StartCoroutine(ResourceGrowthCoroutine());
        }

        private void InitializeSystems()
        {
            // 赋值单例+空值校验
            TerrainManager = _terrainManager;
            if (TerrainManager == null)
            {
                Debug.LogError("[GameManager] TerrainManager 未赋值！");
                return;
            }

            NationCityManager = _nationCityManager;
            if (NationCityManager == null)
            {
                Debug.LogError("[GameManager] NationCityManager 未赋值！");
                return;
            }

            NationResManager = _nationResManager;
            if (NationResManager == null)
            {
                Debug.LogError("[GameManager] NationResManager 未赋值！");
                return;
            }

            // 初始化各管理器
            //TerrainManager.Init();
            //NationCityManager.Init();
            NationResManager.Init();
        }

        // 资源定时增长协程（示例：每5秒增长一次）
        private IEnumerator ResourceGrowthCoroutine()
        {
            // 避免初始化未完成就执行
            yield return new WaitUntil(() => NationResManager != null && NationCityManager != null);
            
            while (true)
            {
                // 触发资源增长逻辑
                NationResManager.GrowAllCityResources();
                // 刷新资源总和显示
                NationResManager.CalculateTotalResources();
                // 间隔时间（可配置）
                yield return new WaitForSeconds(5f);
            }
        }
    }
}