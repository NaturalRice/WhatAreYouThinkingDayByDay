using Game.Game.Nation;
using UnityEngine;

public class ArmyUnit
{
    public NationCityManager owner;
    public CityData fromCity;
    public CityData targetCity;
    public int power;
    public float progress;
    public float moveSpeed;
    public ArmyVisual visual;

    public bool IsArrived => progress >= 1f;

    public void UpdateMove()
    {
        progress += moveSpeed * Time.deltaTime;
    }

    public Vector2 GetCurrentPos()
    {
        return Vector2.Lerp(fromCity.localPos, targetCity.localPos, progress);
    }
}