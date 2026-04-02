using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace ArchitectureBlueprint.UI
{
    
    /// Drives the NetStack splash screen.
    /// Fades in logo + tagline, holds, then loads Main.unity.
    /// Attach to the root GameObject in Splash.unity.
    /// Canvas Group wraps the logo + tagline text objects.

    public class SplashController : MonoBehaviour
    {
        [Header("References")]
        public CanvasGroup logoGroup;
        public TextMeshProUGUI appName;
        public TextMeshProUGUI tagline;

        [Header("Timing (seconds)")]
        public float fadeInDuration = 0.8f;
        public float holdDuration = 1.4f;
        public float fadeOutDuration = 0.5f;

        private void Start()
        {
            if (logoGroup != null) logoGroup.alpha = 0f;
            StartCoroutine(PlaySplash());
        }

        private IEnumerator PlaySplash()
        {
            // Fade in
            yield return Fade(logoGroup, 0f, 1f, fadeInDuration);

            // Hold
            yield return new WaitForSeconds(holdDuration);

            // Fade out
            yield return Fade(logoGroup, 1f, 0f, fadeOutDuration);

            SceneManager.LoadScene("Main");
        }

        private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            if (group == null) yield break;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                group.alpha = Mathf.Lerp(from, to, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            group.alpha = to;
        }
    }
}