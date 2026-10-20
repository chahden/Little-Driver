
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TrafficCarAI : MonoBehaviour
{
    [Header("Movement")]
    public float targetSpeed = 12f;
    public float acceleration = 4f;
    public float braking = 10f;
    public float deceleration = 2f;
    public float safeDistance = 8f;
    public LayerMask detectionLayers = ~0;

    [Header("Engine Sound")]
    public AudioClip engineClip;
    [Range(0f, 1f)] public float engineVolume = 0.35f;
    public float minAudioDistance = 5f;
    public float maxAudioDistance = 30f;

    [Header("Body Color")]
    public Renderer[] bodyRenderers;
    public Color[] possibleColors;

    [Header("Lights")]
    public bool createLights = true;
    public Transform  frontLeftPosition ;
    public Transform  frontRightPosition ;
    public Transform  rearLeftPosition ;
    public Transform  rearRightPosition ;
    public float lightSize = 0.2f;
    public bool illuminateRoad = false;

    private float currentSpeed;
    private bool isBraking;
    private AudioSource engineSource;

    private Renderer leftBrakeLight;
    private Renderer rightBrakeLight;
    private Light leftHeadlight;
    private Light rightHeadlight;

    public float CurrentSpeed
    {
        get { return currentSpeed; }
    }

    private void Awake()
    {
        engineSource = GetComponent<AudioSource>();
        engineSource.playOnAwake = false;
        engineSource.loop = true;
        engineSource.spatialBlend = 1f;
        engineSource.dopplerLevel = 0f;
        engineSource.minDistance = minAudioDistance;
        engineSource.maxDistance = maxAudioDistance;

        FindBodyRenderers();
        ChangeColor();

        if (createLights)
        {
            CreateLights();
        }

        if (engineClip != null)
        {
            engineSource.clip = engineClip;
            engineSource.volume = engineVolume;
            engineSource.Play();
        }
    }

    private void Update()
    {
        float desiredSpeed = targetSpeed;
        isBraking = false;

        // Look ahead for another traffic car or the player.
        Vector3 origin = transform.position + transform.forward * 1.5f;
        origin += Vector3.up * 0.5f;

        RaycastHit hit;

        if (Physics.Raycast(
            origin,
            transform.forward,
            out hit,
            safeDistance,
            detectionLayers,
            QueryTriggerInteraction.Ignore))
        {
            TrafficCarAI otherCar =
                hit.collider.GetComponentInParent<TrafficCarAI>();

            if (otherCar != null && otherCar != this)
            {
                if (otherCar.CurrentSpeed < desiredSpeed)
                {
                    desiredSpeed = Mathf.Max(0f, otherCar.CurrentSpeed - 1f);
                    isBraking = true;
                }
            }
            else if (hit.collider.CompareTag("Player"))
            {
                desiredSpeed = 0f;
                isBraking = true;
            }
        }

        float speedChange = acceleration;

        if (desiredSpeed < currentSpeed)
        {
            speedChange = deceleration;

            if (isBraking)
            {
                speedChange = braking;
            }
        }

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            desiredSpeed,
            speedChange * Time.deltaTime
        );

        transform.position +=
            transform.forward * currentSpeed * Time.deltaTime;

        UpdateBrakeLights();
        UpdateEngineSound();
    }

    private void FindBodyRenderers()
    {
        if (bodyRenderers != null && bodyRenderers.Length > 0)
        {
            return;
        }

        Renderer[] allRenderers =
            GetComponentsInChildren<Renderer>(true);

        System.Collections.Generic.List<Renderer> result =
            new System.Collections.Generic.List<Renderer>();

        for (int i = 0; i < allRenderers.Length; i++)
        {
            string objectName =
                allRenderers[i].gameObject.name.ToLower();

            if (objectName.Contains("wheel") ||
                objectName.Contains("tire") ||
                objectName.Contains("tyre") ||
                objectName.Contains("glass") ||
                objectName.Contains("window") ||
                objectName.Contains("light"))
            {
                continue;
            }

            result.Add(allRenderers[i]);
        }

        bodyRenderers = result.ToArray();
    }

    private void ChangeColor()
    {
        if (bodyRenderers == null || bodyRenderers.Length == 0)
        {
            return;
        }

        Color selectedColor;

        if (possibleColors != null && possibleColors.Length > 0)
        {
            selectedColor =
                possibleColors[Random.Range(0, possibleColors.Length)];
        }
        else
        {
            selectedColor = new Color(
                Random.Range(0.2f, 1f),
                Random.Range(0.2f, 1f),
                Random.Range(0.2f, 1f)
            );
        }

        MaterialPropertyBlock block = new MaterialPropertyBlock();

        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            if (bodyRenderers[i] == null)
            {
                continue;
            }

            bodyRenderers[i].GetPropertyBlock(block);
            block.SetColor("_BaseColor", selectedColor);
            block.SetColor("_Color", selectedColor);
            bodyRenderers[i].SetPropertyBlock(block);
        }
    }

    private void CreateLights()
{
    if (frontLeftPosition == null ||
        frontRightPosition == null ||
        rearLeftPosition == null ||
        rearRightPosition == null)
    {
        Debug.LogError(
            "Assign all four light position Transforms on " + gameObject.name,
            this
        );

        return;
    }

    leftHeadlight = CreateHeadlight(
        "Generated Headlight Left",
        frontLeftPosition.localPosition
    );

    rightHeadlight = CreateHeadlight(
        "Generated Headlight Right",
        frontRightPosition.localPosition
    );

    leftBrakeLight = CreateVisibleLight(
        "Generated Rear Light Left",
        rearLeftPosition.localPosition,
        Color.red
    );

    rightBrakeLight = CreateVisibleLight(
        "Generated Rear Light Right",
        rearRightPosition.localPosition,
        Color.red
    );

    leftHeadlight.enabled = illuminateRoad;
    rightHeadlight.enabled = illuminateRoad;
}
    private Light CreateHeadlight(string objectName, Vector3 localPosition)
    {
        GameObject lightObject =
            GameObject.CreatePrimitive(PrimitiveType.Sphere);

        lightObject.name = objectName;
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = localPosition;
        lightObject.transform.localScale =
            Vector3.one * lightSize;

        Collider objectCollider = lightObject.GetComponent<Collider>();

        if (objectCollider != null)
        {
            Destroy(objectCollider);
        }

        SetLightMaterial(lightObject, Color.white);

        Light actualLight = lightObject.AddComponent<Light>();
        actualLight.type = LightType.Spot;
        actualLight.color = Color.white;
        actualLight.range = 15f;
        actualLight.intensity = 2f;
        actualLight.spotAngle = 45f;

        return actualLight;
    }

    private Renderer CreateVisibleLight(
        string objectName,
        Vector3 localPosition,
        Color color)
    {
        GameObject lightObject =
            GameObject.CreatePrimitive(PrimitiveType.Sphere);

        lightObject.name = objectName;
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = localPosition;
        lightObject.transform.localScale =
            Vector3.one * lightSize;

        Collider objectCollider = lightObject.GetComponent<Collider>();

        if (objectCollider != null)
        {
            Destroy(objectCollider);
        }

        SetLightMaterial(lightObject, color);

        return lightObject.GetComponent<Renderer>();
    }

    private void SetLightMaterial(GameObject lightObject, Color color)
    {
        Renderer lightRenderer = lightObject.GetComponent<Renderer>();
        Material lightMaterial = new Material(lightRenderer.sharedMaterial);

        lightMaterial.color = color;

        if (lightMaterial.HasProperty("_BaseColor"))
        {
            lightMaterial.SetColor("_BaseColor", color);
        }

        if (lightMaterial.HasProperty("_EmissionColor"))
        {
            lightMaterial.EnableKeyword("_EMISSION");
            lightMaterial.SetColor("_EmissionColor", color * 2f);
        }

        lightRenderer.material = lightMaterial;
    }

    private void UpdateBrakeLights()
    {
        Color color = Color.red;

        if (isBraking)
        {
            color = new Color(1f, 0.05f, 0.05f);
        }
        else
        {
            color = new Color(0.35f, 0f, 0f);
        }

        SetRendererColor(leftBrakeLight, color);
        SetRendererColor(rightBrakeLight, color);
    }

    private void SetRendererColor(Renderer targetRenderer, Color color)
    {
        if (targetRenderer == null)
        {
            return;
        }

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(block);
        block.SetColor("_BaseColor", color);
        block.SetColor("_Color", color);
        block.SetColor("_EmissionColor", color * 2f);
        targetRenderer.SetPropertyBlock(block);
    }

    private void UpdateEngineSound()
    {
        if (engineClip == null || engineSource == null)
        {
            return;
        }

        if (!engineSource.isPlaying)
        {
            engineSource.Play();
        }

        float speedRatio = 0f;

        if (targetSpeed > 0f)
        {
            speedRatio = currentSpeed / targetSpeed;
        }

        engineSource.pitch = Mathf.Lerp(
            0.75f,
            1.2f,
            Mathf.Clamp01(speedRatio)
        );

        engineSource.volume = engineVolume *
            Mathf.Clamp01(0.25f + speedRatio * 0.75f);
    }

    public void ResetCar(float newTargetSpeed)
    {
        currentSpeed = 0f;
        targetSpeed = Mathf.Max(0f, newTargetSpeed);
        isBraking = false;
    }
}
