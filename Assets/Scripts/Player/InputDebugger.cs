using UnityEngine;

public class InputDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.anyKey)
            Debug.Log("KEY PRESSED: " + Input.inputString);

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        if (h != 0 || v != 0)
            Debug.Log($"MOVE: H={h} V={v}");
        if (mx != 0 || my != 0)
            Debug.Log($"MOUSE: X={mx} Y={my}");
    }
}
