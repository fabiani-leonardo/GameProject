using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicVolumeController : MonoBehaviour
{
    private AudioSource audioSource;
    private float baseVolume;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
       
        baseVolume = audioSource.volume; 
    }

    void Update()
    {
        
        audioSource.volume = baseVolume * PlayerPrefs.GetFloat("MusicVolume", 1f);
    }
}