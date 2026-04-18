using Game.Game.Terrain;
using UnityEngine;

namespace Game.Game.Nation
{
    [System.Serializable]
    public class CityData
    {
        public GameObject go;
        public CityType type;
        public Vector2 localPos;
        public RectTransform rt;
            
        // 🔥 加这一行！！！
        public TerrainConfig terrainConfig;

        // 核心资源产出
        public int foodOut;
        public int goldOut;
        public int peopleOut;
        public int armyOut;
        // 当前资源数量
        public int currentArmy;
        // 扩展资源产出
        public int woodOut;
        public int stoneOut;
        public int livestockOut;
        public int horseOut;
        public int clothOut;
        public int leatherOut;
        public int forageOut;
        public int saltOut;
        public int ironOut;
        public int copperOut;
        public int goldOreOut;
        public int clayOut;
    }
}