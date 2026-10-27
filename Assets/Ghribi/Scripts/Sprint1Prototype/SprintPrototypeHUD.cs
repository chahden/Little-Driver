using TMPro;
using UnityEngine;

namespace LittleDriver.Prototype
{
    public sealed class SprintPrototypeHUD : MonoBehaviour
    {
        [SerializeField] private SprintPrototypeCar car;
        [SerializeField] private GameObject seatBeltPanel;
        [SerializeField] private GameObject drivingControls;
        [SerializeField] private SprintSeatBeltDrag beltDrag;
        [SerializeField] private TMP_Text speedLabel;
        [SerializeField] private TMP_Text beltLabel;
        [SerializeField] private TMP_Text feedbackLabel;
        [SerializeField] private TMP_Text collisionLabel;
        [SerializeField] private TMP_Text seatBeltInstruction;
        [Header("Driver seat-belt portrait")]
        [SerializeField] private UnityEngine.UI.Image driverPortrait;
        [SerializeField] private UnityEngine.UI.Image driverBorder;
        [SerializeField] private UnityEngine.UI.Outline driverGlow;
        [SerializeField] private Sprite driverWithoutBelt;
        [SerializeField] private Sprite driverWithBelt;
        private float buckledAt = -1;

        private void OnEnable() => car.Restarted += ResetUI;
        private void OnDisable() => car.Restarted -= ResetUI;

        private void Update()
        {
            UpdateDriverPortrait();
            if (car.IsBuckled && buckledAt < 0) buckledAt = Time.unscaledTime;
            bool showBelt = !car.IsBuckled || Time.unscaledTime - buckledAt < 0.65f;
            seatBeltPanel.SetActive(showBelt);
            drivingControls.SetActive(!showBelt && !car.IsFinished);
            speedLabel.text = $"{car.Speed * 3.6f:00} km/h  |  {(car.Lane == 0 ? "LEFT" : "RIGHT")} LANE";
            beltLabel.text = car.IsBuckled ? "BELT: BUCKLED" : "BELT: NOT BUCKLED";
            collisionLabel.text = $"CONTACTS: {car.CollisionCount}  |  HORN: {car.HornCount}";
            seatBeltInstruction.text = car.IsBuckled ? "CLICK! Belt buckled. Ready to drive." : "Drag the belt handle into the buckle, then release.";
            if (car.IsFinished) feedbackLabel.text = "COURSE COMPLETE - press RESTART to try again";
            else if (!car.IsBuckled) feedbackLabel.text = "Buckle up before driving";
            else if (car.IsHornBlocked) feedbackLabel.text = "Bel chwaya aal zamara! Nestaamlouha ken wa9t el haja.";
            else if (car.IsRecovering) feedbackLabel.text = "Oops! Moving back 2 metres. Then keep going!";
            else if (Time.unscaledTime < car.CollisionUntil) feedbackLabel.text = "All okay! Choose a clear lane and keep going.";
            else if (car.IsHonking) feedbackLabel.text = "BEEP!";
            else feedbackLabel.text = "Hold GO to drive. Release to slow down. STOP to brake.";
        }

        public void Restart()
        {
            car.Restart();
        }

        private void ResetUI()
        {
            buckledAt = -1;
            seatBeltPanel.SetActive(true);
            beltDrag.ResetBelt();
            UpdateDriverPortrait();
        }

        private void UpdateDriverPortrait()
        {
            if (driverPortrait == null || driverBorder == null) return;
            bool buckled = car.IsBuckled;
            driverPortrait.sprite = buckled ? driverWithBelt : driverWithoutBelt;
            float pulse = buckled ? 1f : 0.8f + 0.2f * Mathf.Sin(Time.unscaledTime * 4f);
            Color colour = buckled ? new Color(0.12f, 0.8f, 0.3f) : new Color(1f, 0.12f, 0.16f);
            driverBorder.color = colour * new Color(pulse, pulse, pulse, 1f);
            if (driverGlow != null)
                driverGlow.effectColor = new Color(colour.r, colour.g, colour.b, buckled ? 0.35f : 0.3f + 0.25f * pulse);
        }
    }
}
