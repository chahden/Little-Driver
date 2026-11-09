using LittleDriver.Game;
using UnityEngine;

namespace LittleDriver.UI
{
    /// <summary>Shows the "Let's try again!" panel when the stars run out.</summary>
    public sealed class RetryPanel : MonoBehaviour
    {
        [SerializeField] private StarScore score;
        [SerializeField] private GameObject panel;

        private void OnEnable()
        {
            score.StarsChanged += Refresh;
            Refresh(score.Stars);
        }

        private void OnDisable() => score.StarsChanged -= Refresh;

        private void Refresh(int stars) => panel.SetActive(stars <= 0);
    }
}
