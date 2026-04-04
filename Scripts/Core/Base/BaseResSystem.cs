using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 资源基类
public abstract class BaseResSystem : MonoBehaviour
{
    // 抽象方法：初始化资源
    public abstract void Init();
    // 抽象方法：计算资源总和
    public abstract void CalculateTotalResources();
    // 虚方法：资源增长（可重写）
    public virtual void GrowAllCityResources()
    {
        Debug.Log("基础资源增长逻辑");
    }
}
