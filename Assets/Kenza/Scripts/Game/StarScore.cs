using System;
using UnityEngine;

namespace LittleDriver.Game
{
    /// <summary>Why a star was lost or earned.</summary>
    public enum StarReason { Bump, RedLight, BusyCrossing, WaitedAtCrossing, WaitedForCrosser, StoppedAtRedLight }

    /// <summary>The player's stars: mistakes cost one, patience earns one back (never above the starting count).</summary>
    [DefaultExecutionOrder(-100)] // ready before the HUD views read it in OnEnable
    public sealed class StarScore : MonoBehaviour
    {
        [SerializeField, Min(1)] private int startingStars = 3;

        public int Stars { get; private set; }
        public bool IsOutOfStars => Stars <= 0;

        public event Action<int> StarsChanged;
        public event Action<StarReason> Penalized;
        /// <summary>Raised for every good deed, even when the stars are already full.</summary>
        public event Action<StarReason> Rewarded;
        public event Action OutOfStars;

        private void Awake() => Stars = startingStars;

        public void Penalize(StarReason reason)
        {
            if (IsOutOfStars) return;
            Stars--;
            Debug.Log($"[Stars] -1 star ({Stars}/{startingStars}): {reason}", this);
            StarsChanged?.Invoke(Stars);
            Penalized?.Invoke(reason);
            if (IsOutOfStars) OutOfStars?.Invoke();
        }

        public void Reward(StarReason reason)
        {
            if (IsOutOfStars) return;
            if (Stars < startingStars)
            {
                Stars++;
                StarsChanged?.Invoke(Stars);
            }
            Debug.Log($"[Stars] +1 star ({Stars}/{startingStars}): {reason}", this);
            Rewarded?.Invoke(reason);
        }

        public void ResetStars()
        {
            Stars = startingStars;
            StarsChanged?.Invoke(Stars);
        }
    }
}
