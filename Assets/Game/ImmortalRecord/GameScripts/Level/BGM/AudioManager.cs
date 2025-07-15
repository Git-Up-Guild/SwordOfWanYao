using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音效设置")]
    [SerializeField] private AudioClip normalSoundEffect;
    [SerializeField] private AudioClip lightMonkSoundEffect;

    [Header("BGM 设置")]
    [SerializeField] private AudioClip bgmClip;

    private AudioSource seSource;
    private AudioSource bgmSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 分别创建音效和 BGM 播放器
        seSource = gameObject.AddComponent<AudioSource>();
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
    }

    private void Start()
    {
        PlayBGM();
    }

    /// <summary>
    /// 播放音效 1
    /// </summary>
    public void PlayNormalEffect()
    {
        if (normalSoundEffect != null)
        {
            seSource.PlayOneShot(normalSoundEffect);
        }
    }

    /// <summary>
    /// 播放音效 2
    /// </summary>
    public void PlayLightMonkSoundEffect()
    {
        if (lightMonkSoundEffect != null)
        {
            seSource.PlayOneShot(lightMonkSoundEffect);
        }
    }

    /// <summary>
    /// 播放 BGM（循环）
    /// </summary>
    public void PlayBGM()
    {
        if (bgmClip == null)
            return;

        if (bgmSource.clip != bgmClip)
        {
            bgmSource.clip = bgmClip;
        }

        if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    /// <summary>
    /// 停止 BGM
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }
}
