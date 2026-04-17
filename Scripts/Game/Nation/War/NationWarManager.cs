using System.Collections.Generic;
using UnityEngine;
using Game.Game.Nation;

public class NationWarManager : MonoBehaviour
{
    public static NationWarManager Instance;

    public float armyMoveSpeed = 0.4f;
    public GameObject armyVisualPrefab; // 你的军团+箭头预制体
    private List<ArmyUnit> _armyUnits = new List<ArmyUnit>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        for (int i = _armyUnits.Count - 1; i >= 0; i--)
        {
            ArmyUnit unit = _armyUnits[i];
            unit.UpdateMove();

            if (unit.visual != null)
            {
                unit.visual.SetPosition(unit.GetCurrentPos());
            }

            if (unit.IsArrived)
            {
                DoBattle(unit);
                if (unit.visual != null) Destroy(unit.visual.gameObject);
                _armyUnits.RemoveAt(i);
            }
        }
    }

    public void SendArmy(NationCityManager owner, CityData from, CityData target, int power)
    {
        if (power <= 0 || from == null || target == null) return;

        from.armyOut -= power;

        ArmyUnit unit = new ArmyUnit
        {
            owner = owner,
            fromCity = from,
            targetCity = target,
            power = power,
            moveSpeed = armyMoveSpeed
        };

        // 生成军团视觉
        if (armyVisualPrefab != null)
        {
            GameObject go = Instantiate(armyVisualPrefab, owner.mapRoot);
            ArmyVisual visual = go.GetComponent<ArmyVisual>();
            visual.SetPosition(from.localPos);
            visual.LookAtTarget(from.localPos, target.localPos);
            unit.visual = visual;
        }

        _armyUnits.Add(unit);
    }

    private void DoBattle(ArmyUnit unit)
    {
        NationCityManager targetNation = GetNationByCity(unit.targetCity);
        if (targetNation == null) return;

        int defense = unit.targetCity.armyOut * 2;

        if (unit.power > defense)
        {
            targetNation.LoseCity(unit.targetCity);
            unit.owner.CaptureCity(unit.targetCity);
            Debug.Log($"【战争胜利】占领城市！");
        }
        else
        {
            Debug.Log("【战争失败】");
        }
    }

    public NationCityManager GetNationByCity(CityData city)
    {
        NationCityManager[] all = FindObjectsOfType<NationCityManager>();
        foreach (var n in all)
        {
            if (n.cityList.Contains(city)) return n;
        }
        return null;
    }
}