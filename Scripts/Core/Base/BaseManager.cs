using UnityEngine;

namespace Game.Core.Base
{
    public class BaseManager: MonoBehaviour
    {
        public virtual void Init()
        {
            
        }
        
        public interface IGlobalManager { }
        
        // 抽象方法：计算资源总和
        public void CalculateTotalResources(){}
        // 虚方法：资源增长（可重写）
        public virtual void GrowAllCityResources()
        {
            Debug.Log("基础资源增长逻辑");
        }
    }

   
}