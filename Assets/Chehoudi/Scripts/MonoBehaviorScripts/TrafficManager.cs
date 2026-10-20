
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TrafficLaneOption
{
    public float xPosition;
    public bool enabled = true;
}

public class TrafficManager : MonoBehaviour
{
    [Header("References")]
    public GameObject carPrefab;
    public Transform player;

    [Header("Traffic Amount")]
    [Min(1)] public int carCount = 12;
    public float spawnAheadDistance = 60f;
    public float recycleBehindDistance = 30f;
    public float spacingBetweenCars = 15f;
    public float spawnHeight = 0f;

    [Header("Lane Selection")]
    public TrafficLaneOption[] lanes;

    [Header("AI Speed")]
    public float minSpeed = 8f;
    public float maxSpeed = 14f;

    private readonly List<TrafficCarAI> cars =
        new List<TrafficCarAI>();

    private void Start()
    {
        if (carPrefab == null)
        {
            Debug.LogError("TrafficManager: Assign a car prefab.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("TrafficManager: Assign the player Transform.");
            return;
        }

        if (GetEnabledLaneCount() == 0)
        {
            Debug.LogError("TrafficManager: Enable at least one lane.");
            return;
        }

        SpawnInitialTraffic();
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        RecycleCars();
    }

    private int GetEnabledLaneCount()
    {
        int count = 0;

        if (lanes == null)
        {
            return count;
        }

        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i] != null && lanes[i].enabled)
            {
                count++;
            }
        }

        return count;
    }

    private float GetLaneX()
    {
        List<float> availableLanes = new List<float>();

        for (int i = 0; i < lanes.Length; i++)
        {
            if (lanes[i] != null && lanes[i].enabled)
            {
                availableLanes.Add(lanes[i].xPosition);
            }
        }

        int selectedIndex = Random.Range(0, availableLanes.Count);
        return availableLanes[selectedIndex];
    }

    private void SpawnInitialTraffic()
    {
        for (int i = 0; i < carCount; i++)
        {
            float zPosition =
                player.position.z +
                spawnAheadDistance +
                i * spacingBetweenCars;

            SpawnCar(zPosition);
        }
    }

    private void SpawnCar(float zPosition)
    {
        Vector3 spawnPosition = new Vector3(
            GetLaneX(),
            spawnHeight,
            zPosition
        );

        GameObject newCar = Instantiate(
            carPrefab,
            spawnPosition,
            Quaternion.identity
        );

        TrafficCarAI carAI = newCar.GetComponent<TrafficCarAI>();

        if (carAI == null)
        {
            carAI = newCar.AddComponent<TrafficCarAI>();
        }

        float speed = Random.Range(minSpeed, maxSpeed);
        carAI.ResetCar(speed);

        cars.Add(carAI);
    }

    private void RecycleCars()
    {
        float recycleZ = player.position.z - recycleBehindDistance;

        for (int i = 0; i < cars.Count; i++)
        {
            TrafficCarAI car = cars[i];

            if (car == null)
            {
                continue;
            }

            if (car.transform.position.z < recycleZ)
            {
                float newZ = GetFurthestCarZ() + spacingBetweenCars;

                car.transform.position = new Vector3(
                    GetLaneX(),
                    spawnHeight,
                    newZ
                );

                car.transform.rotation = Quaternion.identity;
                car.ResetCar(Random.Range(minSpeed, maxSpeed));
            }
        }
    }

    private float GetFurthestCarZ()
    {
        float furthestZ =
            player.position.z + spawnAheadDistance;

        for (int i = 0; i < cars.Count; i++)
        {
            if (cars[i] != null)
            {
                if (cars[i].transform.position.z > furthestZ)
                {
                    furthestZ = cars[i].transform.position.z;
                }
            }
        }

        return furthestZ;
    }
}
