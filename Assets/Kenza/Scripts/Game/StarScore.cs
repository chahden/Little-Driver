using System;
using UnityEngine;

namespace LittleDriver.Game
{
    /// <summary>The player's stars. Each mistake costs one star.</summary>
    [DefaultExecutionOrder(-100)] // ready before the HUD views read it in OnEnable
    public sealed class StarScore : MonoBehaviour
    {
        [SerializeField, Min(1)] private int startingStars = 3;

        public int Stars { get; private set; }
        public bool IsOutOfStars => Stars <= 0;

        public event Action<int> StarsChanged;
        /// <summary>Raised with the message to show the player.</summary>
        public event Action<string> Penalized;
        public event Action OutOfStars;

        private void Awake() => Stars = startingStars;

        public void Penalize(string message)
        {
            if (IsOutOfStars) return;
            Stars--;
            Debug.Log($"[Stars] -1 star ({Stars}/{startingStars} left): {message}", this);
            StarsChanged?.Invoke(Stars);
            Penalized?.Invoke(message);
            if (IsOutOfStars) OutOfStars?.Invoke();
        }

        public void ResetStars()
        {
            Stars = startingStars;
            StarsChanged?.Invoke(Stars);
        }
    }
}
