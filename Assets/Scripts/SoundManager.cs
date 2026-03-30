using UnityEngine;
using TMPro; 

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("효과음(SFX) 클립")]
    public AudioClip pickaxeHitSound;
    public AudioClip moneyPickupSound;

    private AudioSource sfxSource;

    [Header("UI 설정")]
    public TMP_Text soundButtonText; 
    private bool isMuted = false;    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip clip, float volume = 0.5f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void ToggleSound()
    {
        isMuted = !isMuted; 

        if (isMuted)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            AudioListener.volume = 0.5f;
        }
    }
}