using Game.Core.Base;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game.Map
{
    public class MapDrawUINavigator : BasePanel
    {
        [Header("按钮")] public Button btnBack;
        public Button btnContinue;

        [Header("音效")] public AudioSource audioSource;
        public AudioClip clickSound;

        [Header("动画")] public float animScale = 1.15f;
        public float animSpeed = 0.1f;

        void Start()
        {
            // 绑定按钮事件
            btnBack.onClick.AddListener(OnBackClick);
            btnContinue.onClick.AddListener(OnContinueClick);
        }

        // 返回主菜单
        void OnBackClick()
        {
            PlayClickSound();
            Invoke(nameof(LoadMainMenu), 0.2f);
        }

        // 进入游戏场景
        void OnContinueClick()
        {
            PlayClickSound();
            Invoke(nameof(LoadGameScene), 0.2f);
        }

        void LoadMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        void LoadGameScene()
        {
            MapDrawCore drawCore = FindObjectOfType<MapDrawCore>();
            MapFillSystem fillSystem = FindObjectOfType<MapFillSystem>();

            if (drawCore != null)
            {
                // 保存地图
                MapGlobalData.savedMapTexture = drawCore.GetDrawTexture();
            }

            if (fillSystem != null)
            {
                // 保存陆地/海洋颜色（关键！）
                MapGlobalData.landColor = fillSystem.landColor;
                MapGlobalData.seaColor = fillSystem.seaColor;
            }

            SceneManager.LoadScene("GameScene");
        }

        // 播放音效
        void PlayClickSound()
        {
            if (audioSource != null && clickSound != null)
                audioSource.PlayOneShot(clickSound);
        }

        // 按钮放大动画（无插件版）
        public void OnPointerEnter(Button btn)
        {
            StopAllCoroutines();
            StartCoroutine(ScaleButton(btn.gameObject, Vector3.one * animScale, animSpeed));
        }

        public void OnPointerExit(Button btn)
        {
            StopAllCoroutines();
            StartCoroutine(ScaleButton(btn.gameObject, Vector3.one, animSpeed));
        }

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