using UnityEngine;
using System.Collections;

public class GridBomb : MonoBehaviour
{
    public float warningTime = 1.0f; 
    public float activeTime = 0.5f;  
    public AudioClip chargeSound;    
    public AudioClip explodeSound;   

    private SpriteRenderer sr;
    private BoxCollider2D col;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        col.enabled = false; 

        StartCoroutine(BombRoutine());
    }

    IEnumerator BombRoutine()
    {
        
        float timer = 0f;
        bool visible = true;
        
       
        Color startWarningColor = new Color(1f, 0f, 0f, 0.5f); 
        Color endWarningColor = new Color(1f, 0f, 0f, 0.5f);     
        Color activeColor = new Color(1f, 0f, 0f, 1f);         

       
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
       
        if (chargeSound) AudioSource.PlayClipAtPoint(chargeSound, Camera.main.transform.position, 0.5f * sfxVol);

        while (timer < warningTime)
        {
           
            float progress = timer / warningTime;

            
            Color currentWarningColor = Color.Lerp(startWarningColor, endWarningColor, progress);

            
            float currentBlinkSpeed = Mathf.Lerp(0.15f, 0.03f, progress);

            
            sr.color = visible ? currentWarningColor : Color.clear;
            visible = !visible;
            
           
            yield return new WaitForSeconds(currentBlinkSpeed);
            timer += currentBlinkSpeed;
        }

        
        sr.color = activeColor;
        col.enabled = true;    
        
        if (explodeSound != null)
        {
            
            AudioSource.PlayClipAtPoint(explodeSound, Camera.main.transform.position, 0.6f * sfxVol);
        }

        yield return new WaitForSeconds(activeTime);

        
        Destroy(gameObject);
    }
}