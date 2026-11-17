using UnityEngine;

public class TrashBag : MonoBehaviour
{
    [SerializeField] private AudioClip collectSound;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private GameObject flyPrefab;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        TruckDriver truck = other.GetComponentInParent<TruckDriver>();
        if (truck == null) return;   // ignore anything that isn't the truck

        collected = true;
        if (BagCounterUI.Instance != null)
            BagCounterUI.Instance.AddOne();

        // Plays at a temporary object, so the sound isn't cut off by Destroy
        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position, volume);

        // Fly becomes a child of the truck
        if (flyPrefab != null)
            Instantiate(flyPrefab, truck.transform);

        Destroy(gameObject);
    }
}