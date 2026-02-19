using UnityEngine;
using System.Collections;

public class LaserBehavior : MonoBehaviour
{
    [Header("Tempi")]
    public float warningDuration = 1.0f; 
    public float activeDuration = 0.5f;  
    public AudioClip laserSound; 

    [Header("Riferimenti")]
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider;

    void Start()
    {
        
        boxCollider.enabled = false;
        
       
        StartCoroutine(LaserRoutine());
    }

    IEnumerator LaserRoutine()
    {
       
        float timer = 0f;
        bool isVisible = true;
        
       
        Color startWarningColor = new Color(1f, 0f, 0f, 0.5f); 
        Color endWarningColor = new Color(1f, 0f, 0f, 0.5f);     
        Color activeColor = new Color(1f, 0f, 0f, 1f);         

        while (timer < warningDuration)
        {
           
            float progress = timer / warningDuration;

            
            Color currentWarningColor = Color.Lerp(startWarningColor, endWarningColor, progress);

            
            float currentBlinkSpeed = Mathf.Lerp(0.15f, 0.03f, progress);

            
            spriteRenderer.color = isVisible ? currentWarningColor : Color.clear;
            isVisible = !isVisible;
            
            
            yield return new WaitForSeconds(currentBlinkSpeed);
            timer += currentBlinkSpeed;
        }

        
        spriteRenderer.color = activeColor; 
        boxCollider.enabled = true;        
        
        
        
        if (laserSound != null)
        {
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
           
            AudioSource.PlayClipAtPoint(laserSound, Camera.main.transform.position, 0.4f * sfxVol); 
        }

        yield return new WaitForSeconds(activeDuration);

       
        Destroy(gameObject); 
    }
}