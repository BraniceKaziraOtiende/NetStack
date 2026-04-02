using UnityEngine;
using TMPro;

namespace ArchitectureBlueprint.UI
{
  
    /// Updates the 4 stat cards in the top stats bar.
    /// Attach to the StatsBar panel.
    /// Assign all 4 value label TMP references in Inspector.
   
    public class StatsPanel : MonoBehaviour
    {
        [Header("Value labels — drag TMP ValueLabel from each StatCard here")]
        public TextMeshProUGUI usersValueLabel;
        public TextMeshProUGUI pktsValueLabel;
        public TextMeshProUGUI responseValueLabel;
        public TextMeshProUGUI serversValueLabel;

        private void Start()
        {
            // Show zeroes immediately so fields are never blank
            UpdateStats(0, 0f, 0f);
            UpdateServerCount(0);
        }

        /// Call this every frame from LoadSimulator.
        
        public void UpdateStats(int users, float pktsPerSecond, float responseMs)
        {
            if (usersValueLabel != null)
                usersValueLabel.text = users.ToString("N0");

            if (pktsValueLabel != null)
                pktsValueLabel.text = Mathf.RoundToInt(pktsPerSecond).ToString();

            if (responseValueLabel != null)
            {
                if (responseMs <= 0f)
                    responseValueLabel.text = "—";
                else if (responseMs < 1000f)
                    responseValueLabel.text = responseMs.ToString("F0") + "ms";
                else
                    responseValueLabel.text = (responseMs / 1000f).ToString("F1") + "s";
            }
        }

        public void UpdateServerCount(int count)
        {
            if (serversValueLabel != null)
                serversValueLabel.text = count.ToString();
        }
    }
}