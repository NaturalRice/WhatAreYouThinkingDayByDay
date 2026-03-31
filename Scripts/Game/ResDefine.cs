using UnityEngine;

// 资源类型枚举（16种）
public enum ResourceType
{
    Food,    // 粮食
    Gold,    // 金币
    People,  // 人口
    Army,    // 军队
    Wood,    // 木材
    Stone,   // 石料
    Livestock,// 牲畜
    Horse,   // 马匹
    Cloth,   // 布匹
    Leather, // 皮革
    Forage,  // 草料
    Salt,    // 盐矿
    Iron,    // 铁矿
    Copper,  // 铜矿
    GoldOre, // 金矿
    Clay     // 黏土
}

// 资源配置类（绑定贴图）
[CreateAssetMenu(fileName = "ResConfig", menuName = "Game/资源配置")]
public class ResourceConfig : ScriptableObject
{
    public ResourceType resType;
    public Sprite icon; // 资源贴图（拖入你准备的贴图）
}