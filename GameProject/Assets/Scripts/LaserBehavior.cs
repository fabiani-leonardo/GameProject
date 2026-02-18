using UnityEngine;
using System.Collections;

public class LaserBehavior : MonoBehaviour
{
    [Header("Tempi")]
    public float warningDuration = 1.0f; // Tempo di lampeggiamento
    public float activeDuration = 0.5f;  // Tempo in cui il laser uccide
    public AudioClip laserSound; // Trascina il file del laser qui nell'Inspector

    [Header("Riferimenti")]
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider;

    void Start()
    {
        // Assicuriamoci che il collider sia spento all'inizio
        boxCollider.enabled = false;
        
        // Avvia la sequenza
        StartCoroutine(LaserRoutine());
    }

    IEnumerator LaserRoutine()
    {
        // --- FASE 1: AVVISO (Lampeggio Accelerato + Cambio Colore) ---
        float timer = 0f;
        bool isVisible = true;
        
        // Definiamo i colori di transizione
        Color startWarningColor = new Color(1f, 0f, 0f, 0.5f); // Giallo/Arancio trasparente
        Color endWarningColor = new Color(1f, 0f, 0f, 0.5f);     // Rosso trasparente
        Color activeColor = new Color(1f, 0f, 0f, 1f);           // Rosso solido (Letale)

        while (timer < warningDuration)
        {
            // Calcoliamo a che punto siamo dell'avvertimento (valore da 0.0 a 1.0)
            float progress = timer / warningDuration;

            // 1. CAMBIO COLORE: Sfuma dolcemente dal giallo al rosso in base al progresso
            Color currentWarningColor = Color.Lerp(startWarningColor, endWarningColor, progress);

            // 2. ACCELERAZIONE: La velocità passa da 0.15s (lento) a 0.03s (velocissimo, quasi un tremolio)
            float currentBlinkSpeed = Mathf.Lerp(0.15f, 0.03f, progress);

            // Applica l'effetto visivo
            spriteRenderer.color = isVisible ? currentWarningColor : Color.clear;
            isVisible = !isVisible;
            
            // Aspetta il tempo calcolato e aggiorna il timer
            yield return new WaitForSeconds(currentBlinkSpeed);
            timer += currentBlinkSpeed;
        }

        // --- FASE 2: ATTIVO (Letale) ---
        spriteRenderer.color = activeColor; // Diventa rosso solido
        boxCollider.enabled = true;         // Ora può uccidere il player
        
        
        // Fai partire il suono
        if (laserSound != null)
        {
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
            // Ho aggiunto "* sfxVol" al parametro del volume
            AudioSource.PlayClipAtPoint(laserSound, Camera.main.transform.position, 0.4f * sfxVol); 
        }

        yield return new WaitForSeconds(activeDuration);

        // --- FASE 3: FINE ---
        Destroy(gameObject); // Rimuovi il laser
    }
}