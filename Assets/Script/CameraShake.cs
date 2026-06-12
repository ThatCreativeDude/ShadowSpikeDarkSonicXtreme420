using Unity.Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private CinemachineBasicMultiChannelPerlin noise;

    private float timer;

    void Awake()
    {
        Instance = this;

        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (noise == null)
        {
            Debug.LogError("NO CINEMACHINE PERLIN FOUND");
        }
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                noise.AmplitudeGain = 0;
            }
        }
    }

    public void ShakeCamera(float intensity, float duration = 0.15f)
    {
        if (noise == null)
            return;

        noise.AmplitudeGain = intensity;
        timer = duration;
    }
}