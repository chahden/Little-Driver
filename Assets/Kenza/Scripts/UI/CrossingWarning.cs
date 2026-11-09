using LittleDriver.AI.Pedestrians;
using UnityEngine;

namespace LittleDriver.UI
{
    /// <summary>Warns the driver on the HUD when a road crosser (child, cat, dog) goes onto the road.</summary>
    public sealed class CrossingWarning : MonoBehaviour
    {
        [SerializeField] private RoadCrosser[] crossers = new RoadCrosser[0];
        [SerializeField] private Lvl1HUD hud;

        private void OnEnable()
        {
            foreach (RoadCrosser crosser in crossers) crosser.CrossingStarted += Warn;
        }

        private void OnDisable()
        {
            foreach (RoadCrosser crosser in crossers) crosser.CrossingStarted -= Warn;
        }

        private void Warn(RoadCrosser crosser) => hud.ShowMessage(crosser.WarningMessage);
    }
}
