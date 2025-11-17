using UnityEditor.Build.Content;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Balloon : MonoBehaviour
{
    [Header("Balloon Settings")]
    public int currencyValue = 10;
    public GameObject popEffect;
    public AudioClip popSound;

    private AudioSource audioSource;
    private bool isPopped = false;



    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource = null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    [System.Obsolete]
    public void PopBalloon()
    {
        if (isPopped)
        {
            return;
        }

        isPopped = true;
        
        if (popEffect != null)
        {
            Instantiate(popEffect, transform.position, Quaternion.identity);
        }
        AwardCurrency();

        Destroy(gameObject, 0.1f);
    }

    [System.Obsolete]
    void AwardCurrency()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            // Currency awarding is ignored for now.
        }
    }
}
