using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

namespace ArchitectureBlueprint.UI
{
    public class NodeButtonController : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Node Identity")]
        public Infrastructure.NodeType nodeType;
        public string displayName = "Server";
        public string subLabel = "Handles logic";

        [Header("Theme")]
        public Color themeColor = new Color(0.22f, 0.54f, 0.96f);
        public Color darkTint = new Color(0.08f, 0.20f, 0.43f);
        public Sprite nodeIcon;

        [Header("UI References — all optional")]
        public Image backgroundImage;
        public Image glowLineImage;
        public Image iconBgImage;
        public Image iconImage;
        public TextMeshProUGUI nameLabel;
        public TextMeshProUGUI subLabelText;
        public GameObject activeDot;
        public GameObject lockIcon;

        [Header("Tooltip")]
        public string lockedTooltip = "Place a Load Balancer first.";

        public enum ButtonState { Default, Active, Locked }
        private ButtonState currentState = ButtonState.Default;
        private bool isHovered = false;

        private const float BG_DEFAULT_ALPHA = 0.22f;
        private const float BG_ACTIVE_ALPHA = 0.40f;
        private const float BG_LOCKED_ALPHA = 0.06f;
        private const float BG_HOVER_BOOST = 0.10f;
        private const float LOCKED_GREY = 0.35f;

        public System.Action<NodeButtonController> onClicked;

        private void Start()
        {
            ApplyTheme();
            SetState(ButtonState.Default);
        }

        private void ApplyTheme()
        {
            if (glowLineImage != null)
                glowLineImage.color = themeColor;

            if (iconBgImage != null)
                iconBgImage.color = new Color(themeColor.r, themeColor.g, themeColor.b, 0.2f);

            if (iconImage != null)
            {
                if (nodeIcon != null) iconImage.sprite = nodeIcon;
                iconImage.color = themeColor;
            }

            if (nameLabel != null)
            {
                nameLabel.text = displayName;
                nameLabel.color = new Color(
                    Mathf.Clamp01(themeColor.r * 1.3f),
                    Mathf.Clamp01(themeColor.g * 1.3f),
                    Mathf.Clamp01(themeColor.b * 1.3f), 1f);
            }

            if (subLabelText != null)
            {
                subLabelText.text = subLabel;
                subLabelText.color = new Color(themeColor.r, themeColor.g, themeColor.b, 0.7f);
            }

            if (activeDot != null)
            {
                Image img = activeDot.GetComponent<Image>();
                if (img != null) img.color = themeColor;
            }
        }

        public void SetActive(bool active)
        {
            if (currentState == ButtonState.Locked) return;
            SetState(active ? ButtonState.Active : ButtonState.Default);
        }

        public void SetLocked(bool locked)
        {
            SetState(locked ? ButtonState.Locked : ButtonState.Default);
        }

        public bool IsLocked => currentState == ButtonState.Locked;
        public bool IsActive => currentState == ButtonState.Active;

        private void SetState(ButtonState newState)
        {
            currentState = newState;
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            switch (currentState)
            {
                case ButtonState.Default:
                    SetCardAlpha(BG_DEFAULT_ALPHA + (isHovered ? BG_HOVER_BOOST : 0f));
                    SetOverallAlpha(1f);
                    SetBorderWidth(1.5f);
                    if (activeDot != null) activeDot.SetActive(false);
                    if (lockIcon != null) lockIcon.SetActive(false);
                    break;

                case ButtonState.Active:
                    SetCardAlpha(BG_ACTIVE_ALPHA);
                    SetOverallAlpha(1f);
                    SetBorderWidth(2.5f);
                    if (activeDot != null) activeDot.SetActive(true);
                    if (lockIcon != null) lockIcon.SetActive(false);
                    StopAllCoroutines();
                    StartCoroutine(PulseOnSelect());
                    break;

                case ButtonState.Locked:
                    SetCardAlpha(BG_LOCKED_ALPHA);
                    SetOverallAlpha(LOCKED_GREY);
                    SetBorderWidth(1f);
                    if (activeDot != null) activeDot.SetActive(false);
                    if (lockIcon != null) lockIcon.SetActive(true);
                    break;
            }
        }

        private void SetCardAlpha(float alpha)
        {
            if (backgroundImage == null) return;
            Color c = darkTint;
            c.a = Mathf.Clamp01(alpha);
            backgroundImage.color = c;
        }

        private void SetOverallAlpha(float alpha)
        {
            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = alpha;
            cg.interactable = alpha > 0.5f;
            cg.blocksRaycasts = alpha > 0.5f;
        }

        private void SetBorderWidth(float width)
        {
            Outline outline = GetComponent<Outline>();
            if (outline != null)
                outline.effectDistance = new Vector2(width, -width);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (currentState == ButtonState.Locked)
            {
                ToastNotification toast = FindObjectOfType<ToastNotification>();
                toast?.Show(lockedTooltip, ToastType.Warning);
                StartCoroutine(ShakeAnimation());
                return;
            }
            onClicked?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentState == ButtonState.Default)
            {
                isHovered = true;
                RefreshVisuals();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            if (currentState == ButtonState.Default)
                RefreshVisuals();
        }

        private IEnumerator PulseOnSelect()
        {
            float duration = 0.18f;
            float elapsed = 0f;
            Vector3 original = transform.localScale;
            Vector3 peak = original * 1.08f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(original, peak, t * (2f - t));
                elapsed += Time.deltaTime;
                yield return null;
            }
            elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(peak, original, t * (2f - t));
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localScale = original;
        }

        private IEnumerator ShakeAnimation()
        {
            float duration = 0.3f;
            float elapsed = 0f;
            float intensity = 6f;
            Vector3 original = transform.localPosition;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float offset = Mathf.Sin(t * Mathf.PI * 6f) * intensity * (1f - t);
                transform.localPosition = original + new Vector3(offset, 0f, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.localPosition = original;
        }
    }
}