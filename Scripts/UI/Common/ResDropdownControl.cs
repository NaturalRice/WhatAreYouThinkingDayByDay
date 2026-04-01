using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Common
{
    public class ResDropdownControl : MonoBehaviour
    {
        public Button btnDropdown;
        public GameObject panelExtended;
        public Image imgArrow; // 下拉箭头图标
        public Sprite spriteDown; // 向下箭头
        public Sprite spriteUp; // 向上箭头

        private bool isExtended = false;

        void Start()
        {
            btnDropdown.onClick.AddListener(ToggleDropdown);
            // 默认收起扩展区
            panelExtended.SetActive(false);
            imgArrow.sprite = spriteDown;
        }

        void ToggleDropdown()
        {
            isExtended = !isExtended;
            panelExtended.SetActive(isExtended);
            // 切换箭头图标
            imgArrow.sprite = isExtended ? spriteUp : spriteDown;
            // 平滑动画（可选，增强视觉效果）
            StartCoroutine(AnimatePanel());
        }

        // 平滑展开/收起动画
        private System.Collections.IEnumerator AnimatePanel()
        {
            RectTransform rt = panelExtended.GetComponent<RectTransform>();
            float targetHeight = isExtended ? rt.sizeDelta.y : 0;
            float startHeight = rt.sizeDelta.y;
            float duration = 0.2f;
            float timer = 0;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, timer / duration);
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Lerp(startHeight, targetHeight, t));
                yield return null;
            }

            rt.sizeDelta = new Vector2(rt.sizeDelta.x, targetHeight);
        }
    }
}