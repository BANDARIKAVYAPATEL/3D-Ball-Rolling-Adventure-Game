using UnityEngine;

// A bar that spins around the middle of the path. Touching it restarts the level.
public class SpinningBar : MonoBehaviour
{
    public float degreesPerSecond = 90f;

    void Update()
    {
        transform.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.World);
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Player") && GameManager.Instance != null)
            GameManager.Instance.Die();
    }
}
