using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 6f, -8f);
    public float smooth = 5f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 goal = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, goal, smooth * Time.deltaTime);
        transform.LookAt(target.position);
    }
}
