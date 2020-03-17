using UnityEngine;

public class CameraController : MonoBehaviour
{
    [System.NonSerialized] public Transform target;
    private Vector3 startOffset;
    private Vector3 offset;

    private void Awake()
    {
        startOffset = Camera.main.transform.position;
        offset = startOffset;
    }
    public void SetupCamera(Transform focusTarget)
    {
        target = focusTarget;
        transform.position = target.position + offset;
    }
    private void LateUpdate()
    {
        if (target)
        {
            transform.position = target.position + offset;
            //transform.LookAt(target);
            //Debug.DrawRay(transform.position, transform.forward * 100, Color.blue);
        }
    }
    private void FixedUpdate()
    {
        Client.PingToServer();
    }

    public void Rotate(float amount)
    {
        transform.RotateAround(target.position, Vector3.up, amount * Time.deltaTime * 100f);
    }

}
