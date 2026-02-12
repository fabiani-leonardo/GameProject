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
        // FASE 1: Caricamento (Lampeggia)
        float timer = 0f;
        bool visible = true;
        Color warnColor = new Color(1f, 0f, 0f, 0.31f); // Arancione trasparente

        if (chargeSound) AudioSource.PlayClipAtPoint(chargeSound, Camera.main.transform.position, 0.5f);

        while (timer < warningTime)
        {
            sr.color = visible ? warnColor : Color.clear;
            visible = !visible;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        // FASE 2: ESPLOSIONE (Letale)
        sr.color = Color.red; // Rosso pieno
        col.enabled = true;   // Uccide!
        
        if (explodeSound) AudioSource.PlayClipAtPoint(explodeSound, Camera.main.transform.position, 0.6f);

        yield return new WaitForSeconds(activeTime);

        Destroy(gameObject);
    }
}