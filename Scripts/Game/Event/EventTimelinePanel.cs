using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Game.Game.Event
{
    public class EventTimelinePanel : MonoBehaviour
    {
        [Header("UI 绑定")] public Button btnAddEvent;
        public Button btnConfirm;
        public Button btnClose;
        public RectTransform timelineContent;
        public GameObject eventItemPrefab;

        [Header("事件模板")] private string[] eventTemplates =
        {
            "建国", "天灾", "丰收", "战争", "文化繁荣", "科技突破", "革命"
        };

        private List<GameObject> eventItems = new List<GameObject>();

        void Start()
        {
            btnAddEvent.onClick.AddListener(AddNewEventItem);
            btnClose.onClick.AddListener(() => gameObject.SetActive(false));
            btnConfirm.onClick.AddListener(OnSaveTimeline);

            gameObject.SetActive(false);
        }

        // 添加一个事件
        void AddNewEventItem()
        {
            GameObject item = Instantiate(eventItemPrefab, timelineContent);
            Text text = item.GetComponentInChildren<Text>();
            text.text = eventTemplates[Random.Range(0, eventTemplates.Length)];
            eventItems.Add(item);
        }

        // 保存时间轴
        void OnSaveTimeline()
        {
            Debug.Log("时间轴已保存，共 " + eventItems.Count + " 个事件");
            gameObject.SetActive(false);
        }

        // 打开面板
        public void OpenPanel()
        {
            gameObject.SetActive(true);
        }

        // 保存时间轴（给GameSceneUIManager调用）
        public void SaveTimeline()
        {
            Debug.Log($"✅ 保存事件：共 {eventItems.Count} 个历史事件");
        }
    }
}