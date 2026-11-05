using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    void Update()
    {
        transform.Rotate(0f, 120f * Time.deltaTime, 0f, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance != null) GameManager.Instance.CollectCoin();
        Destroy(gameObject);
    }
}
