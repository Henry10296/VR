using UnityEngine;

public class SwitchBlinkEmission_LoopSound : MonoBehaviour
{
    [Header("目标")]
    public Renderer targetRenderer;

    [Header("发光参数")]
    public Color emissionColor = Color.cyan;
    public float minIntensity = 0f;
    public float maxIntensity = 2f;
    public float blinkSpeed = 6f;

    [Header("音效")]
    public AudioSource audioSource;

    private bool isOn = false;
    private Material mat;

    void Start()
    {
        mat = targetRenderer.material;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.black);

        if (audioSource)
            audioSource.loop = true;
    }

    void Update()
    {
        if (!isOn)
        {
            // 灯灭
            mat.SetColor("_EmissionColor", Color.black);

            if (audioSource && audioSource.isPlaying)
                audioSource.Stop();

            return;
        }

        // 开灯播放音效
        if (audioSource && !audioSource.isPlaying)
            audioSource.Play();

        float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        mat.SetColor("_EmissionColor", emissionColor * intensity);
    }

    //开关
    public void Toggle()
    {
        isOn = !isOn;
    }

    public void TurnOn()
    {
        isOn = true;
    }

    public void TurnOff()
    {
        isOn = false;
    }
}
