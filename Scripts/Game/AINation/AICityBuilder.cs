using UnityEngine;
using Game.Game.Nation;

/// <summary>
/// AI 自动建城逻辑
/// 完全遵守玩家规则：资源、距离、地形、冷却
/// </summary>
public class AICityBuilder : MonoBehaviour
{
    private NationCityManager _cityMgr;
    private NationResManager _resMgr;
    private AINationManager _root;
    private float _buildTimer;
    
    public void StartAI(NationCityManager cityMgr, NationResManager resMgr, AINationManager root)
    {
        _cityMgr = cityMgr;
        _resMgr = resMgr;
        _root = root;
        Invoke(nameof(AIFirstCity), 1.5f);
    }
    
    private void Update()
    {
        if (_cityMgr == null || _resMgr == null) return;

        _buildTimer += Time.deltaTime;
        if (_buildTimer >= _root.aiBuildInterval)
        {
            _buildTimer = 0;
            TryBuildCity();
        }
    }
    
    // AI 第一座城
    private void AIFirstCity()
    {
        Vector2 pos = GetRandomValidPosition();
        _cityMgr.CreateCity(pos, CityType.Normal);
    }
    
    // AI 自动建城（完全遵守玩家规则）
    private void TryBuildCity()
    {
        if (_cityMgr.cityList.Count >= 5) return;

        Vector2 pos = GetRandomValidPosition();

        // 远离玩家
        if (Vector2.Distance(pos, _root.playerCityManager.cityList[0].localPos) < _root.minDistanceFromPlayer)
            return;

        // 必须是陆地
        if (!_cityMgr.IsLand(pos)) return;

        // 必须靠近自己的城市
        bool canBuild = false;
        foreach (var city in _cityMgr.cityList)
        {
            if (Vector2.Distance(pos, city.localPos) < _cityMgr.minCityDistance)
            {
                canBuild = true;
                break;
            }
        }
        if (!canBuild) return;

        // 随机城市类型
        CityType type = GetRandomCityType();
        if (type == CityType.Port && !_cityMgr.IsLand(pos)) return;

        // 🔥 关键：AI 完全和玩家一样，会检查资源、消耗资源
        _cityMgr.CreateCity(pos, type);
    }
    
    private Vector2 GetRandomValidPosition()
    {
        Vector2 size = _root.mapRoot.rect.size / 2.6f;
        return new Vector2(Random.Range(-size.x, size.x), Random.Range(-size.y, size.y));
    }

    // 随机城市类型
    private CityType GetRandomCityType()
    {
        int r = Random.Range(0, 100);
        if (r < 40) return CityType.Normal;
        if (r < 60) return CityType.Town;
        if (r < 75) return CityType.Farm;
        if (r < 85) return CityType.Market;
        if (r < 95) return CityType.Port;
        return CityType.Capital;
    }
}