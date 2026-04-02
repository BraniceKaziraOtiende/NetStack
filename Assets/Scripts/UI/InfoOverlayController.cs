using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ArchitectureBlueprint.Infrastructure;

namespace ArchitectureBlueprint.UI
{
    
    /// The info panel that appears when a student taps a placed node.
    /// Shows: node name, type, current load bar, educational description, and a tip.
    /// Follows the node in world space (world-space canvas attached to node).
    /// Driven by ARInteractionHandler detecting taps.

    public class InfoOverlayController : MonoBehaviour
    {
        [Header("Panel References")]
        public GameObject overlayPanel;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI tipText;
        public Slider loadBar;
        public Image loadBarFill;

        [Header("Load Bar Colours")]
        public Color healthyColour = new Color(0.12f, 0.62f, 0.46f);
        public Color warningColour = new Color(0.94f, 0.62f, 0.15f);
        public Color dangerColour = new Color(0.87f, 0.30f, 0.30f);

        [Header("Animation")]
        public float fadeInDuration = 0.2f;

        private InfrastructureNode currentNode;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = overlayPanel?.GetComponent<CanvasGroup>();
            Hide();
        }

        private void Update()
        {
            // Keep the overlay facing the camera (billboard effect in world space)
            if (overlayPanel != null && overlayPanel.activeSelf && Camera.main != null)
            {
                overlayPanel.transform.LookAt(
                    overlayPanel.transform.position + Camera.main.transform.forward
                );
            }

            // Live-update the load bar while the overlay is open
            if (currentNode != null && overlayPanel != null && overlayPanel.activeSelf)
            {
                RefreshLoadBar(currentNode.currentLoad);
                tipText.text = currentNode.GetEducationalTip();
            }
        }

        /// Called by ARInteractionHandler when the student taps a node.
        public void ShowForNode(InfrastructureNode node)
        {
            currentNode = node;

            titleText.text = node.nodeName + $" <size=70%><color=#888888>[{node.nodeType}]</color></size>";
            descriptionText.text = node.GetDescription();
            tipText.text = node.GetEducationalTip();
            RefreshLoadBar(node.currentLoad);

            overlayPanel.SetActive(true);

            // Position above the node
            overlayPanel.transform.position = node.transform.position + Vector3.up * 0.25f;

            if (canvasGroup != null)
            {
                StopAllCoroutines();
                StartCoroutine(FadeIn());
            }
        }

        public void Hide()
        {
            currentNode = null;
            if (overlayPanel != null) overlayPanel.SetActive(false);
        }

        private void RefreshLoadBar(float load)
        {
            if (loadBar != null) loadBar.value = load;
            if (loadBarFill != null)
            {
                loadBarFill.color = load < 0.6f ? healthyColour
                                  : load < 0.85f ? warningColour
                                  : dangerColour;
            }
        }

        private System.Collections.IEnumerator FadeIn()
        {
            canvasGroup.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                canvasGroup.alpha = elapsed / fadeInDuration;
                elapsed += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
    }
}