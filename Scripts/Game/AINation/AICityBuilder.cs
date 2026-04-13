using UnityEngine;
using Game.Game.Nation;

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

        // 🔥 强制立刻建第一座城（100%成功）
        Invoke(nameof(AIFirstCity), 0.5f);
    }

    private void Update()
    {
        if (_cityMgr == null || _resMgr == null) return;
        if (_cityMgr.cityList.Count == 0) return;

        _buildTimer += Time.deltaTime;
        if (_buildTimer >= _root.aiBuildInterval)
        {
            _buildTimer = 0;
            TryBuildCity();
        }
    }

    // AI 第一座城（必成功）
    private void AIFirstCity()
    {
        Vector2 pos = GetRandomValidPosition();

        // 🔥 如果出生在海里，强制换位置
        int retry = 20;
        while (retry > 0 && !_cityMgr.IsLand(pos))
        {
            pos = GetRandomValidPosition();
            retry--;
        }

        _cityMgr.CreateCity(pos, CityType.Normal);
    }

    private void TryBuildCity()
    {
        if (_cityMgr.cityList.Count < 1) return;

        // 🔥 在自己城市附近生成（100%合法）
        Vector2 basePos = _cityMgr.cityList[0].localPos;
        Vector2 offset = new Vector2(Random.Range(-30f, 30f), Random.Range(-30f, 30f));
        Vector2 finalPos = basePos + offset;

        if (!_cityMgr.IsLand(finalPos)) return;

        _cityMgr.CreateCity(finalPos, CityType.Normal);
    }

    private Vector2 GetRandomValidPosition()
    {
        Vector2 size = _root.mapRoot.rect.size / 2.8f;
        return new Vector2(Random.Range(-size.x, size.x), Random.Range(-size.y, size.y));
    }
}