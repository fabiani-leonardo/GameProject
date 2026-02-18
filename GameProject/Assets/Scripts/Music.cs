using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicVolumeController : MonoBehaviour
{
    private AudioSource audioSource;
    private float baseVolume;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // Salva il volume originale che hai impostato nell'Inspector (es. 0.3)
        baseVolume = audioSource.volume; 
    }

    void Update()
    {
        // Moltiplica il volume di base per lo slider (da 0 a 1)
        // L'Update permette di sentire il cambio di volume in tempo reale mentre muovi lo slider!
        audioSource.volume = baseVolume * PlayerPrefs.GetFloat("MusicVolume", 1f);
    }
}