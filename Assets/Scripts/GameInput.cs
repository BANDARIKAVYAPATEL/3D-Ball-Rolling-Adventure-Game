using UnityEngine;

// Reads WASD / arrow keys with Unity's standard Input Manager.
public static class GameInput
{
    public static Vector2 Move()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
}
