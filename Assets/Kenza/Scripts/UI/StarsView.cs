using LittleDriver.Game;
using UnityEngine;
using UnityEngine.UI;

namespace LittleDriver.UI
{
    /// <summary>Row of star icons: gold for stars left, grey for stars lost.</summary>
    public sealed class StarsView : MonoBehaviour
    {
        [SerializeField] private StarScore score;
        [SerializeField] private Image[] stars;
        [SerializeField] private Color fullColour = new Color(1f, 0.8f, 0.1f);
        [SerializeField] private Color lostColour = new Color(0.35f, 0.35f, 0.4f, 0.6f);

        private void OnEnable()
        {
            score.StarsChanged += Show;
            Show(score.Stars);
        }

        private void OnDisable() => score.StarsChanged -= Show;

        private void Show(int count)
        {
            for (int i = 0; i < stars.Length; i++)
                stars[i].color = i < count ? fullColour : lostColour;
        }
    }
}
