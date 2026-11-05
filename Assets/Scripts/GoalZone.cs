using UnityEngine;

public class GoalZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (GameManager.Instance != null) GameManager.Instance.GoalTouched();
    }
}
