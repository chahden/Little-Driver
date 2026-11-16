using LittleDriver.Driving;
using LittleDriver.Game;
using UnityEngine;
using UnityEngine.UI;

namespace LittleDriver.UI
{
    /// <summary>The seat-belt mini game panel, which controls are visible, and the driver's belt portrait. No text: guidance is spoken.</summary>
    public sealed class Lvl1HUD : MonoBehaviour
    {
        [SerializeField] private Lvl1Car car;
        [SerializeField] private StarScore score;
        [SerializeField] private GameObject seatBeltPanel;
        [SerializeField] private GameObject drivingControls;
        [Tooltip("Seconds the belt panel stays up after the click, so the child sees it worked.")]
        [SerializeField, Min(0f)] private float beltPanelLinger = 0.65f;

        [Header("Driver seat-belt portrait")]
        [SerializeField] private Image driverPortrait;
        [SerializeField] private Image driverBorder;
        [SerializeField] private Outline driverGlow;
        [SerializeField] private Sprite driverWithoutBelt;
        [SerializeField] private Sprite driverWithBelt;

        private float buckledAt = -1f;
        private bool seatBeltOpen;

        /// <summary>True while the seat-belt mini game is open and the car is not yet buckled.</summary>
        public bool IsSeatBeltOpen => seatBeltOpen && !car.IsBuckled;

        private void OnEnable() => car.Restarted += ResetUI;
        private void OnDisable() => car.Restarted -= ResetUI;

        private void Update()
        {
            UpdateDriverPortrait();
            if (car.IsBuckled && buckledAt < 0f) buckledAt = Time.unscaledTime;
            // The belt mini game opens only when the driver portrait is tapped, and closes shortly after the click.
            bool showBelt = car.IsBuckled ? seatBeltOpen && Time.unscaledTime - buckledAt < beltPanelLinger : seatBeltOpen;
            seatBeltPanel.SetActive(showBelt);
            drivingControls.SetActive(!showBelt && !car.IsFinished && !score.IsOutOfStars);
        }

        /// <summary>Opens the seat-belt mini game (driver portrait button).</summary>
        public void OpenSeatBelt()
        {
            if (!car.IsBuckled) seatBeltOpen = true;
        }

        private void ResetUI()
        {
            buckledAt = -1f;
            seatBeltOpen = false;
        }

        private void UpdateDriverPortrait()
        {
            bool buckled = car.IsBuckled;
            driverPortrait.sprite = buckled ? driverWithBelt : driverWithoutBelt;
            float pulse = buckled ? 1f : 0.8f + 0.2f * Mathf.Sin(Time.unscaledTime * 4f);
            Color colour = buckled ? new Color(0.12f, 0.8f, 0.3f) : new Color(1f, 0.12f, 0.16f);
            driverBorder.color = colour * new Color(pulse, pulse, pulse, 1f);
            driverGlow.effectColor = new Color(colour.r, colour.g, colour.b, buckled ? 0.35f : 0.3f + 0.25f * pulse);
            // Gentle bounce so the child notices the portrait is the button to press.
            bool invite = !buckled && !seatBeltOpen;
            driverBorder.rectTransform.localScale = Vector3.one * (invite ? 1f + 0.06f * Mathf.Sin(Time.unscaledTime * 5f) : 1f);
        }
    }
}
