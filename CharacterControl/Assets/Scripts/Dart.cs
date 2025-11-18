using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Dart : MonoBehaviour
{
    [Header("Pooling")]
    [Tooltip("Pool that owns this dart (same one used by PlayerController).")]
    public ObjectPool dartPool;

    [Tooltip("Pool that owns the balloons.")]
    public ObjectPool balloonPool;

    // Prevents double-returns (e.g., trigger + collision in the same frame)
    private bool _returned;

    private void OnEnable()
    {
        _returned = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryHandleHit(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHandleHit(other);
    }

    private void TryHandleHit(Collider hit)
    {
        if (_returned || hit == null) return;

        // Walk up the hierarchy to find an ancestor tagged "Balloon"
        GameObject balloon = FindTaggedAncestor(hit.transform, "Balloon");
        if (balloon == null) return;

        _returned = true; // set early to avoid re-entry

        // Return balloon first, then the dart
        if (balloonPool != null)
            balloonPool.ReturnObject(balloon);
        else
            balloon.SetActive(false); // fallback safety

        if (dartPool != null)
            dartPool.ReturnObject(gameObject);
        else
            gameObject.SetActive(false); // fallback safety
    }

    private static GameObject FindTaggedAncestor(Transform t, string tag)
    {
        while (t != null)
        {
            if (t.CompareTag(tag))
                return t.gameObject;
            t = t.parent;
        }
        return null;
    }
}
