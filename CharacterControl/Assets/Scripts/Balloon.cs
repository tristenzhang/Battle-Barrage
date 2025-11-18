using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Balloon : MonoBehaviour
{
    [Header("Balloon Settings")]
    public int currencyValue = 10;
    public GameObject popEffect;
    public AudioClip popSound;

    [Range(0f, 1f)]
    public float volume = 1f;

    // private AudioSource audioSource;
    // private bool isPopped = false;

    [Tooltip("Optional: world-space position to play the SFX from; defaults to the balloon position.")]
    public Transform sfxOrigin;

    private void OnDisable()
    {
        if (popSound == null) return;

        Vector3 pos = sfxOrigin ? sfxOrigin.position : transform.position;
        AudioSource.PlayClipAtPoint(popSound, pos, volume);
    }

    /*
    [System.Obsolete]
    void AwardCurrency()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.AddCurrency(currencyValue);
        }
    } */
}
