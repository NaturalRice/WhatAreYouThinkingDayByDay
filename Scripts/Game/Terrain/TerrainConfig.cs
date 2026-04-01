using UnityEngine;

namespace Game.Game.Terrain
{
// 地形类型
    public enum TerrainType
    {
        Ocean, // 海洋：不可建造，阻挡扩张
        Mountain, // 山脉：不可建造，阻挡扩张
        Plains, // 平原：正常建造，正常产出
        Desert, // 沙漠：可建造，粮食-50%，石料+50%
        Forest, // 森林：木材+100%，草料+80%
        River, // 河流：粮食+50%，布匹+50%
        SaltMine, // 盐矿：盐产量翻倍
        IronMine, // 铁矿：铁矿翻倍
        GoldMine, // 金矿：金矿翻倍
        ClayHill // 黏土：黏土产量翻倍
    }

// 地形配置（可在编辑器创建配置文件）
// 文件名必须是 TerrainConfig.cs
    [CreateAssetMenu(fileName = "NewTerrainConfig", menuName = "Terrain/Terrain Config")]
    public class TerrainConfig : ScriptableObject
    {
        [Header("基础信息")] public TerrainType terrainType;
        public string terrainName;
        public Color terrainColor;

        [Header("建造规则")] public bool canBuildCity = true;
        public bool blockTerritory = false;

        [Header("资源产出加成 %")] public int foodBonus = 0;
        public int goldBonus = 0;
        public int woodBonus = 0;
        public int stoneBonus = 0;
        public int livestockBonus = 0;
        public int horseBonus = 0;
        public int clothBonus = 0;
        public int leatherBonus = 0;
        public int forageBonus = 0;
        public int saltBonus = 0;
        public int ironBonus = 0;
        public int copperBonus = 0;
        public int goldOreBonus = 0;
        public int clayBonus = 0;
    }
}