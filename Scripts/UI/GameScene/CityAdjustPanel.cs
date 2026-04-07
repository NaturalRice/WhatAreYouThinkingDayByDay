using UnityEngine;
using UnityEngine.UI;
using Game.Game.Nation;

public class CityAdjustPanel : MonoBehaviour
{
    [Header("配置文件")]
    public CityTypeConfig config;

    [Header("小镇")]
    public Slider scaleTown;
    public Slider radiusTown;

    [Header("普通城")]
    public Slider scaleNormal;
    public Slider radiusNormal;

    [Header("都城")]
    public Slider scaleCapital;
    public Slider radiusCapital;

    [Header("农村")]
    public Slider scaleFarm;
    public Slider radiusFarm;

    [Header("市集")]
    public Slider scaleMarket;
    public Slider radiusMarket;

    [Header("港口")]
    public Slider scalePort;
    public Slider radiusPort;

    void Start()
    {
        LoadConfig();
        BindSliders();
    }

    void LoadConfig()
    {
        scaleTown.value = config.cityScaleTown;
        radiusTown.value = config.radiusMultTown;

        scaleNormal.value = config.cityScaleNormal;
        radiusNormal.value = config.radiusMultNormal;

        scaleCapital.value = config.cityScaleCapital;
        radiusCapital.value = config.radiusMultCapital;

        scaleFarm.value = config.cityScaleFarm;
        radiusFarm.value = config.radiusMultFarm;

        scaleMarket.value = config.cityScaleMarket;
        radiusMarket.value = config.radiusMultMarket;

        scalePort.value = config.cityScalePort;
        radiusPort.value = config.radiusMultPort;
    }

    void BindSliders()
    {
        scaleTown.onValueChanged.AddListener(v => config.cityScaleTown = v);
        radiusTown.onValueChanged.AddListener(v => config.radiusMultTown = v);

        scaleNormal.onValueChanged.AddListener(v => config.cityScaleNormal = v);
        radiusNormal.onValueChanged.AddListener(v => config.radiusMultNormal = v);

        scaleCapital.onValueChanged.AddListener(v => config.cityScaleCapital = v);
        radiusCapital.onValueChanged.AddListener(v => config.radiusMultCapital = v);

        scaleFarm.onValueChanged.AddListener(v => config.cityScaleFarm = v);
        radiusFarm.onValueChanged.AddListener(v => config.radiusMultFarm = v);

        scaleMarket.onValueChanged.AddListener(v => config.cityScaleMarket = v);
        radiusMarket.onValueChanged.AddListener(v => config.radiusMultMarket = v);

        scalePort.onValueChanged.AddListener(v => config.cityScalePort = v);
        radiusPort.onValueChanged.AddListener(v => config.radiusMultPort = v);
    }
}