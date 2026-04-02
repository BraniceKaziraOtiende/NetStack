using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArchitectureBlueprint.UI
{
    public class InfoPanelController : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject infoPanel;
        public CanvasGroup panelGroup;
        public float fadeDuration = 0.35f;

        [Header("Buttons")]
        public Button startButton;
        public Button skipButton;

        [Header("Pause game while panel shows")]
        public MonoBehaviour placementManager;
        public MonoBehaviour toolbarManager;

        private void Start()
        {
            if (startButton != null) startButton.onClick.AddListener(HidePanel);
            if (skipButton != null) skipButton.onClick.AddListener(HidePanel);

            // Always show panel on scene load
            if (infoPanel != null) infoPanel.SetActive(true);
            if (panelGroup != null) panelGroup.alpha = 1f;
            EnableGame(false);
        }

        public void HidePanel()
        {
            StartCoroutine(FadeAndHide());
        }

        public void ReShowPanel()
        {
            if (infoPanel != null) infoPanel.SetActive(true);
            if (panelGroup != null) panelGroup.alpha = 0f;
            EnableGame(false);
            StartCoroutine(Fade(0f, 1f));
        }

        private void EnableGame(bool enabled)
        {
            if (placementManager != null) placementManager.enabled = enabled;
            if (toolbarManager != null) toolbarManager.enabled = enabled;
        }

        private IEnumerator Fade(float from, float to)
        {
            if (panelGroup == null) yield break;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                panelGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            panelGroup.alpha = to;
        }

        private IEnumerator FadeAndHide()
        {
            yield return Fade(1f, 0f);
            if (infoPanel != null) infoPanel.SetActive(false);
            EnableGame(true);
        }
    }
}