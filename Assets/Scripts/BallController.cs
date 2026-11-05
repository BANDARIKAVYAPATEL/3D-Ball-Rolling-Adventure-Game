using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallController : MonoBehaviour
{
    public float moveForce = 10f;
    public float fallY = -10f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SetPhysics(moveForce, 1.2f);
    }

    // Called by the level generator: ice levels use low drag so the ball slides.
    public void SetPhysics(float force, float drag)
    {
        moveForce = force;
        if (rb == null) rb = GetComponent<Rigidbody>();
#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = drag;
        rb.angularDamping = 0.5f;
#else
        rb.drag = drag;
        rb.angularDrag = 0.5f;
#endif
    }

    void FixedUpdate()
    {
        Vector2 m = GameInput.Move();
        rb.AddForce(new Vector3(m.x, 0f, m.y) * moveForce);
    }

    void Update()
    {
        // Fell off the platform: restart the level.
        if (transform.position.y < fallY && GameManager.Instance != null)
            GameManager.Instance.Die();
    }
}
