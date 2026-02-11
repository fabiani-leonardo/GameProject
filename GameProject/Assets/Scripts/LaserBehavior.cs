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
        // --- FASE 1: AVVISO (Lampeggio) ---
        float timer = 0f;
        bool isVisible = true;
        
        // Colore di avviso (Giallo o Rosso trasparente)
        Color warningColor = new Color(1f, 0f, 0f, 0.3f); 
        Color activeColor = new Color(1f, 0f, 0f, 1f); // Rosso pieno

        while (timer < warningDuration)
        {
            // Effetto lampeggio veloce
            spriteRenderer.color = isVisible ? warningColor : Color.clear;
            isVisible = !isVisible;
            
            float blinkSpeed = 0.1f; // Ogni quanto lampeggia
            yield return new WaitForSeconds(blinkSpeed);
            timer += blinkSpeed;
        }

        // --- FASE 2: ATTIVO (Letale) ---
        spriteRenderer.color = activeColor; // Diventa rosso solido
        boxCollider.enabled = true;         // Ora può uccidere il player
        
        if (laserSound != null)
        {
            // Il terzo parametro (0.5f) è il volume (0.0 = muto, 1.0 = massimo)
            // Prova con 0.3f o 0.5f per abbassarlo
            AudioSource.PlayClipAtPoint(laserSound, Camera.main.transform.position, 0.1f); 
        }

        yield return new WaitForSeconds(activeDuration);

        // --- FASE 3: FINE ---
        Destroy(gameObject); // Rimuovi il laser
    }
}