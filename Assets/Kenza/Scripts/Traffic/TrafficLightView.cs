using UnityEngine;

namespace LittleDriver.Traffic
{
    /// <summary>Lights the lamp that matches the traffic light's state and dims the other two.</summary>
    public sealed class TrafficLightView : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField] private TrafficLight trafficLight;
        [SerializeField] private Renderer redLamp;
        [SerializeField] private Renderer yellowLamp;
        [SerializeField] private Renderer greenLamp;
        [SerializeField] private Color red = new Color(1f, 0.12f, 0.08f);
        [SerializeField] private Color yellow = new Color(1f, 0.78f, 0.05f);
        [SerializeField] private Color green = new Color(0.15f, 1f, 0.35f);
        [SerializeField, Range(0f, 1f)] private float offBrightness = 0.15f;

        private MaterialPropertyBlock properties;

        private void OnEnable()
        {
            properties ??= new MaterialPropertyBlock();
            trafficLight.StateChanged += Show;
            Show(trafficLight.CurrentState);
        }

        private void OnDisable() => trafficLight.StateChanged -= Show;

        private void Show(TrafficLightState state)
        {
            SetLamp(redLamp, red, state == TrafficLightState.Red);
            SetLamp(yellowLamp, yellow, state == TrafficLightState.Yellow);
            SetLamp(greenLamp, green, state == TrafficLightState.Green);
        }

        private void SetLamp(Renderer lamp, Color colour, bool on)
        {
            lamp.GetPropertyBlock(properties);
            properties.SetColor(BaseColorId, on ? colour : colour * offBrightness);
            lamp.SetPropertyBlock(properties);
        }
    }
}
