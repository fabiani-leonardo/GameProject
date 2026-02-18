using UnityEngine;
using System.Collections;

public class GridBomb : MonoBehaviour
{
    public float warningTime = 1.0f; // Tempo prima di esplodere
    public float activeTime = 0.5f;  // Tempo in cui uccide
    public AudioClip chargeSound;    // Suono di caricamento (bip bip)
    public AudioClip explodeSound;   // Suono esplosione

    private SpriteRenderer sr;
    private BoxCollider2D col;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        col.enabled = false; // Inizia innocua

        StartCoroutine(BombRoutine());
    }

    IEnumerator BombRoutine()
    {
        // FASE 1: Caricamento (Lampeggio Accelerato + Cambio Colore)
        float timer = 0f;
        bool visible = true;
        
        // Colori di transizione (uguali al laser)
        Color startWarningColor = new Color(1f, 0f, 0f, 0.5f); // Giallo/Arancio trasparente
        Color endWarningColor = new Color(1f, 0f, 0f, 0.5f);     // Rosso trasparente
        Color activeColor = new Color(1f, 0f, 0f, 1f);           // Rosso solido (Letale)

        // --- FIX: Leggiamo il volume PRIMA del suono di caricamento ---
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        // Suono di caricamento opzionale moltiplicato per sfxVol
        if (chargeSound) AudioSource.PlayClipAtPoint(chargeSound, Camera.main.transform.position, 0.5f * sfxVol);

        while (timer < warningTime)
        {
            // Calcolo del progresso (da 0.0 a 1.0)
            float progress = timer / warningTime;

            // 1. CAMBIO COLORE
            Color currentWarningColor = Color.Lerp(startWarningColor, endWarningColor, progress);

            // 2. ACCELERAZIONE (da 0.15s a 0.03s)
            float currentBlinkSpeed = Mathf.Lerp(0.15f, 0.03f, progress);

            // Applica l'effetto
            sr.color = visible ? currentWarningColor : Color.clear;
            visible = !visible;
            
            // Aspetta
            yield return new WaitForSeconds(currentBlinkSpeed);
            timer += currentBlinkSpeed;
        }

        // FASE 2: ESPLOSIONE (Letale)
        sr.color = activeColor; // Rosso pieno
        col.enabled = true;     // Attiva l'hitbox mortale
        
        if (explodeSound != null)
        {
            // Non serve dichiarare di nuovo "float sfxVol = ...", basta usarlo:
            AudioSource.PlayClipAtPoint(explodeSound, Camera.main.transform.position, 0.6f * sfxVol);
        }

        yield return new WaitForSeconds(activeTime);

        // FASE 3: FINE
        Destroy(gameObject);
    }
}