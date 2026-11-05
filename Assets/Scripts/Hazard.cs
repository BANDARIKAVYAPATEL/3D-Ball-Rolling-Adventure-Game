using UnityEngine;

// A red block that slides back and forth. Touching it restarts the level.
public class Hazard : MonoBehaviour
{
    public Vector3 moveOffset = new Vector3(6f, 0f, 0f);
    public float speed = 0.35f;

    Vector3 start;

    void Start()
    {
        start = transform.position;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1f);
        transform.position = start + moveOffset * (t - 0.5f);
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Player") && GameManager.Instance != null)
            GameManager.Instance.Die();
    }
}
