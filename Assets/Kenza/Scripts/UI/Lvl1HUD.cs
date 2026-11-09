using LittleDriver.Driving;
using LittleDriver.Game;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LittleDriver.UI
{
    /// <summary>Speed, the driver's belt portrait, which controls are visible, and the feedback line.</summary>
    public sealed class Lvl1HUD : MonoBehaviour
    {
        [SerializeField] private Lvl1Car car;
        [SerializeField] private CarHorn horn;
        [SerializeField] private StarScore score;
        [SerializeField] private GameObject seatBeltPanel;
        [SerializeField] private GameObject drivingControls;
        [SerializeField] private TMP_Text speedLabel;
        [SerializeField] private TMP_Text feedbackLabel;
        [SerializeField] private TMP_Text seatBeltInstruction;
        [Tooltip("Seconds the belt panel stays up after the click, so the child sees it worked.")]
        [SerializeField, Min(0f)] private float beltPanelLinger = 0.65f;
        [FormerlySerializedAs("penaltyMessageSeconds")]
        [SerializeField, Min(0.5f)] private float messageSeconds = 3f;

        [Header("Driver seat-belt portrait")]
        [SerializeField] private Image driverPortrait;
        [SerializeField] private Image driverBorder;
        [SerializeField] private Outline driverGlow;
        [SerializeField] private Sprite driverWithoutBelt;
        [SerializeField] private Sprite driverWithBelt;

        private float buckledAt = -1f;
        private bool seatBeltOpen;
        private string message;
        private float messageUntil;

        private void OnEnable()
        {
            car.Restarted += ResetUI;
            score.Penalized += ShowMessage;
        }

        private void OnDisable()
        {
            car.Restarted -= ResetUI;
            score.Penalized -= ShowMessage;
        }

        private void Update()
        {
            UpdateDriverPortrait();
            if (car.IsBuckled && buckledAt < 0f) buckledAt = Time.unscaledTime;
            // The belt mini game opens only when the driver portrait is tapped, and closes shortly after the click.
            bool showBelt = car.IsBuckled ? seatBeltOpen && Time.unscaledTime - buckledAt < beltPanelLinger : seatBeltOpen;
            seatBeltPanel.SetActive(showBelt);
            drivingControls.SetActive(!showBelt && !car.IsFinished && !score.IsOutOfStars);
            speedLabel.text = $"{car.Speed * 3.6f:00} km/h";
            seatBeltInstruction.text = car.IsBuckled ? "CLICK! Belt buckled. Ready to drive." : "Drag the belt handle into the buckle, then release.";
            feedbackLabel.text = CurrentFeedback();
        }

        private string CurrentFeedback()
        {
            if (car.IsFinished) return "COURSE COMPLETE - press RESTART to try again";
            if (!car.IsBuckled) return seatBeltOpen ? "Buckle up before driving" : "Tap your driver to put on the seat belt!";
            if (Time.unscaledTime < messageUntil) return message;
            if (score.IsOutOfStars) return "No stars left - press RESTART to try again";
            if (horn.IsHonking) return "BEEP!";
            return "Hold GO to drive. Turn the wheel to steer. STOP to brake.";
        }

        /// <summary>Shows a message on the feedback line for a few seconds.</summary>
        public void ShowMessage(string message)
        {
            this.message = message;
            messageUntil = Time.unscaledTime + messageSeconds;
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
            messageUntil = 0f;
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
