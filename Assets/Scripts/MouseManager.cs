using UnityEngine;

public class MouseManager : MonoBehaviour
{
    private Vector3 dragOrigin;
    public Camera mainCamera;
    public float speed = 1.5f;
    public BoxCollider boxCollider;


    private void Start()
    {
        // Set initial position and rotation of camera
        mainCamera.transform.position = new Vector3(0, 20,-30);
        mainCamera.transform.rotation = Quaternion.Euler(36.0f, 180.0f, 0.0f);
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Input.mousePosition;
            return;
        }
    
        if (!Input.GetMouseButton(1)) return;

        // on right mouse button hold
        moveCameraInBoundary();
    }

    private void moveCameraInBoundary()
    {
        Vector3 pos = mainCamera.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(
            pos.x * speed,
            0,
            pos.y * speed
        );
    
        mainCamera.transform.Translate(move, Space.World);

        Vector3 newPos = mainCamera.transform.position;
        Bounds areaBounds = boxCollider.bounds;

        mainCamera.transform.position = new Vector3(
            Mathf.Clamp(newPos.x, areaBounds.min.x, areaBounds.max.x),
            newPos.y,
            Mathf.Clamp(newPos.z, areaBounds.min.z, areaBounds.max.z)
        );
    }
}
