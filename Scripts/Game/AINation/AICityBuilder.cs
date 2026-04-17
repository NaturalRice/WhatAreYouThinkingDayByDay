using UnityEngine;
using Game.Game.Nation;

public class AICityBuilder : MonoBehaviour
{
    public float aiWarCheckInterval = 8f;
    private float _aiWarTimer = 0;
    
    private NationCityManager _cityMgr;
    private NationResManager _resMgr;
    private AINationManager _root;
    private float _buildTimer;

    public void StartAI(NationCityManager cityMgr, NationResManager resMgr, AINationManager root)
    {
        _cityMgr = cityMgr;
        _resMgr = resMgr;
        _root = root;

        // 🔥 强制立刻建第一座城（100%成功）
        Invoke(nameof(AIFirstCity), 0.5f);
    }

    private void Update()
    {
        // 🔥 第一层防护：核心管理器为空则直接返回
        if (_cityMgr == null || _resMgr == null || _root == null) return;
        if (_cityMgr.cityList.Count == 0) return;

        _buildTimer += Time.deltaTime;
        if (_buildTimer >= _root.aiBuildInterval)
        {
            _buildTimer = 0;
            TryBuildCity();
        }
        
        CheckAIWar();
    }

    // AI 第一座城（必成功）
    private void AIFirstCity()
    {
        // 🔥 加空值校验
        if (_cityMgr == null || _root == null) return;
        
        Vector2 pos = GetRandomValidPosition();

        // 🔥 如果出生在海里，强制换位置
        int retry = 20;
        while (retry > 0 && !_cityMgr.IsLand(pos))
        {
            pos = GetRandomValidPosition();
            retry--;
        }
        
        // 随机城市类型
        CityType type = GetRandomCityType();

        _cityMgr.CreateCity(pos, type);
    }

    private void TryBuildCity()
    {
        if (_cityMgr.cityList.Count < 1) return;

        // 🔥 在自己城市附近生成（100%合法）
        Vector2 basePos = _cityMgr.cityList[0].localPos;
        Vector2 offset = new Vector2(Random.Range(-30f, 30f), Random.Range(-30f, 30f));
        Vector2 finalPos = basePos + offset;

        if (!_cityMgr.IsLand(finalPos)) return;

        // 🔥 随机城市类型（替代原有的固定Normal类型）
        CityType randomType = GetRandomCityType();
        _cityMgr.CreateCity(finalPos, randomType);
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

    private Vector2 GetRandomValidPosition()
    {
        // 🔥 加空值校验
        if (_root == null || _root.mapRoot == null) return Vector2.zero;
        
        Vector2 size = _root.mapRoot.rect.size / 2.8f;
        return new Vector2(Random.Range(-size.x, size.x), Random.Range(-size.y, size.y));
    }
    
    void CheckAIWar()
    {
        // 🔥 第二层防护：核心变量为空则返回
        if (_cityMgr == null || _cityMgr.cityList.Count == 0) return;
        
        _aiWarTimer += Time.deltaTime;
        if (_aiWarTimer < aiWarCheckInterval) return;
        _aiWarTimer = 0;

        // 找邻国
        NationCityManager[] all = FindObjectsOfType<NationCityManager>();
        foreach (var other in all)
        {
            // 🔥 第三层防护：排除空对象 + 自身 + 无城市的国家
            if (other == null || other == _cityMgr || other.cityList.Count == 0) continue;

            // 距离近=邻国
            float d = Vector2.Distance(
                _cityMgr.cityList[0].localPos,
                other.cityList[0].localPos);

            if (d < 120f)
            {
                // 🔥 第四层防护：校验 armyOut 所属对象非空
                if (_cityMgr.cityList[0] == null || other.cityList[0] == null) continue;
                
                // 兵力比对方强 → 宣战+进攻
                int myArmy = _cityMgr.cityList[0].armyOut;
                int theirArmy = other.cityList[0].armyOut;

                if (myArmy > theirArmy * 1.5f)
                {
                    // 🔥 第五层防护：校验 diplomacy 非空（关键修复点！）
                    if (_cityMgr.diplomacy == null)
                    {
                        _cityMgr.diplomacy = new NationDiplomacy(_cityMgr);
                    }
                    _cityMgr.diplomacy.DeclareWar(other);
                    
                    // 🔥 调用战争管理器的SendArmy，而非直接AttackCity（适配现有战争逻辑）
                    if (NationWarManager.Instance != null)
                    {
                        NationWarManager.Instance.SendArmy(
                            _cityMgr, 
                            _cityMgr.cityList[0], 
                            other.cityList[0], 
                            myArmy // 派出当前城市所有军队
                        );
                    }
                    else
                    {
                        // 兼容旧逻辑（备选）
                        _cityMgr.AttackCity(_cityMgr.cityList[0], other.cityList[0], other);
                    }
                }
            }
        }
    }
}