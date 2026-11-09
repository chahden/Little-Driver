using System.Collections.Generic;
using UnityEngine;

namespace LittleDriver.Traffic
{
    /// <summary>Anything driving on the road that pedestrians must look out for.</summary>
    public interface IVehicle
    {
        Vector3 Position { get; }
        Vector3 Forward { get; }
        /// <summary>Forward speed in m/s.</summary>
        float Speed { get; }
    }

    /// <summary>Every vehicle currently on the road (player and AI).</summary>
    public static class Vehicles
    {
        private static readonly List<IVehicle> all = new List<IVehicle>();

        public static IReadOnlyList<IVehicle> All => all;

        public static void Register(IVehicle vehicle)
        {
            if (!all.Contains(vehicle)) all.Add(vehicle);
        }

        public static void Unregister(IVehicle vehicle) => all.Remove(vehicle);
    }
}
