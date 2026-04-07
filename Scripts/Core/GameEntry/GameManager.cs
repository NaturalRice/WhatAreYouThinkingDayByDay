using UnityEngine;
using System.Collections;
using Game.Game.Nation;
using Game.Game.Terrain;

namespace Game.Core.GameEntry
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        
        // 声明为可序列化字段，允许在Inspector赋值
        [Header("核心管理器")]
        public TerrainManager terrainManager;
        public NationCityManager cityManager;
        public NationResManager resManager;

        // 对外提供只读访问
        public static TerrainManager TerrainManager { get; private set; }
        public static NationCityManager NationCityManager { get; private set; }
        public static NationResManager NationResManager { get; private set; }
        
        private void Awake()
        {
            // 修复单例赋值
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
    
            InitializeSystems();
            StartCoroutine(ResourceGrowthCoroutine());
        }

        private void InitializeSystems()
        {
            // 1. 给静态属性赋值（核心修复）
            TerrainManager = terrainManager;
            NationCityManager = cityManager;
            NationResManager = resManager;

            // 2. 调用各管理器Init（保持原有逻辑）
            if (terrainManager != null) terrainManager.Init();
            if (cityManager != null) cityManager.Init();
            if (resManager != null) resManager.Init();

            // 3. 额外：确保CityManager关联ResManager（兜底）
            if (cityManager != null && resManager != null)
            {
                cityManager.GetComponent<NationCityManager>().resManager = resManager;
            }
        }

        // 资源定时增长协程（示例：每5秒增长一次）
        private IEnumerator ResourceGrowthCoroutine()
        {
            // 修复：等待实例赋值完成，而非静态属性
            yield return new WaitUntil(() => resManager != null && cityManager != null);
    
            while (true)
            {
                NationResManager.GrowAllCityResources();
                NationResManager.CalculateTotalResources();
                yield return new WaitForSeconds(5f);
            }
        }
    }
}