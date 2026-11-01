
using UnityEngine;
using UnityEngine.InputSystem;

public class GerakWASD : MonoBehaviour
{
    public float kecepatan = 3f;

    void Update()
    {
        if (Keyboard.current == null)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed)
            horizontal = -1f;

        if (Keyboard.current.dKey.isPressed)
            horizontal = 1f;

        if (Keyboard.current.wKey.isPressed)
            vertical = 1f;

        if (Keyboard.current.sKey.isPressed)
            vertical = -1f;

        Vector3 gerakan = new Vector3(
            horizontal,
            0f,
            vertical
        );

        transform.Translate(
            gerakan.normalized * kecepatan * Time.deltaTime,
            Space.World
        );
    }
}
