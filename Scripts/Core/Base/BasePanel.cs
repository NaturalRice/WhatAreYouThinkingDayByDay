using UnityEngine;
namespace Game.Core.Base
{
    /// <summary>
    /// 所有UI面板的基类，统一生命周期
    /// </summary>
    public abstract class BasePanel : MonoBehaviour
    {
        // 面板显示
        public virtual void Show()
        {
            gameObject.SetActive(true);
            OnShow();
        }

        // 面板隐藏
        public virtual void Hide()
        {
            gameObject.SetActive(false);
            OnHide();
        }

        // 面板初始化（仅调用一次）
        public virtual void Init() { }

        // 面板刷新（按需重写）
        public virtual void Refresh() { }

        // 显示后回调
        protected virtual void OnShow() { }

        // 隐藏后回调
        protected virtual void OnHide() { }

        // 销毁时回调
        protected virtual void OnDestroy() { }
    }
}