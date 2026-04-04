using UnityEngine;

namespace Game.Core.Base
{
    public class ManagerBase<T> : MonoBehaviour where T : ManagerBase<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = (T)this;
                if (this is IGlobalManager)
                    DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public interface IGlobalManager { }
}