using System;

namespace Game.Core.GameEntry
{
    /// <summary>
    /// 全局事件中心，用于系统间解耦通信
    /// </summary>
    public static class EventManager
    {
        // 城市相关事件
        public static Action OnCityCreated;    // 城市创建
        public static Action OnCitySelected;   // 城市选中
        public static Action OnCityDestroyed;  // 城市销毁
        
        // 资源相关事件
        public static Action OnResourceUpdated; // 资源更新
        
        // 地图相关事件
        public static Action OnMapSaved;        // 地图保存
        public static Action OnTerrainChanged;  // 地形修改
        
        // 国家相关事件
        public static Action OnNationCreated;  // 国家创建
    }
}