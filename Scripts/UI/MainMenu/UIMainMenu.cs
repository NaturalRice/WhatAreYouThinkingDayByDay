using Game.Core.Base;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.UI.MainMenu
{
    public class UIMainMenu : BasePanel
    {
        [Header("按钮")] public Button btnStart;
        public Button btnHelp;
        public Button btnExit;
        public Button btnCloseHelp;

        [Header("面板")] public GameObject panelHelp;

        [Header("音效")] public AudioSource audioSource;
        public AudioClip clickSound;

        [Header("动画")] public float animScale = 1.15f;
        public float animSpeed = 0.1f;

        void Start()
        {
            // 绑定按钮事件
            btnStart.onClick.AddListener(OnStartClick);
            btnHelp.onClick.AddListener(OnHelpClick);
            btnExit.onClick.AddListener(OnExitClick);
            btnCloseHelp.onClick.AddListener(OnCloseHelpClick);

            // 初始化隐藏帮助面板
            panelHelp.SetActive(false);
        }

        // 开始绘制
        void OnStartClick()
        {
            PlayClickSound();
            SceneManager.LoadScene("MapDraw"); // 你的画板场景名
        }

        // 打开帮助
        void OnHelpClick()
        {
            PlayClickSound();
            panelHelp.SetActive(true);
        }

        // 关闭帮助
        void OnCloseHelpClick()
        {
            PlayClickSound();
            panelHelp.SetActive(false);
        }

        // 退出游戏（兼容编辑器 + 打包版本）
        void OnExitClick()
        {
            PlayClickSound();

            // 先延迟0.2秒，让音效播放完再退出
            Invoke(nameof(ExitGame), 0.2f);
        }

        void ExitGame()
        {
#if UNITY_EDITOR
        // 编辑器模式：退出播放模式
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 打包后：真正退出
            Application.Quit();
#endif
        }

        // 播放点击音效
        void PlayClickSound()
        {
            if (audioSource != null && clickSound != null)
                audioSource.PlayOneShot(clickSound);
        }

        // 按钮放大动画（纯原生代码，无插件）
        public void OnPointerEnter(Button btn)
        {
            StopAllCoroutines();
            StartCoroutine(ScaleButton(btn.gameObject, Vector3.one * 1.15f, 0.1f));
        }

        public void OnPointerExit(Button btn)
        {
            StopAllCoroutines();
            StartCoroutine(ScaleButton(btn.gameObject, Vector3.one, 0.1f));
        }

        // 平滑缩放协程
        private System.Collections.IEnumerator ScaleButton(GameObject target, Vector3 targetScale, float duration)
        {
            if (target == null) yield break;

            Vector3 startScale = target.transform.localScale;
            float time = 0;

            while (time < duration)
            {
                time += Time.deltaTime;
                target.transform.localScale = Vector3.Lerp(startScale, targetScale, time / duration);
                yield return null;
            }

            target.transform.localScale = targetScale;
        }
    }
}