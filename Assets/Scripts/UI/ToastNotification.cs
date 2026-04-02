using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ArchitectureBlueprint.UI
{
    public enum ToastType { Info, Warning, Danger, Success }

    /// <summary>
    /// Shows a sliding toast message at the top of the screen.
    /// Attach to the ToastArea panel. Assign toastPanel and messageText.
    /// The toast slides DOWN into view, holds, then slides back UP.
    /// </summary>
    public class ToastNotification : MonoBehaviour
    {
        [Header("References — must assign these")]
        public RectTransform toastPanel;
        public TextMeshProUGUI messageText;
        public Image backgroundImage;

        [Header("Timing")]
        public float slideInDuration = 0.25f;
        public float holdDuration = 3.0f;
        public float slideOutDuration = 0.2f;

        [Header("Slide positions")]
        public float hiddenY = 120f;   // above screen
        public float shownY = -60f;   // below top edge

        [Header("Colours")]
        public Color infoColour = new Color(0.10f, 0.37f, 0.54f);
        public Color warningColour = new Color(0.48f, 0.29f, 0.04f);
        public Color dangerColour = new Color(0.48f, 0.11f, 0.11f);
        public Color successColour = new Color(0.06f, 0.30f, 0.16f);

        private Coroutine currentToast;
        private bool isInitialised = false;

        private void Start()
        {
            // Make sure toast starts hidden above the screen
            if (toastPanel != null)
            {
                Vector2 pos = toastPanel.anchoredPosition;
                pos.y = hiddenY;
                toastPanel.anchoredPosition = pos;
            }
            isInitialised = true;
        }

        public void Show(string message, ToastType type = ToastType.Info)
        {
            if (toastPanel == null || messageText == null)
            {
                Debug.LogWarning("[Toast] toastPanel or messageText not assigned!");
                return;
            }

            if (currentToast != null) StopCoroutine(currentToast);
            currentToast = StartCoroutine(ShowRoutine(message, type));
        }

        private IEnumerator ShowRoutine(string message, ToastType type)
        {
            // Set content
            messageText.text = message;
            if (backgroundImage != null)
                backgroundImage.color = GetColour(type);

            // Ensure visible
            toastPanel.gameObject.SetActive(true);

            // Slide in — from hiddenY down to shownY
            yield return SlideY(hiddenY, shownY, slideInDuration);

            // Hold
            yield return new WaitForSeconds(holdDuration);

            // Slide out — back up to hiddenY
            yield return SlideY(shownY, hiddenY, slideOutDuration);

            toastPanel.gameObject.SetActive(false);
        }

        private IEnumerator SlideY(float from, float to, float duration)
        {
            float elapsed = 0f;
            Vector2 pos = toastPanel.anchoredPosition;
            while (elapsed < duration)
            {
                pos.y = Mathf.Lerp(from, to, elapsed / duration);
                toastPanel.anchoredPosition = pos;
                elapsed += Time.deltaTime;
                yield return null;
            }
            pos.y = to;
            toastPanel.anchoredPosition = pos;
        }

        private Color GetColour(ToastType type)
        {
            switch (type)
            {
                case ToastType.Warning: return warningColour;
                case ToastType.Danger: return dangerColour;
                case ToastType.Success: return successColour;
                default: return infoColour;
            }
        }
    }
}