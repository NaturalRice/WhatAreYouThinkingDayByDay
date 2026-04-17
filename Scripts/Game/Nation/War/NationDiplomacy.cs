using System.Collections.Generic;
using UnityEngine;

namespace Game.Game.Nation
{
    [System.Serializable]
    public class NationDiplomacy
    {
        public NationCityManager owner;

        // 敌对国家列表
        public List<NationCityManager> enemies = new List<NationCityManager>();

        public NationDiplomacy(NationCityManager owner)
        {
            this.owner = owner;
        }

        // 宣战
        public void DeclareWar(NationCityManager target)
        {
            if (!enemies.Contains(target))
                enemies.Add(target);
        }

        // 和谈
        public void MakePeace(NationCityManager target)
        {
            enemies.Remove(target);
        }

        // 是否在战争中
        public bool IsAtWar(NationCityManager target)
        {
            return enemies.Contains(target);
        }
    }
}